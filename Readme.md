# Rcp.AspNetCore.Authentication.ApiKey

This project is an implementation of using Api Keys for ASP.NET Core. 

It was created because I needed to use Api Keys to secure an api endpoint for use by external customers.  Since I had
most of the work done, I took it the extra step to make the library.

This, however, is open to Man In The Middle attacks or sniffing or any other technique that can see the http headers
on an unencrypted connection.  Please use HTTPS to prevent leaking of credentials.

## Getting started

Get an https cert, [LetsEncrypt](https://letsencrypt.org/) if you need a free one so there is no excuse, and apply it
to your server.

Add a reference to the package in your web app, then in the `ConfigureServices` method in `startup.cs` add
`app.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme).UseApiKey(...);` with your options.  The only
required delegate is `OnValidateCredentials` to validate any Api Key provided with the request.  The delegate must create
set the value of `context.Principal` to a new `ClaimsPrincipal` then call `context.Success()`.

After setting up `ConfigureServices`, add `app.UseAuthentication();` in the `Configure` method in `startup.cs` or nothing
will work.  Ask me how I know. (On second though, don't.  It's embarrassing.)

Example:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(
        x =>
        {
            x.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        }).AddApiKey(options =>
            {
                options.EnableLogging = false;
                options.Events = new ApiKeyAuthenticationEvents
                    {
                        OnValidateCredentials = async context =>
                            {
                                if (context.ApiKey == "abc123")
                                {
                                    context.Principal = new
                                        ClaimsPrincipal(new ClaimsIdentity(new []
                                            {
                                                new Claim("TestClaim", "Test"),
                                            },
                                            context.Scheme.Name,
                                            "TestUser",
                                            "user"));

                                    context.Success();
                                }
                                else
                                {
                                    context.Fail(new AuthenticationException());
                                }
                            }
                    };
                });
    
    // Other service configuration.
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseAuthentication();

    // Other app configuration.
}
```

In the sample you can see the Api Key that was sent is available in the context.ApiKey which is then checked.  If the key
matches the expected value then the login is valid and a set of `Claim`s are created, added to a `ClaimsIdentity`, which
is then added to a `ClaimsPrincipal`, which is assigned to the context and will be used further in the `AuthorizationHandler`.

After creating and assigning the `ClaimsPrincipal` the delegate calls `context.Success()`.  If this is not called then
the `AuthorizationHandler` will assume the authorization failed and return an HTTP 401 error.  (Again, don't ask me how I know.)

## Other Options

Since I am a bit weird and dont like to use the `Authorization` header with a scheme, I default the library to my preffered
method: a custome header of `X-API-KEY`.  However you can use the `Authorization` header (or any other custom header) by
setting `ApiKeyAuthenticationOptions.HttpHeader` to the prefered the prefered header value.  If you want to use a Scheme
identifier setting the `ApiKeyAuthenticationOptions.Scheme` will let you use the `Header: Scheme Key` format.

```c#
//Other Code

.AddApiKey(options =>
    {
        //Other Options
        options.HttpHeader = "Authorization",
        options.Scheme = "ApiToken"
    });

//Other Code
```

The last option is to enable informational logging by setting `ApiKeyAuthenticationOptions.EnableLogging`.  This will be
information on every login request and can be used to detect attacks if there are multiple failed attempts in a row.

```c#
//Other Code

.AddApiKey(options =>
    {
        //Other Options
        options.EnableLogging = false
    });

//Other Code
```

## Using IOC in your OnValidateCredentials delegate

In prod you will probably be using some sort of backing store.  Since we are in the Asp.Net Core middleware pipeline
we have the ability to access configured services with `context.HttpContext.RequestServices.GetService<T>();`
This is useful as we dont have to have a giant delegate with calls to new all over the place.  It is also nice to 
break out the logic.

```c#
options.Events = new ApiKeyAuthenticationEvents
    {
        OnValidateCredentials = async context =>
            {
                var validationService = context.HttpContext.RequestServices.GetService<IKeyValidationService>();
                var claimsService = context.HttpContext.RequestServices.GetService<IKeyClaimsService>();

                if (validationService.IsKeyValid(context.ApiKey)
                {
                    var claims = claimsService.GetClaims(context.ApiKey);

                    context.Principal = new
                        ClaimsPrincipal(new ClaimsIdentity(
                            claims,
                            context.Scheme.Name,
                            "TestUser",
                            "user"
                    context.Success();
                }
                else
                {
                    context.Fail(new AuthenticationException());
                }
            }
    };
```

## Thoughts on using this in production

Since api keys are like a username and password all in one they may not be the best option but they are a 
simple option for authorization.  They have fallen out of favor with [Json Web Tokens](https://jwt.io/) or [Oauth](https://oauth.net/2/) taking their
place.  But if you need quick simple authorization they are still a very valid option.  Just take the
following into consideration to harden the service:

1. If used over a non secure connection, the only thing an attacker needs to use your api is in the clear and sniffable. (Please dont be horrible person, SSL certs are free as in beer now days.)
    a. [Let's Encrypt](https://letsencrypt.org/)
    b. [SSL For Free](https://www.sslforfree.com/) rides on top of Let's Encrypt
2. Add exponential lockout of IP addresses after multiple failed attempts will help prevent brute force guessing of api keys
    a. after three incorrect api keys from an ip address lockout time is 30 seconds
    b. if the key gets locked out again, lockout time is 1 min
    c. if the key gets locked out again again, lockout time is 2 min
    d. if the key gets locked out again again again, lockout time is 4 min
    e. if the key gets locked out again again again again ..... you get the picture.
2. Have a white list of IP addresses that dont get locked out.  This can keep you from being locked out at the office if a Dev is accidently testing against production (Again, don't ask how I know.)
3. If you are using https (because your not a horrible person) make sure all the requests use it.  Its easy and you only have to do it once.
    
    ```c#
    services.Configure<MvcOptions>(options =>
    {
        options.Filters.Add(new RequireHttpsAttribute());
    });
    ```

## Final notes

Using api keys can possibly cause issues since they are validated on every request.  If you run into this you can use
some sort of caching of successful keys.  Check out [Rcp.Caching.CachingManager] (https://github.com/RubberChickenParadise/Rcp.Caching.CachingManager) for an option that has some optimizations
 around multiple cache misses at the same time in a multithreaded application (IE: a web app).

If you dont feel like writing your own exponential lockout for IP addresses, use mine [Rcp.Security.BackOff](https://github.com/RubberChickenParadise/Rcp.Security.BackOff)

## Framework versions supported

Currently this only supports .net Standard 2.0 an on.