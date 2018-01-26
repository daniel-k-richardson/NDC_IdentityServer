# Federation Gateway
---

This lab is based on a simple movie review website.
It allows customers to browse and search movies and view movie reviews.
It also allows reviewers to create and edit movie reviews.

## Overview

In this lab you will consolidate all external authentication into a single authentication gateway.
IdentityServer will act as this gateway. 
You will also implement single sign-out which allows the user to sign-out of both the movie web app, and IdentityServer. 

__Lab Note__: For this lab you will need to keep two solutions open: one for IdentityServer and another for the movie review client application. 
IdentityServer will need to be running for the client application to authenticate users.

## Part 1: Move Google authentication from the movie web app to IdentityServer

In this part, you will remove Google authentication from the movie review web app and add it into IdentityServer.

First, remove Google from the movie review app.

* Open the "MovieWebApp" project from `~/before`.
* Open `~/Startup.cs` and find the `ConfigureServices` method.
* Remove the call to `AddGoogle`.
* Open `~/Controllers/Account.cs` and in `Login` change the scheme used for the challenge from `"Google"` to `"oidc"`.

Next, add Google to IdentityServer.

* Open the "IdentityServerHost" project from `~/before`.
* Open `~/Startup.cs` and find the `ConfigureServices` method.
* Add the Google authentication handler.
  * Use the `AddAuthentication` API after the call to `AddIdentityServer`.
  * Use the `AddGoogle` fluent API on the return from `AddAuthentication`.
  * Use `IdentityServerConstants.ExternalCookieAuthenticationScheme` for the `"SignInScheme"`.
  This is the cookie middleware that IdentityServer automaitcally registers for external identity provider logins.
  * Note that the `ClientId` and `ClientSecret` are different than the movie review app since it's hosted at a different URL.  

```
services.AddAuthentication()
.AddGoogle("Google", options =>
{
    options.ClientId = "998042782978-s07498t8i8jas7npj4crve1skpromf37.apps.googleusercontent.com";
    options.ClientSecret = "HsnwJri_53zn7VcO1Fm7THBb";
    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
});
```

* Re-run IdentityServer and the movie review app.
* Trigger a login to the movie review application and you should now be able to use a Google account.

If you're interested in how the users from Google are handled, feel free to inspect the `AccountController` in the IdentityServer project (specifically the `External` and `ExternalCallback` methods).

## Part 2: Single sign-out

In this part you will implement single sign-out. 
This will involve the movie web application making a request to IdentityServer when the user triggers logout.

The first step is to trigger an OpenID Connect sign-out in the movie web app when the user logs out.

* Back in the "MovieWebApp", open `~/Controllers/AccountController.cs`.
* Locate the `Logout` method.

Notice how `Logout` is only triggering sign-out with `"Cookies"`. 
We need to change it to also trigger `"oidc"`, which is the scheme of the OpenID Connect middleware.


Also notice because sign-out is only local the `Logout` method is redirecting back to the home page.
This will not be the workflow anymore, as we will be redirecting to IdentityServer for sign-out.

* Replace the entire `Logout` method with a simpler one that returns `SignOut` passing `"Cookies", "oidc"`. 

```
public IActionResult Logout()
{
    return SignOut("Cookies", "oidc");
}
```

This `SignOutResult` has the side-effect of triggering the redirect to IdentityServer for sign-out, so we no longer need to try to control the redirect.

* Run the application, login, and then logout.
You should be redirected back to IdentityServer for sign-out.

One problem with no longer controlling the redirect is that once a user has signed-out at IdentityServer, they have no easy way to return back to the movie web app.
We will fix this by passing a "post logout redirect uri" to IdentityServer as part of the sign-out process.
If properly configured, IdentityServer will no longer prompt the user to sign-out, and will provide the user a link to return back to the movie web app after they have signed out.

To achieve this, we need to configure the post logout redirect uri for the movie web app in IdentityServer.

* In the "IdentityServer" project open `~/Config.cs`.
* Locate the `Client` configuration for `"movie_client"`.
* Add a new `PostLogoutRedirectUris` property to the `Client`.
  * Add the value `"http://localhost:32361/signout-callback-oidc"` to the list.

```
new Client
{
    ClientId = "movie_client",
    
    // other settings ...

    PostLogoutRedirectUris = 
    {
        "http://localhost:32361/signout-callback-oidc"
    },

    // other settings ...
}
```

Next, we need to configure the movie web app to request a redirect back to the movie web app after the user has signed out.

* In the "MovieWebApp" project open `~/Controllers/AccountController.cs`.
* Locate the `Logout` method.
* Change the call to `SignOut` and pass a `AuthenticationProperties` as the first parameter.
  * Set the `RedirectUri` property to `"/"`.

```
public IActionResult Logout()
{
    return SignOut(new AuthenticationProperties
    {
        RedirectUri = "/"
    }, "Cookies", "oidc");
}
```

Finally, as part of the sign-out protocol we are expected to pass the original `id_token` to IdentityServer.
As of now, we are not keeping track of the `id_token`. 
You can confirm this by looking at the list of claims on the home page of the movie web app.
The `id_token` is shown in the list, but the value should be empty.
We need to keep track of the `id_token` and the OpenID Connect middleware will do this for us.

* Open `~/Startup.cs`.
* Locate the call to `AddOpenIdConnect` in the `ConfigureServices` method.
* On the `OpenIdConnectOptions` set the `SaveTokens` property to `true`.

```
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "http://localhost:1941/";

    // other properties...

    options.SaveTokens = true;

    // other properties...
})
``` 

* Re-run IdentityServer and the movie review app.
* Login to the movie web app, and then logout. 
You should no longer be prompted for signing out, and you should see a link back to the movie review app on the "logged out" page. 

## Part 3: [Challenge] Bypass home realm discovery (HRD)

Sometimes displaying the "home realm discovery" (HRD) page is not desirable. 
IdentityServer supports passing a custom `acr_values` protocol parameter in the form of `"idp:Google"`.
This value would be passed from the client to IdentityServer when triggering authorization.
See if you can customize the OpenID Connect middleware in the movie web app to pass this custom `acr_values` parameter to skip the login page and redirect the user directly to Google.
_Hint_: Look into the `Events` property on the `OpenIdConnectOptions`.

## Part 4: [Challenge] Build custom registration page for external users in IdentityServer

Now that we have Google logins in IdentityServer, any Google user can access the movies application.
Inspect the code in IdentityServer and think about how you would design a registration process for users, or even how would you design a mechanism for existing users to associate a Google login with their local accounts.
