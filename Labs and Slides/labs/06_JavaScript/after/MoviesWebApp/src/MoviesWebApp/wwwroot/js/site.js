/// <reference path="../lib/jquery/dist/jquery.js" />
/// <reference path="../lib/oidc-client/dist/oidc-client.js" />

//Oidc.Log.logger = console;

var config = {
    authority: 'http://localhost:1941/',
    client_id: 'movie_client',
    redirect_uri: 'http://localhost:32361/callback.html',
    post_logout_redirect_uri: 'http://localhost:32361/index.html',
    response_type: 'id_token token',
    scope: 'openid profile email movie_api'
};
var mgr = new Oidc.UserManager(config);

$(function () {
    var home_button = $("#home_button");
    var movies_button = $("#movies_button");
    var login_button = $("#login_button");
    var login_status = $("#login_status");
    var body_container = $("#body_container");

    home_button.click(showHomePage);
    movies_button.click(showMoviesPage);
    login_button.click(toggleLogin);

    showLoginStatus();

    function showLoginStatus() {
        mgr.getUser().then(function (user) {
            if (user) {
                showLoggedInUser(user);
            }
            else {
                showAnonymousUser();
            }
        });

        function showLoggedInUser(user) {
            login_status.text("Logged in as: " + user.profile.name);
            login_button.text("Logout");
        }

        function showAnonymousUser() {
            login_status.text("Not logged in");
            login_button.text("Login");
        }
    }

    if (location.hash === "#movies") {
        showMoviesPage();
    }
    else {
        showHomePage();
    }

    function showHomePage() {
        body_container.load("_home.html", bindHomePage);

        function bindHomePage() {
            var claims = $("#claims");
            var dl = claims.find("dl");
            mgr.getUser().then(function (user) {
                if (user) {
                    dl.append("<dt>access_token</dt>");
                    dl.append("<dd>" + user.access_token + "</dd>");
                    for (var key in user.profile) {
                        dl.append("<dt>" + key + "</dt>");
                        dl.append("<dd>" + user.profile[key] + "</dd>");
                    }
                    claims.show();
                }
                else {
                    claims.hide();
                }
            });
        }
    }

    function showMoviesPage() {

        mgr.getUser().then(function (user) {
            if (user) {
                body_container.load("_movies.html", bindMoviePage);
            }
            else {
                mgr.signinRedirect({ data: 'movies' });
            }

            function bindMoviePage() {
                $(".pagination").on("click", "a", function () {
                    getMovieData($(this).data('page'));
                });
                
                getMovieData(1);
            }

            function bindMovieData(movies) {
                $("#total_count").text(movies.totalCount + " total");
                $("#current_page").text(movies.page);
                $("#total_pages").text(movies.totalPages);

                var count = 7;
                var half = parseInt((count / 2) + 1);
                var start = Math.max(movies.page - (count - half), 1);
                var end = Math.min(Math.max(movies.page - (count - half), 1) + (count - 1), movies.totalPages);
                if (end - start < count) {
                    start = end - count + 1;
                }

                var ul = $(".pagination");
                ul.empty();

                ul.append("<li><a data-page='1'>&laquo;</a></li>");
                for (var i = start; i <= end; i++) {
                    var li = "<li class='" + (i == movies.page ? "active" : "") + "'>";
                    li += ("<a data-page='" + i + "'>" + i + "</a>");
                    li += "</li>";
                    ul.append(li);
                }
                ul.append("<li><a data-page='" + movies.totalPages + "'>&raquo;</a></li>");

                var tbody = $("tbody");
                tbody.html('');

                for (var i = 0; i < movies.count; i++) {
                    var row = "<tr>";
                    row += "<td>" + movies.movies[i].title + "</td>";
                    row += "<td>" + movies.movies[i].year + "</td>";
                    row += "<td>" + movies.movies[i].rating + "</td>";
                    row += "<td>" + movies.movies[i].countryName + "</td>";
                    row += "<td>" + movies.movies[i].directorName + "</td>";
                    row += "<td><img width='50' src=\"/Posters/" + encodeURIComponent(movies.movies[i].posterName) + "\" /></td>";
                    row += "</tr>";

                    tbody.append(row);
                }
            }

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
        });
    }

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
});
