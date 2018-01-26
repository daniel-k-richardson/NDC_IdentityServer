# ASP.NET Core Authentication and Authorization
---

This lab is based on a simple movie review website.
It allows customers to browse and search movies and view movie reviews.
It also allows reviewers to create and edit movie reviews.

## Overview

In this lab you will add cookie-based authentication to the movie review website using the cookie authentication middleware and claims-based identity.
Once users are authenticated, you will then also implement policy-based and resource-based authorization using the ASP.NET Core authorization framework.

### Application notes

_Data_: All the data for the movie review website it kept in-memory, so any changes to data will be lost when the application restarts.

_Users_: The lab predefines five users whose usernames are **user1** through **user5**.
These users' passwords will be the same as their username.
Once these users login to the applicaiton they will have different roles within the application: 
*user1*, *user2* and *user3* are reviewers, 
*user4* is a customer, 
and *user5* is an administrator.
When you login you can choose one of those usernames in order to trigger different behavior in the application.

## Part 1: Cookie-base authentication

In this part you will add the cookie authentication middleware and services, allow the user to login and logout, and use claims to model the identity of the authenticated user.

* Open the application from the `~/before` folder. 
  * Inspect the code to become familiar with the structure.
  * Run the application to see what it currently does.

To authenticate users, we need to add the authentication services and middleware to the application. We will do this work in the `MoviesWebApp` project.

* Open `~/Startup.cs`.
* In the `ConfigureServices` method add the authentication services to DI.
  * Set `"Cookies"` as the default scheme.
* Also add the cookie handler with `AddCookies`. 
  * Set the `LoginPath` to `"/Account/Login"`.
  * Set the `AccessDeniedPath` to `"/Account/AccessDenied"`.

```
services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
```

* In the `Configure` method register the  authentication middleware after the static file middleware, but before the MVC middleware.

```
app.UseStaticFiles();

app.UseAuthentication();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});
```
Next we will use the authentication services to issue the login cookie.

* Open `~/Controllers/AccountController.cs` and find the `Login` action methods.
* Implement the logic to allow users to signin.
  * We don't have a real database of username/passwords, so just check that they are the same.
  * If successful, create a list of `Claim`s and populate it with the `sub` claim with the value of the `username`.
  * Notice there is a `MovieIdentityService` in the `AccountController` -- this allows application specific claims to be loaded based upon the `sub` claim.
   Feel free to look in the implementation to understand the additional claims being loaded for the users.
   Invoke it and merge the claims returned into the claims collection you created.
  * Create `ClaimsIdentity` and `ClaimsPrincipal` from the claims.
  * Use the `SignInAsync` method on the `HttpContext` and issue the cookie from the `ClaimsPrincipal`.
  * Rediriect the user to the `ReturnUrl` (if present), or to the home page. 

```
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (model.Username == model.Password)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", model.Username)
        };
        claims.AddRange(_identityService.GetClaimsForUser(model.Username));

        var ci = new ClaimsIdentity(claims, "password", "name", "role");
        var cp = new ClaimsPrincipal(ci);

        await HttpContext.SignInAsync("Cookies", cp);

        if (model.ReturnUrl != null)
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    ModelState.AddModelError("", "Invalid username or password");
    return View();
}
```

* Find the `Logout` method.
* Implement the logic to allow a user to signout.

```
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync("Cookies");
    return RedirectToAction("Index", "Home");
}
```

* Run the application and test signing in and signing out.

## Part 2: Policy-based and Resource-based authorization

In this part you will enable authorization. 
There are several pieces to this, including preventing anonymous access to much of the application, only allowing customers to use the search feature, and only allowing reviewers to create and edit reviews.

* Back in `~/Startup.cs`, add the authorization services to DI in `ConfigureServices`.

```
services.AddAuthorization();
```

Next we want a global authorization filter that prevents anonymous access.

* In `ConfigureServices` locate the call to `AddMvc` and the configuration callback. 
* Create a policy by using a `AuthorizationPolicyBuilder`, 
and calling `RequireAuthenticatedUser` and `Build`. 
* Create a new `AuthorizeFilter` using the new policy. 
* Add the filter to the `MvcOptions`'s `Filters` collection.

```
services.AddMvc(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
```

If you were to run the application now, an anonymous user would not be able to access any page including the login page. 
We now need to relax the global filter except for the few places where we want to allow anoymous access.

* Add the `[AllowAnonymous]` attribute to both the `HomeController` and the `AccountController`.

```
[AllowAnonymous]
public class AccountController : Controller
{
    ...
}
```

* Run the application to test that an anonymous user cannot access the movies, but can login.

The next authorization we want to enforce is only customers may use the search feature. 
This involves building an authorization policy.

* Locate the call to `AddAuthorization` in `ConfigureServices`.
* Change the signature to accept an delegate that passes the options.

```
services.AddAuthorization(options =>
{
    ...
});

```
 * In the callback, use the options API `AddPolicy` to create a new policy and name it `"SearchPolicy"`.
 * Build the policy to use `RequireAuthenticatedUser` and `RequireAssertion`. 
   * For the assertion callback check for either the `"Admin"` or `"Customer"` role claim and if they are present return `true`. Return `false` otherwise.

```
services.AddAuthorization(options =>
{
    options.AddPolicy("SearchPolicy", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireAssertion(ctx =>
        {
            if (ctx.User.HasClaim("role", "Admin") ||
                ctx.User.HasClaim("role", "Customer"))
            {
                return true;
            }
            return false;
        });
    });
});
``` 

