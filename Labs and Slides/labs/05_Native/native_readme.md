# Native client applications
---

This lab is based on a native console application to search the movie database (via the movie web api).

## Overview

In this lab the client application is a native, cross-platform console application.
The pertinent steps in this lab will be the same if you're building a Windows or macOS desktop application, 
and are no different than if you were to use a platform-specific UI framework to build your application (WinForms, WPF, MacApp, Cocoa, GTK#, etc).

__Lab Note__: For this lab you will need to keep three solutions open: one for IdentityServer, one for the movie review web API, and another for the movie search console application.

## Part 1: Obtaining access tokens using resource owner password flow

In this part you will configure the console movie client to get tokens via the resource owner password flow.
This flow is commonly used when migrating legacy applications to OAuth2 for security.
This flow involves prompting the user for credentials and those credentials are used to get a token from the token endpoint.
To assist with the protocol details we will be using the _IdentityModel_ NuGet package.

To get started, let's take a look at the console movie application.

* Open the "MoviesNativeClientApp" solution from `~/before`.
* Notice the `const string authorityUrl` variable at the top of the `Program` class. 
 This is the URL for IdentityServer.
* Open `~/Program.cs` and find `MainAsync`.
  * Notice the `PromptForPasswordAsync` is the helper to get a token. The token is then used in `RunSearchLoopAsync`.
* Examine `RunSearchLoopAsync` to get an idea how the token is being used, what web API being invoked, and how the results are being processed.
* Examine `PromptForPasswordAsync` and notice that there are a few `TODO`s to fill in.

You will now fill in those `TODO`s.

* In the `PromptForPasswordAsync` method:
  * Create an instance of `DiscoveryClient` using the `authorityUrl`.
    * Use its `GetAsync` to load the metadata for IdentityServer.  
  * Use the `TokenClient` class to obtain an access token.
    * Create an instance with: 
      * The `TokenEndpoint` from the metadata for the address.
      * `"native_client"` as the client id.
    * Use `RequestResourceOwnerPasswordAsync` to obtain an access token.
      * Pass the `username` and `password`.
      * Also pass `"movie_api"` as the scope.
    * If the result's `IsError` is `true`, show the error and return from the method. 
    * Return the result's `AccessToken` from the method if it's successful.

```
private static async Task<string> PromptForPasswordAsync()
{
    Console.WriteLine("Enter your credentials.");
    Console.Write("Username: ");
    var username = Console.ReadLine();
    Console.Write("Password: ");
    var password = Console.ReadLine();

    var discoveryClient = new DiscoveryClient(authorityUrl);
    var discoveryResult = await discoveryClient.GetAsync();

    var tokenClient = 
        new TokenClient(discoveryResult.TokenEndpoint, "native_client");
    var tokenResult =
        await tokenClient.RequestResourceOwnerPasswordAsync(username, password, "movie_api");

    if (tokenResult.IsError)
    {
        Console.WriteLine($"Error: {tokenResult.Error}");
        if (tokenResult.ErrorDescription != null)
        {
            Console.WriteLine($"Description: {tokenResult.ErrorDescription}");
        }
    }

    Console.WriteLine();

    return tokenResult.AccessToken;
}
```

Next, we need to configure this client in IdentityServer.

* Open the "IdentityServerHost" solution from `~/before`.
* Open `~/Config.cs` and find the `Clients` configuration.
* Add a new `Client` to the list and set:
  * `ClientId` to `"native_client"`.
  * `ClientName` to `"Console Movie Search"`.
  * `AllowedGrantTypes` to `GrantTypes.ResourceOwnerPassword`.
  * `RequireClientSecret` to `false`.
  * Set the `AllowedScopes` collection, and add `"movie_api"` to the list.

```
new Client
{
    ClientId = "native_client",
    ClientName = "Console Movie Search",
    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
    AllowedScopes = { "movie_api" },
    RequireClientSecret = false,
},
```

* Run the IdentityServer project to ensure it's working.

Next, we need to make sure the web API is running.

* Open the "MoviesWebApi" solution from `~/before`.
* Run the project so it's available for the console client.

Finally, we can run the client.

* Back in the console movie client, run the application, and login.
One thing you might notice is that if you use _user1_, _user2_, or _user3_ they will be denied access due to the "SearchPolicy" in the web api authorization.
You will need to login as _user4_ or _user5_ to use the search function.

You now have a native application that can get an access token for an API.
The only problem is that any users that require federated authentication (social media logins or other external logins like AAD, ADFS, Windows, Auth0, Ping Federate, etc) will not work.
Federated logins require a browser, and you will solve that in the next part.  

## Part 2: Obtaining access tokens using code flow with the system browser

In this part you will use the system browser to get access tokens.
This involves using the hybrid flow, which means some activity will happen in the browser and some activity will be done via the token endpoint.
Also, since we're using the system browser the user will benefit from SSO, as well as any password managers they have installed in the system browser.
But, given that the system browser is a shared execution environment, we should (and will) use PKCE to protect the application and user from any malicious agents that might intercept the browser traffic. 

To get started, we need to change the console movie client's configuration in IdentityServer.

* Back in the "IdentityServerHost" project open `~/Config.cs` and find the `Clients` configuration.
* Change the `Client` configuration for `"native_client"`:
  * `AllowedGrantTypes` to `GrantTypes.Hybrid`.
  * Add `"openid"` to the `AllowedScopes` collection.
  * `RequirePkce` to `true`.
  * Set the `RedirectUris` collection, and add `"http://127.0.0.1:12345/native_client_callback"` to the list.

```
new Client
{
    ClientId = "native_client",
    ClientName = "Console Movie Search",
    AllowedGrantTypes = GrantTypes.Hybrid,
    RequirePkce = true,
    RedirectUris = 
    {
        "http://127.0.0.1:12345/native_client_callback"
    },
    AllowedScopes = { "openid", "movie_api" },
    RequireClientSecret = false,
}
```

Next, change the console movie client to use the browser.

* Back in the "MoviesNativeClientApp" project open `~/Program.cs` and find the `MainAsync` method.
* Change the code so that the `accessToken` is now coming from the `LaunchBrowserForTokenAsync` method.

```
//var accessToken = await PromptForPasswordAsync();
var accessToken = await LaunchBrowserForTokenAsync();
```

Next, change how we request tokens. We will use the `OidcClient` class from the _IdentityModel.OidcClient_ NuGet package.

* Find the `LaunchBrowserForTokenAsync` method.
    * Notice the creation of the `SystemBrowser` class.
    This class will launch the system browser when we need the user to login.
    It also encapsulates a HTTP listener (using ASP.NET Core) on the loopback address `"http://127.0.0.1:12345/native_client_callback"` to handle the response from IdentityServer.
* After the creation of the `SystemBrowser`, create an instance of `OidcClientOptions`. 
This is used to model the client OpenID Connect configuration. 
Configure it as such:
  * `authorityUrl` as the authority.
  * `"openid movie_api"` for the scope.
  * `"http://127.0.0.1:12345/native_client"` for the redirect URI.
  * `"native_client"` for the client id. 
  * `browser` as the browser.

```
var browser = new SystemBrowser(12345, "/native_client_callback");

var opts = new OidcClientOptions
{
    Authority = authorityUrl,
    Scope = "openid movie_api",
    RedirectUri = "http://127.0.0.1:12345/native_client_callback",
    ClientId = "native_client",
    Browser = browser,
};
```

Next we can create the `OidcClient`. 

* Create an instance of `OidcClient` and pass the `OidcClientOptions` to the constructor.

```
var client = new OidcClient(opts);
```

Now we should be able to launch the browser. 

* Call `LoginAsync` on the `OidcClient`.
  * If the result is an error, show the error and return `null` from this method.
  * If the result is successful, then return the `AccessToken` from this method.

```
var client = new OidcClient(opts);
var request = new LoginRequest();
var result = await client.LoginAsync(request);
if (result.IsError)
{
    Console.WriteLine($"Error: {result.Error}.");
    return null;
}

return result.AccessToken;
```

You should now be able to use the hybrid flow to get an access token.

* Run the console movie client and login to the browser.
If successful, the browser should inform you that you can return to the application.
The console app should then be able to search movies.

## Part 3: [Challenge] Access token expiration

As always, you need to think about access token expiration.
Think about how to get a refresh token and use it to get new access tokens.
Also, think about where you would store the refresh token.
Feel free to implement your ideas.
