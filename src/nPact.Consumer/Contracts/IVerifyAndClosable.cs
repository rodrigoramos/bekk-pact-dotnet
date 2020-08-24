namespace nPact.Consumer.Contracts
{
    public interface IVerifyAndClosable
    {
         int VerifyAndClose(int expectedMatches = 1);
    }
}