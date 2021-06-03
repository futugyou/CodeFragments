using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kong1
{
    public static class PassportConfig
    {
        private static IConfiguration _config;
        public static IWebHostEnvironment Env;

        public static void InitPassportConfig(this IConfiguration config, IWebHostEnvironment environment)
        {
            if (PassportConfig._config != null)
                return;
            PassportConfig._config = config;
            PassportConfig.Env = environment;
        }

        public static bool IsDevelopment() => Env.IsDevelopment();

        public static bool IsStaging() => Env.IsStaging();

        public static bool IsProduction() => Env.IsProduction();

        /// <summary>获取单个简单配置</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key) => PassportConfig._config[key];

        public static bool GetBool(string key)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(key) && PassportConfig._config != null)
                bool.TryParse(PassportConfig.Get(key), out result);
            return result;
        }

        public static int GetInt(string key, int defaultValue = -2147483648)
        {
            int result = 0;
            return !string.IsNullOrEmpty(key) && PassportConfig._config != null && (!int.TryParse(PassportConfig.Get(key), out result) && defaultValue != int.MinValue) ? defaultValue : result;
        }

        /// <summary>获取某个section中指定key的配置</summary>
        /// <param name="key"></param>
        /// <param name="valIndex"></param>
        /// <returns></returns>
        public static string GetSection(string key, string valIndex) => ((IConfiguration)PassportConfig._config.GetSection(key))[valIndex];

        /// <summary>获取指定section</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IConfigurationSection GetSection(string key) => PassportConfig._config.GetSection(key);

        /// <summary>获取数据库连接字符串</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetConnectionString(string name) => ConfigurationExtensions.GetConnectionString(PassportConfig._config, name);

        /// <summary>获取命令行 --healthHost 参数</summary>
        /// <returns></returns>
        public static string GetHealthHost() => PassportConfig.Get("healthHost") ?? throw new ArgumentNullException("healthHost cannot be null or empty!");

        /// <summary>获取当前运行程序的端口</summary>
        /// <returns></returns>
        public static int GetCurrentPort() => new Uri((PassportConfig.Get("urls") ?? "http://[::]:5000").Replace("*", "[::]").Replace("+", "[::]")).Port;
    }
}
