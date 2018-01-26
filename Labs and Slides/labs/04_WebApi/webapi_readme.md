# Web APIs
---

This lab is based on a simple movie review website.
It allows customers to browse and search movies and view movie reviews.
It also allows reviewers to create and edit movie reviews.

## Overview

In this lab the movie review logic has been split into two projects: one for the back-end movie review logic as a web API, and one for the front-end movie review UI as a web application.
The web API will require access tokens to use its functionality, and the movie review web app will obtain access tokens and pass them to the web API.
IdentityServer will be used to issue this access token to the movie review web application.

__Lab Note__: For this lab you will need to keep three solutions open: one for IdentityServer, one for the movie review web API, and another for the movie review client application.

## Part 1: Configuring the movie review web API to accept access tokens 

Much of the movie review functionality has been reworked into a web API. 
This includes the data access and the authorization logic.
In this part you will get familiar with the web API and then configure it to accept access tokens from IdentityServer.

* Open the "MoviesWebApi" solution from `~/before`.
* Spend some time examining how the web API project is structured.
  * Open `~/Startup.cs`.
    * Find the `ConfigureServices` method and notice `AddMoviesLibrary` is still being used to register the movie services in DI. Also notice the authorization system is configured the same as it was in the prior labs.
    * Find the `Configure` method and notice the call to `UseAuthentication` has been added to include the authentication middleware in the pipeline.
  * Inspect the code in `~/Controllers`. 
    This is the web API code to access the movie review functionality. 
    * Notice how the authorization logic from the prior labs has been adapted to the web API project. 

We need to configure this web API project to accept tokens from IdentityServer.

* Add a NuGet package reference to `IdentityServer4.AccessTokenValidation`.
* Open `~/Startup.cs` and find the `ConfigureServices` method.
* Add a call to `AddAuthentication` to add the authentication services.
   * Set `"Bearer"` as the default scheme.
* Using the fluent API, call `AddJwtBearer` to add JWT support.
  * On the options set:
    * `Authority` to `"http://localhost:1941/"` which is the URL for IdentityServer.
    * `RequireHttpsMetadata` to `false` since we're not using HTTPS for IdentityServer.
    * `ApiName` to `"movie_api"` which will be the name representing this web API for audience validation.

```
services.AddAuthentication("Bearer")
    .AddIdentityServerAuthentication("Bearer", options =>
    {
        options.Authority = "http://localhost:1941/";
        options.RequireHttpsMetadata = false;
        options.ApiName = "movie_api";
    });
```

* Build and run the movie web API project to ensure it's working. 

Next we will configure IdentityServer for this web API.

* Open the "IdentityServerHost" solution from `~/before`.
* Open `~/Config.cs` and find the `ApiResources` configuration.
* Add a new `ApiResource` and to the constructor pass:
  * `name` to `"movie_api"`.
  * `displayName` to `"Movie Review Service"`.
* Also set the `UserClaims` collection and add `"role"` since our API requires the user's role claim for its authorization logic.

```
new ApiResource("movie_api", "Movie Review Service")
{
    UserClaims = { "role" }
}
```

* Build and run IdentityServer to ensure it's working.

## Part 2: Update movie review web app to obtain and use access tokens for the movie review web API 

In this part you will update the movie review web application to obtain access tokens on behalf of the user 
and then use the access token to call the web API.

* Open the "MoviesWebApp" solution from `~/before`.
* Spend some time examining how the web app is now structured.
  * Open `~/Startup.cs` and find the `ConfigureServices` method.
    * Notice the call to `AddMoviesLibrary`. This now accepts a URL for configuring where to call the web API.
    * Notice the registration of the `AddAccessTokenAccessor` class. This is used by the "MoviesLibrary" code to obtain the access token to use when calling the web APIs.
  * Open `~/AccessTokenAccessor.cs` and find the `GetAccessTokenAsync` method. 
    * Notice it returns `null` for the access token (for now).
  * In the "MoviesLibrary" project open `~/MovieService.cs`.
    * Notice the constructor which accepts an `IAccessTokenAccessor`.
    * Notice the `GetHttpClientAsync` method which creates the `HttpClient` to call the web API.
      It uses the `IAccessTokenAccessor` for setting the "Bearer" token (if present).
    * Notice the other methods in this class using the `HttpClient` and how they are now checking for 401 and 403 status codes, and returning an `AccessDenied` result.
    * Notice the code in `~/ReviewService.cs` is similar.
  * Back in the "MoviesWebApp" project open `~/Controllers/MovieController.cs`.
    * Notice how the `MovieService` is used in the various action methods. 
      The code is checking for `IsAccessDenied` and calling `Forbid`, as the authorization logic now resides in the web API project. 

