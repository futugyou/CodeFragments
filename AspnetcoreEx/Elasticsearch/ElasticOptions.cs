namespace AspnetcoreEx.Elasticsearch;

public class ElasticOptions
{
    public string[] Uris { get; set; }
    public string DefaultIndex { get; set; }
    public int ConnectionLimit { get; set; } = 80;
    public Uri[] UriList => Uris.Select(p => new Uri(p)).ToArray();
    public string ApiID { get; set; }
    public string ApiKey { get; set; }
    public string Base64EncodedApiKey { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string CertificatePath { get; set; }
}