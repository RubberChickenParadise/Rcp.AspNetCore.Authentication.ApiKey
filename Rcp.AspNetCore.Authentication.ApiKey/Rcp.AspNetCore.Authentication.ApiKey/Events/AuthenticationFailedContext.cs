// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Rcp.AspNetCore.Authentication.ApiKey.Events
{
    public class AuthenticationFailedContext : ResultContext<ApiKeyAuthenticationOptions>
    {
        public AuthenticationFailedContext(HttpContext                 context,
                                           AuthenticationScheme        scheme,
                                           ApiKeyAuthenticationOptions options)
            : base(context,
                   scheme,
                   options)
        {
        }

        public Exception Exception { get; set; }
    }
}
