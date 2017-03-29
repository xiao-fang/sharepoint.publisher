using Newtonsoft.Json;
using System;

namespace SP.Publisher
{
    public class PublishConfig
    {
        [JsonProperty("spSiteUrl", Required = Required.Always)]
        public string SPSiteUrl { get; set; }

        [JsonProperty("user", Required = Required.Always)]
        public string SPUser { get; set; }

        [JsonProperty("password", Required = Required.Always)]
        public string Password { get; set; }

        [JsonProperty("spSiteType", Required = Required.Always)]
        public SiteType SPSiteType { get; set; }

        [JsonProperty("pubPaths")]
        public PublishPath[] Paths { get; set; }

        public override string ToString()
        {
            var paths = Paths == null ? "null" : string.Join<PublishPath>(Environment.NewLine, Paths);
            return string.Join(Environment.NewLine,
               $"SP Site: {SPSiteUrl}",
               $"SiteType: {SPSiteType}",
               $"SiteUser: {SPUser}",
               $"PubPaths: {paths}");
        }
    }

    public class PublishPath
    {
        [JsonProperty("src", Required = Required.Always)]
        public string Source { get; set; }

        [JsonProperty("dest", Required = Required.Always)]
        public string Destination { get; set; }

        [JsonProperty("filter")]
        public string FileFilter { get; set; }

        [JsonProperty("recursive")]
        public bool IsRecursive { get; set; }

        public override string ToString()
        {
            return $"Path: recursive={IsRecursive}, filter={FileFilter}, {Source} -> {Destination}";
        }
    }
}
