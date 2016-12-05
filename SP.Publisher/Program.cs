using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BufferHeight = 5000;

            DemoEntry();

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void DemoEntry()
        {
            var assetsPath = @"[path for local site assets to be published to sharepoint]";
            var pagesPath = @"[path for local site pages to be published to sharepoint]";

            var spClient = new SPClient();
            spClient.Connect("https://spsite-host/sp-teams-site-or-subsite", "sp-user", "sp-password", SiteType.OnPremise);
            spClient.PublishFolder(assetsPath, "SiteAssets", true);
            spClient.PublishFolder(pagesPath, "SitePages", true, "*.aspx");
        }
    }
}
