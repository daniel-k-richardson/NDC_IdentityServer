/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/oidc-client/dist/oidc-client.js" />

//Oidc.Log.logger = console;

function showError(err) {
    $("#error_text").text(err.message);
    $("#error_container").removeClass("hidden");
}

var mgr = new Oidc.UserManager();
mgr.signinRedirectCallback().then(function (user) {
    var url = "index.html";
    if (user.state) {
        url += "#" + user.state;
    }
    location.replace(url);
}).catch(function (err) {
    showError(err);
});
