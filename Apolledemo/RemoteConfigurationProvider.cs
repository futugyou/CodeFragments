using Microsoft.Extensions.Configuration;

namespace apolledemo
{
    public class RemoteConfigurationProvider : ConfigurationProvider
    {
        public RemoteConfigurationProvider() : base()
        {
        }

        public override void Load()
        {
            this.Data["name"] = "zhangshan";
            Load(false);
        }

        public void Load(bool reload)
        {
            if (reload)
            {
                this.OnReload();
            }
        }
    }
}