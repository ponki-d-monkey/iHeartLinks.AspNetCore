namespace iHeartLinks.AspNetCore.LinkRequestProcessors
{
    public interface ILinkRequestProcessor
    {
        LinkRequest Process(string request);
    }
}
