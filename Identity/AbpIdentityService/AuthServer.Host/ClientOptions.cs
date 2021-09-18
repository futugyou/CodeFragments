namespace AuthServer.Host
{
    public class ClientOptions
    {
        public List<ClientDetail> Clients { get; set; }
    }

    public class ClientDetail
    {
        public string Name { get; set; }
        public string Secret { get; set; }
        public string Scopes { get; set; }
    }

}
