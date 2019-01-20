// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;

namespace Rcp.AspNetCore.Authentication.ApiKey
{
    public static class ApiKeyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
        {
            return builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationScheme);
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder,
                                                      string                     authenticationScheme)
        {
            return builder.AddApiKey(authenticationScheme,
                                     null);
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder          builder,
                                                      Action<ApiKeyAuthenticationOptions> configureOptions)
        {
            return builder.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationScheme,
                                     configureOptions);
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder          builder,
                                                      string                              authenticationScheme,
                                                      Action<ApiKeyAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme,
                                                                                               configureOptions);
        }
    }
}
