using Newtonsoft.Json;
using System;
using System.IO;

namespace SP.Publisher
{
    class Program
    {
        /// <param name="args">
        ///  - config.file.path [required]
        ///  - deploy_username [optional, can also set in config file]
        ///  - deploy_password [optional, can also set in config file]
        /// </param>
        static void Main(string[] args)
        {
#if DEBUG
            Console.BufferHeight = 5000;
            args = new string[] { @"dev-amy-edworks.json" };
#endif

            MainEntry(args);
        }

        static void MainEntry(string[] args)
        {
            if (args != null && args.Length > 0 && File.Exists(args[0]))
            {
                var file = args[0];
                var config = LoadPublishConfig(file);

                LogHelper.Info($"{config}");
                if (config.Paths == null || config.Paths.Length == 0)
                {
                    LogHelper.Warning("No pubPath node need to be published");
                    return;
                }

                /*if args[1], args[2] set, then update user/password from command line*/
                if (args.Length >= 3)
                {
                    config.SPUser = args[1];
                    config.Password = args[2];
                }

                LogHelper.Info($"Start to connect to SharePoint site: {config.SPSiteUrl}");
                var spClient = new SPClient();
                spClient.Connect(config.SPSiteUrl, config.SPUser, config.Password, config.SPSiteType);
                LogHelper.Info($"Connected successfully to SharePoint site: {config.SPSiteUrl}");

                foreach (var path in config.Paths)
                {
                    LogHelper.Info($"Start to publish: [{path.Destination}]");
                    spClient.PublishFolder(path.Source, path.Destination, path.IsRecursive, path.FileFilter);
                    LogHelper.Info($"Published successfully: [{path.Destination}]");
                }
            }
            else
            {
                LogHelper.Error("PublishConfig file does not exist.", null);
            }
        }

        static void DemoEntry()
        {
            var assetsPath = @"[path for local site assets to be published to sharepoint]";
            var pagesPath = @"[path for local site pages to be published to sharepoint]";

            var spClient = new SPClient();
            spClient.Connect("https://spsite-host/sp-teams-site-or-subsite", "sp-user", "sp-password", SiteType.OnPremise);
            spClient.PublishFolder(assetsPath, "SiteAssets/v1.0", true);
            spClient.PublishFolder(pagesPath, "SitePages", true, "*.aspx");
        }

        static PublishConfig LoadPublishConfig(string file)
        {
            var configJson = File.ReadAllText(file);
            var config = JsonConvert.DeserializeObject<PublishConfig>(configJson);
            return config;
        }
    }
}