Now that you've looked over the restructured code, we can run it and see how it works without access tokens.

* Run the movie web application and login.
* Once logged in, you should see on the home page the list of claims for the user.
 The access token is listed in the list of claims, and at this point should be empty.
 This shows that we do not currently have an access token.
 If you try to use any of the functions in the app you should get the "Access Denied" message.

Next you will change the movies web app to request an access token.

* Open `~/Startup.cs` and find the `ConfigureServices` method.
* Locate the call to `AddOpenIdConnect`.
* Change the following properties on the `OpenIdConnectOptions`:
  * `ResponseType` should now be `"code id_token"`.
  * `Scope` should now include `"movie_api"`.
  * Add a new property `ClientSecret` and set it to `"secret"`.
  * Set `GetClaimsFromUserInfoEndpoint` to `true`.

```
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "http://localhost:1941/";
    options.RequireHttpsMetadata = false;

    options.ClientId = "movie_client";
    options.ClientSecret = "secret";

    options.ResponseType = "code id_token";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("movie_api");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
})
```

Next, we need to change the configuration in IdentityServer to reflect these changes.

* Back in the "IdentityServerHost" project, open `~/Config.cs`.
* Find the `Clients` configuration.
* For the `"movie_client"` entry, change these properties:
  * Change `AllowedGrantTypes` to `GrantTypes.Hybrid`.
  * Set the `ClientSecrets` collection, and add a `Secret` passing `"secret".Sha256()` to the constructor. 
      IdentityServer assumes that secrets stored are hashed, thus the need for the `.Sha256()` on the value.
  * Add `"movie_api"` to the `AllowedScopes`.

```
new Client
{
    ClientId = "movie_client",
    ClientName = "Moive Review App",
    AllowedGrantTypes = GrantTypes.Hybrid,
    ClientSecrets = 
    {
        new Secret("secret".Sha256())
    },
    RedirectUris = 
    {
        "http://localhost:32361/signin-oidc"
    },

    PostLogoutRedirectUris = 
    {
        "http://localhost:32361/signout-callback-oidc"
    },

    AllowedScopes = 
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        IdentityServerConstants.StandardScopes.Email,
        "movie_api"
    }
}
```

* Re-run IdentityServer with the new configuration changes.
* Re-run the movie web app and login. You should now see an `access_token` on the home page with a value.

 At this point, we're still not using the access token, so we'll do that now.

* Back in the "MoviesWebApp" project open `~/AccessTokenAccessor.cs` and find the `GetAccessTokenAsync` method.
* Change this method to get the access token by calling the `GetTokenAsync` extension method on the `HttpContext`. 
  * Pass `"access_token"` as the parameter to `GetTokenAsync` extension method.
  * Use the access token as the return value from this method.

```
public Task<string> GetAccessTokenAsync()
{
    return _context.HttpContext.GetTokenAsync("access_token");
}
```
* Run the movie web app and login.
 You should now be able to view the list of movies and use the other features of the movie review application. 


## Part 3: [Challenge] Use refresh tokens

At some point the access token that's being used by the movie web app will expire. 
This will cause the web API to fail with a 401. 
To allow the movie review app to continue to use the web API, you will need to use refresh tokens.
Your challenge is to use refresh tokens -- see if you can get it working!

_Hint_: This will involve requesting the `offline_access` scope, using the `GetTokenAsync("refresh_token")` extension method, and contacting the token endpoint in IdentityServer. 
You might want to use the `TokenClient` class in the "IdentityModel" Nuget package for accessing the token endpoint.
This also might involve reworking the "MoviesLibrary" to distinguish between "access denied" and "forbidden" for security failures.
