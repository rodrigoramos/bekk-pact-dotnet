using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Rendering
{
    class Responder : IDisposable
    {
        private NetworkStream stream;

        public Responder(NetworkStream stream)
        {
            this.stream = stream;
        }
        public void Respond(IPactResponseDefinition response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if(stream == null) throw new InvalidOperationException("Stream is disposed.");
            const string crlf = "\r\n";
            using (var writer = new StreamWriter(stream, Encoding.ASCII))
            {
                var status = (HttpStatusCode) response.ResponseStatusCode.GetValueOrDefault(200);
                writer.Write($"HTTP/1.1 {(int)status} {status}{crlf}");
                foreach (var header in response.ResponseHeaders)
                {
                    writer.Write($"{header.Key}: {header.Value}{crlf}");
                }
                writer.Write(crlf);
                if(response.ResponseBody!=null)
                {
                    writer.Write(response.ResponseBody.Render());
                }
                writer.Flush();
                writer.Dispose();
            }
        }

        public void Dispose()
        {
            stream?.Flush();
            stream?.Dispose();
            stream = null;
        }
    }
}