using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace iHeartLinks.AspNetCore
{
    public sealed class UrlHelperBuilder : IUrlHelperBuilder
    {
        private readonly IActionContextAccessor actionContextAccessor;
        private readonly IUrlHelperFactory urlHelperFactory;

        public UrlHelperBuilder(
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
            this.actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
            this.urlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        public IUrlHelper Build()
        {
            var actionContext = actionContextAccessor.ActionContext;

            return urlHelperFactory.GetUrlHelper(actionContext);
        }
    }
}
