# External Authentication
---

This lab is based on a simple movie review website.
It allows customers to browse and search movies and view movie reviews.
It also allows reviewers to create and edit movie reviews.

## Overview

In this lab you will remove the local authentication in the movie review application and change it to use external authentication.
For the first part, you will use the OIDC protocol and use IdentityServer as the provider.
For the second part, you will use social media accounts as the provider.

__Lab Note__: For this lab you will need to keep two solutions open: one for IdentityServer and another for the movie review client application. 
IdentityServer will need to be running for the client application to authentiate users.

## Part 1: Setting up IdentityServer

Since we need IdentityServer running, in this part we will simply open the solution and launch the server.

* Open the "IdentityServerHost" solution from the `~/before` folder. Inspect the structure of the project.
* Open `~/Program.cs`.
    * Notice how we have a file-based logger configured for `"identityserver4_log.txt"`.
* Open `~/Config.cs`. This contains the main configuration for IdentityServer.  
    * Notice the identity resources. This represents the identity data that IdentityServer protects.
    * Notice the clients. This represents the client applications that are using IdentityServer.
    * Notice the users. This represents the centralized identity data for the users.
* Open `~/Startup.cs`.
  * Find `ConfigureServices` and notice how IdentityServer is being setup in DI.
  * Find `Configure` and notice how IdentityServer is being added as middleware. 

**If at any time using IdentityServer you experience problems, the log file is where to look to find out what's wrong.**

* Launch the server and you should see the IdentityServer welcome page. If you see it, then it's running and you can now configure your client application to use it.
* As you work on the rest of this lab, you will want to leave this solution open so it can continue to run while you work on the client application.

## Part 2: Using OIDC from the movie review application

In this part you will change the movie review application to use IdentityServer for signing in users.

* Open the "MoviesWebApp" from the `~/before` folder.
* Open `~/Startup.cs` and locate where the authentication services are added to DI in `ConfigureServices`.
* Add the OpenID Connect authentication handler to DI.
    * Use the `AddOpenIdConnect` extension method chained to the `AddAuthentication` method.
    * Use `"oidc"` as the scheme name.
    * For the `OpenIdConnectOptions`:
        * Set the authority to `"http://localhost:1941/"` (which if you noticed is the URL for IdentityServer).
        * Disable the `RequireHttpsMetadata` property.
        * Set the client id to `"movie_client"`.
        * Set the response type to `"id_token"`.
        * Set the scopes to `"openid"`, `"profile"`, and `"email"`.

```
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "http://localhost:1941/";
    options.RequireHttpsMetadata = false;

    options.ClientId = "movie_client";
    options.ResponseType = "id_token";

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
}
```

* Next, in the `AccountController` change the code that triggers login.
    * Change the `Login` action method to trigger a challenge result and pass `"oidc"` as the authentication scheme to use the OpenId Connect middleware.
    Also pass a `AuthenticationProperties` and set the `RedirectUri` to be either the `returnUrl` parameter or `"/"` if there is no `returnUrl`.

```
[HttpGet]
public IActionResult Login(string returnUrl)
{
    return Challenge(new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/"
    }, "oidc");
}    
```

* Now run the movie review client application. 
Test the login by clicking the "Login" link in the top right of the page.
You should see the user redirected to IdentityServer and you can use one of the username/passwords you saw configured in the `Config.cs` file.

* Notice how when the user is logged in the name is not being shown in the menu. 
This is because the `"name"` claim is not being used for the `Username` property from the claims identity class.
Change the OpenID Connect middleware to use the `"name"` and `"role"` claims by setting the `TokenValidationParameters` on the `OpenIdConnectOptions`.

```
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "http://localhost:1941/";
    options.RequireHttpsMetadata = false;

    options.ClientId = "movie_client";
    options.ResponseType = "id_token";

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
})
```

* Now, run the application and visit a movie details page. 
Notice the review buttons are not enabled. 
This is because the only claims that are known are from IdentityServer. 
The application specific claims are no longer in the cookie.
Add them now.
    * Assign the `Events` property on the `OpenIdConnectOptions`.
    * Handle the `OnTicketReceived` event.
    * Resolve the `MovieIdentityService` from the DI system (on `HttpContext.RequestServices` on the notification context).
    * Invoke `GetClaimsForUser` passing the `"sub"` claim from the `Principal` on the notification context.
    * Add the returned claims to the `Principal.Identities.First()` claims identity object.
    * Return `Task.CompletedTask` from the method.

```
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "http://localhost:1941/";
    options.RequireHttpsMetadata = false;

    options.ClientId = "movie_client";
    options.ResponseType = "id_token";

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };

    options.Events = new OpenIdConnectEvents
    {
        OnTicketReceived = n =>
        {
            var idSvc =
                n.HttpContext.RequestServices.GetRequiredService<MovieIdentityService>();

            var appClaims = 
               idSvc.GetClaimsForUser(n.Principal.FindFirst("sub")?.Value);

            n.Principal.Identities.First().AddClaims(appClaims);

            return Task.CompletedTask;
        }
    };
})
```

* Run the application, re-login, and now all of the authorization should behave the same as it did before.


## Part 3: Add Google authentication to the movie review application 

In this part, we want to allow social logins to the movie review application, so we will add Google logins.

* Add the Google authentication handler after the OpenID Connect handler.

```
AddGoogle("Google", options =>
{
    options.ClientId = "998042782978-lrga3i7tf8g6eotqv3ltjhqd2bguhnf4.apps.googleusercontent.com";
    options.ClientSecret = "lAVx368q3GDXZS_dlrrntrDN";
})
```

* Re-run the movie review application and trigger a login.
Did you see Google's login page? 
Probabaly not; 
that's because the Google middleware is not being triggered because the `Login` action method is using `"oidc"` as the scheme.
* Fix this by changing `Login` to use the `"Google"` as the scheme.

```
[HttpGet]
public IActionResult Login(string returnUrl)
{
    return Challenge(new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/"
    }, "Google");
}
```

* Now run it again, and see if you can login with a Google account.

So now that you have Google enabled you should have come to the realization that we have lost the ability to login with our OpenID Connect provider (IdentityServer).
In the next lab we will solve this problem.
