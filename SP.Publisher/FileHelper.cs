using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SP.Publisher
{
    public class FileHelper
    {
        public const string DefaultIndent = @"  ";

        /* http://www.fileformat.info/info/unicode/char/0005/index.htm '|' */
        private const char NODEO = '\u0019';

        /* http://www.fileformat.info/info/unicode/char/0019/index.htm '├' */
        private const char NODE1 = '\u0019';

        /* http://www.fileformat.info/info/unicode/char/001c/index.htm '└' */
        private const char NODE2 = '\u001C';

        /// <summary>
        /// Hierarchy Node
        /// </summary>
        public class Node
        {
            public string Name { get; set; }

            public bool IsRoot { get; set; }

            public IEnumerable<Node> Children { get; set; }

            public bool HasChild
            {
                get { return Children != null && Children.Count() > 0; }
            }
        }

        /// <summary>
        /// Hierarchy Node with FileInfo
        /// </summary>
        public class FileNode : Node
        {
            public string FullPath { get; set; }

            public bool IsDirectory { get; set; }
        }

        /// <summary>
        /// print out node hierarchy
        /// </summary>
        /// <param name="node">root node</param>
        /// <param name="indent">indent indicator</param>
        public static void PrintHierarchy(Node node, string indent = DefaultIndent)
        {
            if (node.IsRoot)
            {
                Console.WriteLine(node.Name);
            }
            else
            {
                //Console.WriteLine("{0}{1}{2}", indent, node.HasChild ? "├-" : "└-", node.Name);
                Console.WriteLine("{0}{1}-{2}", indent, node.HasChild ? NODE1 : NODE2, node.Name);
                //indent += node.HasChild ? "│ " : "  ";
                indent += $"{(node.HasChild ? NODEO : ' ')} ";
            }

            if (node.HasChild)
            {
                foreach (var child in node.Children)
                {
                    PrintHierarchy(child, indent);
                }
            }
        }

        /// <summary>
        /// build node hierarchy
        /// </summary>
        /// <param name="node">root node</param>
        /// <param name="indent">indent indicator</param>
        /// <returns></returns>
        public static string BuildHierarchy(Node node, string indent = DefaultIndent)
        {
            var sb = new StringBuilder();

            if (node.IsRoot)
            {
                sb.AppendLine(node.Name);
            }
            else
            {
                //sb.AppendFormat("{0}{1}{2}", indent, node.HasChild ? "├-" : "└-", node.Name);
                sb.AppendFormat("{0}{1}-{2}", indent, node.HasChild ? NODE1 : NODE2, node.Name);
                //indent += node.HasChild ? "│ " : "  ";
                indent += $"{(node.HasChild ? NODEO : ' ')} ";
            }

            if (node.HasChild)
            {
                foreach (var child in node.Children)
                {
                    sb.AppendLine(BuildHierarchy(child, indent));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert from file path to Node
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="isRecursive">recursive for node children</param>
        /// <param name="isRoot">is root node</param>
        /// <returns>node from file path</returns>
        public static Node PathToNode(string path, bool isRecursive = true, bool isRoot = false)
        {
            var name = Path.GetFileName(path);
            var isDir = IsDirectory(path);

            var node = new Node() { Name = name, IsRoot = isDir ? isRoot : false };

            if (isDir)
            {
                node.IsRoot = isRoot;
                var childrens = Directory.GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
                if (isRecursive)
                {
                    node.Children = childrens.Select(child => PathToNode(child, isRecursive, false));
                }
                else
                {
                    node.Children = childrens.Select(child => new Node() { Name = Path.GetFileName(child), IsRoot = false });
                }
            }

            return node;
        }

        /// <summary>
        /// Convert from file path to FileNode
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="isRecursive">recursive for node children</param>
        /// <param name="isRoot">is root node</param>
        /// <returns>file node from file path</returns>
        public static FileNode PathToFileNode(string path, bool isRecursive = true, bool isRoot = false, string filter = @"*")
        {
            var name = Path.GetFileName(path);
            var isDir = IsDirectory(path);

            var node = new FileNode() { Name = name, FullPath = path, IsDirectory = isDir, IsRoot = isDir ? isRoot : false };

            if (isDir)
            {
                node.IsRoot = isRoot;
                var childrens = Directory.GetFileSystemEntries(path, filter, SearchOption.TopDirectoryOnly);
                if (isRecursive)
                {
                    node.Children = childrens.Select(child => PathToFileNode(child, isRecursive, false));
                }
                else
                {
                    node.Children = childrens.Select(child => new FileNode()
                    {
                        Name = Path.GetFileName(child),
                        IsDirectory = IsDirectory(child),
                        FullPath = child,
                        IsRoot = false
                    });
                }
            }

            return node;
        }

        /// <summary>
        /// check if file path is a directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if file path is a directory</returns>
        public static bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }
}
