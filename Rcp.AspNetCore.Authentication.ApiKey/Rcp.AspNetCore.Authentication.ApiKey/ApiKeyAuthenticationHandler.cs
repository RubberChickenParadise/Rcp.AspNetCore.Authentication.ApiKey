// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Rcp.AspNetCore.Authentication.ApiKey.Events;

namespace Rcp.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string AuthenticationFailed =
            "Authentication Failed for {Method} {Url} from {IpAddress} because of {Reason}";

        private const string AuthenticationSuccessful =
            "Authentication succeeded for {Method} {Url} from {IpAddress} for {Name}";


        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                           ILoggerFactory                               logger,
                                           UrlEncoder                                   encoder,
                                           ISystemClock                                 clock)
            : base(options,
                   logger,
                   encoder,
                   clock)
        {
        }

        protected new ApiKeyAuthenticationEvents Events
        {
            get => (ApiKeyAuthenticationEvents) base.Events;
            set => base.Events = value;
        }

        protected override Task<object> CreateEventsAsync()
        {
            return Task.FromResult<object>(new ApiKeyAuthenticationEvents());
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string header = Request.Headers[Options.HttpHeader];

            if (string.IsNullOrWhiteSpace(header))
            {
                Log(LogLevel.Information,
                    AuthenticationFailed,
                    Request.Method,
                    Request.GetDisplayUrl(),
                    Request.HttpContext.Connection.RemoteIpAddress,
                    "No Http Header Sent");

                return AuthenticateResult.NoResult();
            }

            if (!string.IsNullOrEmpty(Options.Scheme) &&
                !header.StartsWith(Options.Scheme + ' ',
                                   StringComparison.OrdinalIgnoreCase))
            {
                Log(LogLevel.Information,
                    AuthenticationFailed,
                    Request.Method,
                    Request.GetDisplayUrl(),
                    Request.HttpContext.Connection.RemoteIpAddress,
                    "Security Scheme not presented");

                return AuthenticateResult.NoResult();
            }

            var apiKey = header.Substring(Options.Scheme.Length)
                               .Trim();

            if (string.IsNullOrEmpty(apiKey))
            {
                Log(LogLevel.Information,
                    AuthenticationFailed,
                    Request.Method,
                    Request.GetDisplayUrl(),
                    Request.HttpContext.Connection.RemoteIpAddress,
                    "No Api Key present");

                return AuthenticateResult.Fail("No credentials");
            }

            try
            {
                var validateCredentialsContext = new ValidateCredentialsContext(Context,
                                                                                Scheme,
                                                                                Options)
                                                 {
                                                     ApiKey = apiKey
                                                 };

                await Events.ValidateCredentials(validateCredentialsContext);

                if (validateCredentialsContext.Result != null &&
                    validateCredentialsContext.Result.Succeeded)
                {
                    Log(LogLevel.Information,
                        AuthenticationSuccessful,
                        Request.Method,
                        Request.GetDisplayUrl(),
                        Request.HttpContext.Connection.RemoteIpAddress,
                        validateCredentialsContext.Principal.Identity.Name);

                    return AuthenticateResult.Success(new AuthenticationTicket(validateCredentialsContext.Principal,
                                                                               Scheme.Name));
                }

                if (validateCredentialsContext.Result         != null &&
                    validateCredentialsContext.Result.Failure != null)
                {

                    Log(LogLevel.Information,
                        AuthenticationFailed,
                        Request.Method,
                        Request.GetDisplayUrl(),
                        Request.HttpContext.Connection.RemoteIpAddress,
                        "Validate Credentials returned failure");

                    return AuthenticateResult.Fail(validateCredentialsContext.Result.Failure);
                }

                Log(LogLevel.Information,
                    AuthenticationFailed,
                    Request.Method,
                    Request.GetDisplayUrl(),
                    Request.HttpContext.Connection.RemoteIpAddress,
                    "Authentication hit final no results.  Something may be wrong with the authentication setup");

                return AuthenticateResult.NoResult();
            }
            catch (Exception ex)
            {
                var authenticationFailedContext = new AuthenticationFailedContext(Context,
                                                                                  Scheme,
                                                                                  Options)
                                                  {
                                                      Exception = ex
                                                  };

                await Events.AuthenticationFailed(authenticationFailedContext);

                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                Log(LogLevel.Warning,
                    ex,
                    AuthenticationFailed,
                    Request.Method,
                    Request.GetDisplayUrl(),
                    Request.HttpContext.Connection.RemoteIpAddress,
                    "Authentication failed or an unknown reason");

                throw;
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;

            var headerValue = Options.HttpHeader + $" {Options.Scheme}";

            Response.Headers.Append(HeaderNames.WWWAuthenticate,
                                    headerValue);

            return Task.CompletedTask;
        }

        private void Log(LogLevel        level,
                         string          message,
                         params object[] objects)
        {
            if (!Options.EnableLogging)
            {
                return;
            }

            Logger.Log(level,
                       message,
                       objects);
        }

        private void Log(LogLevel        level,
                         Exception       ex,
                         string          message,
                         params object[] objects)
        {
            if (!Options.EnableLogging)
            {
                return;
            }

            Logger.Log(level,
                       ex,
                       message,
                       objects);
        }
    }
}
