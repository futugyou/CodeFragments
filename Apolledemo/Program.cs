using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace apolledemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            var selfSource = new RemoteConfigurationSource();

            builder.Add(selfSource);

            var root = builder.Build();

            Console.WriteLine($"name: {root["name"]}");

            ChangeToken.OnChange(() => root.GetReloadToken(), () =>
            {
                Console.WriteLine($"reload name: {root["name"]}");
            });

            selfSource.Provider.Set("name", "lisi");
            selfSource.Provider.Load(true);
            Console.ReadKey();
        }
    }
}
