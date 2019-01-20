// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Rcp.AspNetCore.Authentication.ApiKey.Events;

namespace Rcp.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        ///     Sets if the Authentication handler should log information level messages
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        ///     The object provided by the application to process events raised by the api key authentication handler.
        ///     The application may implement the interface fully, or it may create an instance of ApiKeyEvents
        ///     and assign delegates only to the events it wants to process.
        /// </summary>
        public new ApiKeyAuthenticationEvents Events
        {
            get => (ApiKeyAuthenticationEvents) base.Events;

            set => base.Events = value;
        }

        /// <summary>
        ///     Http header to find the api key in
        /// </summary>
        public string HttpHeader { get; set; } = "X-API-KEY";

        /// <summary>
        ///     Type of Authorization scheme
        ///     &lt;Header&gt;: &lt;Scheme&gt; &lt;Credentials&gt;
        /// </summary>
        public string Scheme { get; set; } = string.Empty;
    }
}
