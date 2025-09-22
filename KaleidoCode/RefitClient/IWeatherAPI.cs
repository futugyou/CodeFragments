
using System.Text.Json.Serialization;

namespace KaleidoCode.RefitClient;

public interface IWeatherAPI
{
    [Get("/WeatherForecast")]
    Task<dynamic> Get();
}

[Headers("User-Agent: Refit Integration Tests")]
//[RefitClient("github")]
public interface IGitHubApi
{
    [Get("/users/{username}")]
    Task<User> GetUser(string userName);
}

public class User
{
    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; }

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; }

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; }

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; }

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; }

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; }

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; }

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; }

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("site_admin")]
    public object SiteAdmin { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }

    [JsonPropertyName("blog")]
    public string Blog { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("hireable")]
    public string Hireable { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }

    [JsonPropertyName("twitter_username")]
    public string TwitterUsername { get; set; }

    [JsonPropertyName("public_repos")]
    public int PublicRepos { get; set; }

    [JsonPropertyName("public_gists")]
    public int PublicGists { get; set; }

    [JsonPropertyName("followers")]
    public int Followers { get; set; }

    [JsonPropertyName("following")]
    public int Following { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }
}
