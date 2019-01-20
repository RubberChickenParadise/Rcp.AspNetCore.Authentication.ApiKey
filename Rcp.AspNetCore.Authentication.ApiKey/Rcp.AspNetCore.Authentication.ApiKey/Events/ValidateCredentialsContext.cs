// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Rcp.AspNetCore.Authentication.ApiKey.Events
{
    public class ValidateCredentialsContext : ResultContext<ApiKeyAuthenticationOptions>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ValidateCredentialsContext" />.
        /// </summary>
        /// <param name="context">The HttpContext the validate context applies too.</param>
        /// <param name="scheme">The scheme used when the Basic Authentication handler was registered.</param>
        /// <param name="options">
        ///     The <see /> for the instance of
        ///     <see cref="ApiKeyAuthenticationOptions" /> creating this instance.
        /// </param>
        public ValidateCredentialsContext(HttpContext                 context,
                                          AuthenticationScheme        scheme,
                                          ApiKeyAuthenticationOptions options)
            : base(context,
                   scheme,
                   options)
        {
        }

        public string ApiKey { get; set; }
    }
}
