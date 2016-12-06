using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Security;
using static SP.Publisher.FileHelper;

namespace SP.Publisher
{
    /// <summary>
    /// SharePoint site type, support both On-Premise and Office365
    /// </summary>
    public enum SiteType
    {
        OnPremise = 0,
        Office365 = 1
    }

    public interface ISPClient
    {
        /// <summary>
        /// connect to SharePoint site or sub site
        /// </summary>
        /// <param name="url">SharePoint site url or sub site url</param>
        /// <param name="username">site user name</param>
        /// <param name="password"> site user password</param>
        /// <param name="siteType">site type: On-Premise or Office365</param>
        void Connect(string url, string username, string password, SiteType siteType);

        /// <summary>
        /// publish source file path to dest SharePoint url, including files, folders and the hierarchy
        /// </summary>
        /// <param name="src">source file path</param>
        /// <param name="dest">destination SharePoint url</param>
        /// <param name="isRecursive">if isRecursive, then publish source and its' hierarchy (files and folders)</param>
        /// <param name="filter"> to be implemented</param>
        void PublishFolder(string src, string dest, bool isRecursive = true, string filter = null);
    }

    public class SPClient : ISPClient
    {
        public string SiteUrl { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public SiteType Sitetype { get; private set; }

        public void Connect(string url, string username, string password, SiteType siteType)
        {
            SiteUrl = url;
            Username = username;
            Password = password;
            Sitetype = siteType;
        }

        /// <summary>
        /// get SharePoint client
        /// </summary>
        private ClientContext GetClient()
        {
            if (IsInitialized())
            {
                ClientContext client = new ClientContext(SiteUrl);

                client.ExecutingWebRequest += (s, e) =>
                {
                    e.WebRequestExecutor.WebRequest.PreAuthenticate = true;
                };

                if (Sitetype == SiteType.Office365)
                {
                    client.Credentials = new SharePointOnlineCredentials(Username, ToSecureString(Password));
                }
                else
                {
                    client.Credentials = new NetworkCredential(Username, Password);
                }

                return client;
            }
            else
            {
                throw new Exception("Connection to SharePoint Site is NOT Initialized");
            }
        }

        public void PublishFolder(string src, string dest, bool isRecursive = true, string filter = null)
        {
            using (var client = GetClient())
            {
                var web = client.Web;

                CreateCascadeFolders(web, SiteUrl, dest);

                var node = FileHelper.PathToFileNode(src, true, true);

                FileHelper.PrintHierarchy(node);

                var destRoot = string.Concat(SiteUrl.TrimEnd('/'), "/", dest.TrimEnd('/'));
                PublishNode(web, node, destRoot);
            }
        }

        private static void PublishNode(Web web, FileNode node, string rootFolderUrl)
        {
            var rootFolder = web.GetFolderByServerRelativeUrl(rootFolderUrl);

            if (node.IsDirectory)
            {
                var destFolderUrl = node.IsRoot ? rootFolderUrl : string.Concat(rootFolderUrl, "/", node.Name);
                /** only publish files/folders under root, not including root itself. */
                if (!node.IsRoot)
                {
                    if (!web.IsPropertyAvailable(destFolderUrl))
                    {
                        var destFolder = rootFolder.Folders.Add(destFolderUrl);
                        web.Context.Load(destFolder);
                    }
                }

                if (node.HasChild)
                {
                    foreach (var child in node.Children)
                    {
                        PublishNode(web, (FileNode)child, destFolderUrl);
                    }
                }
            }
            else
            {
                var destFileUrl = string.Concat(rootFolderUrl, "/", node.Name);
                var destFileInfo = new FileCreationInformation()
                {
                    Content = System.IO.File.ReadAllBytes(node.FullPath),
                    Url = destFileUrl,
                    Overwrite = true
                };
                var destFile = rootFolder.Files.Add(destFileInfo);
                web.Context.Load(destFile);
            }

            web.Context.ExecuteQuery();
        }

        /// <summary>
        /// create SharePoint Cascade folder structure, format like: "folder1/folder2/folder3"
        /// </summary>
        /// <param name="root">SharePoint url for cascade folders to be created under</param>
        /// <param name="dest">destionation folders to be created under a given root folder, format like: "folder1/folder2/folder3"</param>
        public void CreateCascadeFolders(Web web, string root, string dest)
        {
            var rootUrl = root.TrimEnd('/');
            var rootFolder = web.GetFolderByServerRelativeUrl(rootUrl);

            var folders = dest.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var folder = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == 0)
                {
                    folder = folders[i];
                }
                else
                {
                    folder += "/" + folders[i];
                }

                var folderUrl = string.Concat(rootUrl, "/", folder);
                if (!web.IsPropertyAvailable(folderUrl))
                {
                    var destFolder = rootFolder.Folders.Add(folderUrl);
                    web.Context.Load(destFolder);
                }
            }
            web.Context.ExecuteQuery();
        }

        /// <summary>
        /// check if a SharePoint client is initialized with required fields, before it can be connected.
        /// </summary>
        private bool IsInitialized()
        {
            var absense = string.IsNullOrEmpty(SiteUrl)
                || string.IsNullOrEmpty(Username)
                || string.IsNullOrEmpty(Password)
                || !SiteType.IsDefined(typeof(SiteType), Sitetype);
            return !absense;
        }

        /// <summary>
        /// cast plain text to security string
        /// </summary>
        /// <param name="str">plain text string</param>
        /// <returns>security string cast from plain text</returns>
        private static SecureString ToSecureString(string str)
        {
            var ss = new SecureString();
            foreach (var c in (str ?? string.Empty))
            {
                ss.AppendChar(c);
            }
            return ss;
        }
    }
}
