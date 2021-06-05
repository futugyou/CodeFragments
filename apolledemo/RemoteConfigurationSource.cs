namespace apolledemo
{
    public class RemoteConfigurationSource : IConfigurationSource
    {
        public RemoteConfigurationProvider _provider;
        public RemoteConfigurationProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new RemoteConfigurationProvider();
                }
                return _provider;
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this.Provider;
        }
    }
}