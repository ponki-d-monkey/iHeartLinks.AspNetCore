namespace iHeartLinks.AspNetCore.LinkKeyProcessors
{
    public interface ILinkKeyProcessor
    {
        LinkKey Process(string key);
    }
}
