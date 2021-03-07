﻿using System;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class EmptyBaseUrlProvider : IBaseUrlProvider
    {
        public Uri Provide()
        {
            return new Uri("", UriKind.Relative);
        }
    }
}
