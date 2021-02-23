using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore
{
    public interface IUrlHelperBuilder
    {
        IUrlHelper Build();
    }
}
