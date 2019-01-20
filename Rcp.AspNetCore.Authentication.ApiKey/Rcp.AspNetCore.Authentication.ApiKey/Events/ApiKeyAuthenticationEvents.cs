// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Rcp.AspNetCore.Authentication.ApiKey.Events
{
    public class ApiKeyAuthenticationEvents
    {
        /// <summary>
        ///     A delegate assigned to this property will be invoked when the authentication fails.
        /// </summary>
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } =
            context => Task.CompletedTask;

        /// <summary>
        ///     A delegate assigned to this property will be invoked when the credentials need validation.
        /// </summary>
        /// <remarks>
        ///     You must provide a delegate for this property for authentication to occur.
        ///     In your delegate you should construct an authentication principal from the user details,
        ///     attach it to the context.Principal property and finally call context.Success();
        /// </remarks>
        public Func<ValidateCredentialsContext, Task> OnValidateCredentials { get; set; } =
            context => Task.CompletedTask;

        public virtual Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            return OnAuthenticationFailed(context);
        }

        public virtual Task ValidateCredentials(ValidateCredentialsContext context)
        {
            return OnValidateCredentials(context);
        }
    }
}