* Now apply the `"SearchPolicy"` to the `Search` action method on the `MovieController`.
```
[Authorize("SearchPolicy")]
public IActionResult Search(string searchTerm = null)
{
    ...
}
```

* Run the application to test that only customers or admins (i.e. **user4** or **user5**) are allowed to use the search feature.
If not allowed, you should be redirected to the "access denied" page.

The final authorization logic we require is to only allow reviewers to create and edit reviews.
We will do this by building authorization handlers. 

* Expand the `~/Authorization` folder. 
This contains the beginings of the authorization handlers.
Open the files and inspect the starter code.
* For the `MovieAuthorizationHandler` implement the logic that only reviewers are allowed to review movies.

```
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context, 
    OperationAuthorizationRequirement requirement, 
    MovieDetails movie)
{
    if (requirement == MovieOperations.Review)
    {
        if (context.User.HasClaim("role", "Reviewer"))
        {
            context.Succeed(requirement);
        }
    }
    return Task.FromResult(0);
}
```

* For the `ReviewAuthorizationHandler` implement the logic that only the reviewer that created the review can edit it.
  * Use the `sub` claim on the user and compare it to the `UserId` property on the `MovieReview`.
  * Also, allow admins to perform any operation.

```
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context, 
    OperationAuthorizationRequirement requirement, 
    MovieReview review)
{
    if (context.User.HasClaim("role", "Admin"))
    {
        context.Succeed(requirement);
    }

    if (requirement == ReviewOperations.Edit)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub != null && review.UserId == sub)
        {
            context.Succeed(requirement);
        }
    }
    return Task.FromResult(0);
}
```

* To use these authorization handlers, they need to be registered in DI in `ConfigureServices`. Do that now.

```
services.AddTransient<IAuthorizationHandler, ReviewAuthorizationHandler>();
services.AddTransient<IAuthorizationHandler, MovieAuthorizationHandler>();
```

Next we want to invoke the authorization logic in the MVC code to protect access.

* In the `ReviewController` controller change the consructor and inject an `IAuthorizationService` and store it in a member variable.

```
private IAuthorizationService _authorization;
public ReviewController(ReviewService reviews, 
    MovieService movies, IAuthorizationService authorization)
{
    _reviews = reviews;
    _movies = movies;
    _authorization = authorization;
}
```

* In `New` enforce the authorization for creating a review for the movie. If the user is not allowed, then return the result from `Forbid`.

```
var authz = await _authorization.AuthorizeAsync(User, 
    movie, Authorization.MovieOperations.Review);
if (!authz.Succeeded)
{
    return Forbid();
}
```

* In `Edit` and `Delete` enforce the authorization for editing the review.

```
var authz = await _authorization.AuthorizeAsync(User, 
    review, Authorization.ReviewOperations.Edit);
if (!authz.Succeeded)
{
    return Forbid();
}
```

* Run the application and test that only reviewers can create reviews, and that reviewers can only edit their own reviews.  

Next we want to hide the buttons in the UI if the user is not allowed to create or edit reviews.

* In `~/Views/Movie/Details.cshtml` notice the `IAuthorizationService` is already being injected.
* Locate the "create review" button and hide it if the user is not authorized.

```
@{ 
    var authz = await authorization.AuthorizeAsync(User,
        Model, MoviesWebApp.Authorization.MovieOperations.Review);
    if (authz.Succeeded)
    {
        <div class="row search-form">
            <a asp-action="New" asp-controller="Review" 
                asp-route-movieId="@Model.Id" 
                class="btn btn-primary">Write a review</a>
        </div>
    }
}
```

* Locate the "edit review" button and hide it is the user is not authorized.

```
<td>
@{ 
    var editAuthz = await authorization.AuthorizeAsync(User,
        review, MoviesWebApp.Authorization.ReviewOperations.Edit);
    if (editAuthz.Succeeded)
    {
        <a asp-action="Edit" asp-controller="Review" 
            asp-route-id="@review.Id" 
            class="btn btn-primary">edit</a>
    }
}
</td>
```

* Run and test that the buttons are now hidden when appropriate.

Finally, we need to make a change in our authorization logic. 
Reviewers are not allowed to create reviews for all movies. Certain reviewers are only allowed to review movies from certain countries.
This logic requires a lookup in a permission database and this is implemented in a class called `ReviewPermissionService`. 
You will now incorporate this additional logic in the `MovieAuthorizationHandler`.

*  Change the constructor to accept the `MovieAuthorizationHandler` and store it in a member variable.

```
private ReviewPermissionService _reviewPermissions;
public MovieAuthorizationHandler(ReviewPermissionService reviewPermissions)
{
    _reviewPermissions = reviewPermissions;
}
```

* In `Handle` after the role check, invoke `GetAllowedCountries` on the `ReviewPermissionService` and compare the movie's `CountryName` to the returned list. Only if the movie is from an allowed country, then call `Succeed`.

```
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context, 
    OperationAuthorizationRequirement requirement, 
    MovieDetails movie)
{
    if (requirement == MovieOperations.Review)
    {
        if (context.User.HasClaim("role", "Reviewer"))
        {
            var allowed = _reviewPermissions.GetAllowedCountries(context.User);
            if (allowed.Contains(movie.CountryName))
            {
                context.Succeed(requirement);
            }
        }
    }

    return Task.FromResult(0);
}
```

* Run and test the country-specific authorization. *user1* should be able to create movies from any country, but *user2* cannot create a review for France.
