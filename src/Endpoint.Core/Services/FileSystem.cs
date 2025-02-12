﻿using System.IO;
using System.Linq;

namespace Endpoint.Core.Services
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path)
            => File.Exists(path);

        public bool Exists(string[] paths)
            => paths.Any(x => Exists(x));

        public Stream OpenRead(string path)
            => File.OpenRead(path);

        public string ReadAllText(string path)
            => File.ReadAllText(path);

        public void WriteAllLines(string path, string[] contents)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllLines(path, contents);
        }

        public string ParentFolder(string path)
        {
            var directories = path.Split(Path.DirectorySeparatorChar);

            string parentFolderPath = string.Join($"{Path.DirectorySeparatorChar}", directories.ToList()
                .Take(directories.Length - 1));

            return parentFolderPath;
        }

        public void CreateDirectory(string directory)
        {
            System.IO.Directory.CreateDirectory(directory);
        }
    }
}
