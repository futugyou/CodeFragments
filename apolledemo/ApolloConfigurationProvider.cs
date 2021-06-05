namespace apolledemo
{
    public class ApolloConfigurationProvider : ConfigurationProvider, IRepositoryChangeListener, IConfigurationSource
    {
        internal string? SectionKey { get; }
        internal IConfigRepository ConfigRepository { get; }
        private Task? _initializeTask;

        public ApolloConfigurationProvider(string? sectionKey, IConfigRepository configRepository)
        {
            SectionKey = sectionKey;
            ConfigRepository = configRepository;
            ConfigRepository.AddChangeListener(this);
            _initializeTask = ConfigRepository.Initialize();
        }

        public override void Load()
        {
            Interlocked.Exchange(ref _initializeTask, null)?.ConfigureAwait(false).GetAwaiter().GetResult();

            SetData(ConfigRepository.GetConfig());
        }

        protected virtual void SetData(Properties properties)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in properties.GetPropertyNames())
            {
                if (string.IsNullOrEmpty(SectionKey))
                    data[key] = properties.GetProperty(key) ?? string.Empty;
                else
                    data[$"{SectionKey}{ConfigurationPath.KeyDelimiter}{key}"] = properties.GetProperty(key) ?? string.Empty;
            }

            Data = data;
        }

        void IRepositoryChangeListener.OnRepositoryChange(string namespaceName, Properties newProperties)
        {
            SetData(newProperties);

            OnReload();
        }

        IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder) => this;
    }
}