// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var config = {
    authority: "https://localhost:5001",
    client_id: "spa",
    redirect_uri: "https://localhost:5005/html/callback.html",
    response_type: "code",
    scope: "openid profile api1",
    post_logout_redirect_uri: "https://localhost:5005/html/index.html",
};
var mge = new Oidc.UserManager(config);

function login() {
    mge.signinRedirect();
}

function api() {
    mge.getUser().then(function (user) {
        if (user == null || user.access_token == undefined) {
            mge.signinRedirect();
            return;
        }

        var url = "https://localhost:5003/WeatherForecast";
        var xhr = new XMLHttpRequest();
        xhr.open("GET", url);
        xhr.onload = function () {
            console.log(xhr.status, JSON.parse(xhr.responseText));
            document.getElementById("textarea").innerText = xhr.responseText;
        }
        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        xhr.send();
    });
}


function logout() {
    mge.signoutRedirect();
}