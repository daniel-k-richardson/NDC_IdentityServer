/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/oidc-client/dist/oidc-client.js" />

//Oidc.Log.logger = console;

function showError(err) {
    $("#error_text").text(err.message);
    $("#error_container").removeClass("hidden");
}

var mgr = new Oidc.UserManager();

// TODO: call signinRedirectCallback to process the callback
// if successful, redirect the user back to "index.html"
// if not successful, call showError with the error
