# JavaScript client applications
---

This lab is based on a simple movie review website.
It allows customers to browse movies.

## Overview

In this lab the movie review application has been rewritten as a pure JavaScript-based application.
It won't have as much functionality as the previous labs, but it will suffice to show how to obtain an access token and call a web API.

__Lab Note__: For this lab you will need to keep three solutions open: one for IdentityServer, one for the movie review web API, and another for the movie review client application.

## Part 1: Logging a user into a JavaScript-based application

In this part you will examine the structure of the JavaScript-based application.
You will then modify it (with the help of the [_oidc-client_](https://github.com/IdentityModel/oidc-client) JavaScript library) to perform an OpenID Connect login and obtain an access token to invoke the movie review web API.

* Open the "MoviesWebApp" solution from `~/before`.
* Run the application to see that the home page looks essentially the same as it did in the prior labs.
* Open `~/Startup.cs` and notice how bare the server-side code is. 
  The only server-side code that's being used is the static file middleware to serve up files from the `~/wwwroot` folder.
* Expand the `~/wwwroot` folder and notice the `.html` files. These are the files that now make up this application.
  * Open `~/wwwroot/index.html` to examine the structure. Notice it includes a JavaScript file `oidc-client.js` as well as `site.js`.
    `oidc-client.js` is the _oidc-client_ library to help with the OpenID Connect protocol.
    `site.js` contains the application-specific code. 
  * Open `~/wwwroot/js/site.js` to examine the application code.
    This file contains the logic to dynamically build the UI based on the state of the logged in user and the user's activity. 
    Notice at the top of the file the creation of the `Oidc.UserManager` object. 
    The `UserManager` is the class used to log the user in and out, and query the user's login status (which includes accessing the user's claims and access token). 

The first thing we can do is use the `UserManager` to query the user's status.

* Find the `showLoginStatus` function.
* In the function call `getUser` on the `UserManager` (via the `mgr` variable) to load the currently logged in user.
  * This function returns a JavaScript `Promise`, so to receive the callback you need to use the `then` function on the return value passing a callback function to receive the results.
  * In the `then` callback the `user` will be passed as the parameter. If it is not `undefined` (or _truthy_) then the user is logged in so you can call `showLoggedInUser` passing the user object.
    If the `user` is `undefined` (or _falsy_) then they are not logged in so call the `showAnonymousUser` function.

```
function showLoginStatus() {
    mgr.getUser().then(function (user) {
        if (user) {
            showLoggedInUser(user);
        }
        else {
            showAnonymousUser();
        }
    });
}
```

* Run the application. 
You should now see that the application indicates that the user is not logged in.

Next, configure the application to login using IdentityServer and obtain an access token.

* At the top of `site.js` find the `config` object that's being passed to the `UserManager` constructor.
* Set these properties on the `config` object:
  * `authority` to `'http://localhost:1941/'`, which is the URL of IdentityServer.
  * `client_id` to `'movie_client'`, which is the client id of this application.
  * `redirect_uri` to `'http://localhost:32361/callback.html'`, which is the page in this application to handle the sign in response from IdentityServer. 
  * `post_logout_redirect_uri` to `'http://localhost:32361/index.html'`, which is the page in this application after the user has signed out of IdentityServer. 
  * `response_type` to `'id_token token'`, which indicates that we need both authentication for the user and authorization to invoke the web API. 
  * `scope` to `'openid profile email movie_api'`, which expresses the identity and API resources this application wants access to.

```
var config = {
    authority: 'http://localhost:1941/',
    client_id: 'movie_client',
    redirect_uri: 'http://localhost:32361/callback.html',
    post_logout_redirect_uri: 'http://localhost:32361/index.html',
    response_type: 'id_token token',
    scope: 'openid profile email movie_api'
};
var mgr = new Oidc.UserManager(config);
```

Next, trigger login and logout.

* Find the `toggleLogin` function.
* In this method use the `getUser` on the `UserManager` (like you did previously) to know if the user is logged in or not.
  If the user is not logged in, then call `signinRedirect` on the `UserManager`.
  If the user is logged in, then call `signoutRedirect` on the `UserManager`.

```
function toggleLogin() {
    mgr.getUser().then(function (user) {
        if (user) {
            mgr.signoutRedirect();
        }
        else {
            mgr.signinRedirect();
        }
    });
}
```

Next, we need to handle the response from IdentityServer.

* Open `~/wwwroot/callback.html` and inspect the code.
  * Notice that `oidc-client.js` is included, as well as another file `callback.js`.
    `callback.js` will have the code to handle callbacks.
* Open `~/wwwroot/js/callback.js`.
  * Notice it has a helper function for showing errors, as well as an instance of the `UserManager`.
* Call `signinRedirectCallback` on the `UserManager` to process the response from IdentityServer.
  This method returns a `Promise`, so use `then` to handle the successful result. In the callback redirect the browser to `index.html` (either with `window.location` or `location.replace`).
  Use `catch` to handle any errors. The `catch` callback function will be passed the error, which you can then pass to the `showError` helper.

```
var mgr = new Oidc.UserManager();
mgr.signinRedirectCallback().then(function (user) {
    var url = "index.html";
    location.replace(url);
}).catch(function (err) {
    showError(err);
});
```

Finally, we need to update IdentityServer with some updated configuration about this client since it has changed.

* Open the "IdentityServerHost" solution from `~/before`.
* Open `~/Config.cs` and find the `Clients` configuration.
* Make the following changes on the `Client` for the `"movie_client"` entry:
  * Change `AllowedGrantTypes` to `GrantTypes.Implicit`.
  * Set `AllowAccessTokensViaBrowser` to `true`.
  * Change the `RedirectUris` to `"http://localhost:32361/callback.html"`.
  * Change the `PostLogoutRedirectUris` to `"http://localhost:32361/index.html"`.
  * Remove the `ClientSecrets`.
  * Set the `AllowedCorsOrigins` collection, and add the value `"http://localhost:32361"`.  

```
new Client
{
    ClientId = "movie_client",
    ClientName = "Moive Review App",
    AllowedGrantTypes = GrantTypes.Implicit,
    AllowAccessTokensViaBrowser = true,
    RedirectUris = 
    {
        "http://localhost:32361/callback.html"
    },
    PostLogoutRedirectUris = 
    {
        "http://localhost:32361/index.html"
    },
    AllowedCorsOrigins =
    {
        "http://localhost:32361"
    },
    AllowedScopes = 
    {
        StandardScopes.OpenId.Name,
        StandardScopes.Profile.Name,
        StandardScopes.Email.Name,
        "movie_api"
    }
}
```

* Run IdentityServer to ensure it is working.
* Run the movie web application and try to login. 
  You should now see some claims for the user on the home page.

## Part 2: Using the access token to invoke the web API

In this part you will call the web API with the user's access token.
Also, you will need to configure CORS in the web API to allow it to be called from the browser.

* In the movie web app, open `~/wwwroot/js/site.js`.
* Find the `getMovieData` function.
This is invoked when the user clicks the "Movies" link at the top of the page.
  * Notice the AJAX call: `$.ajax`. 
    This invokes the web API but is not passing the access token.
* Add a `headers` object to the `$.ajax` call. 
  * Create an `Authorization` property and set it to the string `"Bearer " + user.access_token`.

```
function getMovieData(page) {
    $.ajax({
        url: 'http://localhost:53223/movies?page=' + page,
        type: 'GET',
        dataType:'json',
        headers: {
            "Authorization": "Bearer " + user.access_token
        }
    }).then(function (response) {
        bindMovieData(response);
    });
}
```

Now when the web API is invoked the access token will be passed.
Except, we can't invoke the web API if it's not running.

* Open the "MoviesWebApi" solution from `~/before`.
* Run the web API project.
* Run the movie web application, login, and click the "Movies" link.
  You should see an empty and incomplete movies page.
  The web API call should have failed due to cross-origin issues, and you can confim that by opening the developer console in the browser.
  There should be an error to the effect: 
  _XMLHttpRequest cannot load http://localhost:53223/movies?page=1. Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource. Origin 'http://localhost:32361' is therefore not allowed access._

We will enable CORS next in the web API.

* Back in the "MoviesWebApi" project, open `~/Startup.cs` and find the `ConfigureServices` method.
* Add a call to `AddCors` and use the overload where you pass a callback function to configure the options.
  * In the callback function, use the `options` parameter to create a new policy with the `AddPolicy` method.
  * Give the new policy the name `"MoviesWebApp"`.
  * On the new policy, use `WithOrigins` to add `"http://localhost:32361"`. 
  * Also on the new policy, use `AllowAnyHeader` and `AllowAnyMethod`.

```
services.AddCors(options =>
{
    options.AddPolicy("MoviesWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:32361")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

* In `~/Startup.cs` and find the `Configure` method.
* Before the call to `UseAuthentication`, call `UseCors` passing the policy name you defined above (i.e. `"MoviesWebApp"`).

```
app.UseCors("MoviesWebApp");
```

* Run the movie review web API project.
* Run the movie web app and try to access movies again. 
This time you should see the movies listed in the UI.

## Part 3: [Challenge] Think about token expiration

One problem is that the access token will expire at some point.
The _oidc-client_ has a "silent renew" feature to automatically request new access tokens via an `<iframe>`.
Look into configuring this feature to ensure the access token is never expired.
This will involve setting the `silent_redirect_uri` property, and building a new, dedicated callback page that uses the `signinSilentCallback` function on the `UserManager`.
The docs for _oidc-client_ are [here](https://github.com/IdentityModel/oidc-client-js/wiki).
