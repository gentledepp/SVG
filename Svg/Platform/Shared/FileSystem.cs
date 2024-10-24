using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Svg.Interfaces;
using File = System.IO.File;

namespace Svg
{
    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public Stream OpenReadWrite(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public virtual string GetDefaultStoragePath()
        {
#if WINDOWS_UWP
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
        }

        public string PathCombine(params string[] segments)
        {
            return Path.Combine(segments);
        }

        public void DeleteFile(string storagePath)
        {
            File.Delete(storagePath);
        }

        public void DeleteFolder(string storagePath)
        {
            Directory.Delete(storagePath, true);
        }

        public void EnsureDirectoryExists(string directoryName)
        {
            if (Directory.Exists(directoryName))
                return;
            Directory.CreateDirectory(directoryName);
        }
    }
}