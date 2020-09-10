using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using nPact.Consumer.Contracts;
using nPact.Consumer.Rendering;

namespace nPact.Consumer.Server
{
    class Listener : IDisposable
    {
        private TcpListener listener;
        private Func<IPactRequestDefinition, IPactResponseDefinition> callback;
        private CancellationTokenSource cancellation;
        public void Start(Uri uri, Func<IPactRequestDefinition, IPactResponseDefinition> callback)
        {
            var address = uri.HostNameType == UriHostNameType.IPv4 ? 
                IPAddress.Parse(uri.Host) : 
                Dns.GetHostAddressesAsync(uri.Host).Result[0];
            var port = uri.Port;
            listener = new TcpListener(address, port);
            this.callback = callback;
            cancellation = new CancellationTokenSource();
            Task.Run(() => Listen(uri), cancellation.Token);
        }

        private async Task Listen(Uri baseUri)
        {
            if (listener == null) throw new InvalidOperationException();
            try
            {
                State = ListenerState.Started;
                listener.Start();
                while (State < ListenerState.Cancelled && !(cancellation?.IsCancellationRequested).GetValueOrDefault())
                {
                    State = ListenerState.Listening;
                    await Task.Yield();
                    using (var client = await Task.Run(() => listener.AcceptTcpClientAsync(), cancellation.Token).ConfigureAwait(false))
                    {
                        if (client == null || State == ListenerState.Cancelled) continue;
                        State = ListenerState.Parsing;
                        var stream = client.GetStream();
                            
                        try
                        {
                            byte[] readBuffer = new byte[8096];
                            var request = new StringBuilder();
                            do
                            {
                                var numberOfBytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length);
                                request.Append(Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead));
                            } 
                            while (stream.DataAvailable);
                            var pact = new RequestParser(request.ToString(), baseUri);
                            var response = callback(pact);
                            using (var responder = new Responder(stream))
                            {
                                responder.Respond(response);
                            }
                        }
                        catch(Exception e)
                        {
                            if(stream.CanWrite)
                            using (var responder = new Responder(stream))
                            {
                                responder.Respond(new ExceptionResponse(e));
                            }
                            throw;
                        }
                    }
                }
            }
            catch(ObjectDisposedException)
            {
                if(State < ListenerState.Cancelled)
                {
                    throw;
                }
            }
            catch(Exception e)
            {
                System.Console.WriteLine($"{e.GetType()}: {e.Message}");
                throw;
            }
            finally
            {
                listener.Stop();
                State = ListenerState.Stopped;
                Stopped?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler<EventArgs> Stopped;

        public void Dispose()
        {
            cancellation?.Cancel();
            listener.Stop();
            State = ListenerState.Cancelled;
        }
        public ListenerState State { get; private set; }
        public enum ListenerState
        {
            Created,
            Started,
            Listening,
            Parsing,
            Cancelled,
            Stopped
        }
    }
}