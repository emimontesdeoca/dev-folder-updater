using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DevFolderUpdate
{
    class Program
    {
        public static string DEV_PATH;
        public static string WEBSITE_PATH;
        public static string MODULE_NAME;

        public enum LogTypes
        {
            Error,
            Copied,
            Changes
        }

        static void Main(string[] args)
        {
            ShowHeader();

            DEV_PATH = @"C:\Desarrollo\Dev\Hotelequia.Backend\src\Hotelequia.Backend.Activities\bin" ?? AskForString("Enter developer folder path");
            WEBSITE_PATH = @"C:\Websites\hotelequia-backend-dev_2019050715_2\bin" ?? AskForString("Enter website folder path");
            //MODULE_NAME = AskForString("Enter module name");

            if (String.IsNullOrEmpty(DEV_PATH) || string.IsNullOrEmpty(WEBSITE_PATH))
            {
                Log(LogTypes.Error, "Error in values");
                Console.WriteLine("Press 'q' to quit the sample.");
                Console.ReadLine();
            }
            else
            {
                RunWatcher();
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void RunWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = DEV_PATH;

                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Filter = $"*.*";

                watcher.Changed += new FileSystemEventHandler(OnChanged);
                //watcher.Created += new FileSystemEventHandler(OnChanged);
                //watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                //watcher.Renamed += new RenamedEventHandler(OnRenamed);

                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the sample.");
                while (Console.Read() != 'q') ;
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            string path = Path.Combine(WEBSITE_PATH, e.Name);

            try
            {
                if (IsFile(path))
                {
                    if (File.Exists(path))
                    {
                        var newCurrentFile = new FileInfo(e.FullPath);
                        var currentFile = new FileInfo(path);

                        if (newCurrentFile.LastWriteTimeUtc > currentFile.LastWriteTimeUtc)
                        {
                            Log(LogTypes.Changes, e.Name);
                            FileCopy(e.Name, e.FullPath, path);
                        }
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        var newCurrentDirectory = new DirectoryInfo(e.FullPath);
                        var currentDirectory = new DirectoryInfo(path);

                        if (newCurrentDirectory.LastWriteTimeUtc > currentDirectory.LastWriteTimeUtc)
                        {
                            Log(LogTypes.Changes, e.Name);
                            DirectoryCopy(e.FullPath, path, true);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Log(e.ChangeType, e.Name);

            //string path = Path.Combine(WEBSITE_PATH, e.Name);

            //if (IsFile(e.FullPath))
            //{
            //    FileCopy(e.Name, e.FullPath, path);
            //}
            //else
            //{
            //    DirectoryCopy(e.FullPath, path, true);
            //}
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Log(LogTypes.Changes, e.Name);

            string path = Path.Combine(WEBSITE_PATH, e.Name);

            if (File.Exists(path) || Directory.Exists(path))
            {
                if (IsFile(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Log(LogTypes.Changes, e.Name);

            string path = Path.Combine(WEBSITE_PATH, e.Name);
            string oldPath = Path.Combine(WEBSITE_PATH, e.OldName);

            if (IsFile(oldPath))
            {
                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, path);
                }
            }
            else
            {
                if (Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, path);
                }
            }
        }

        #region UTILS


        private static void Log(LogTypes types, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now}] - ");

            switch (types)
            {
                case LogTypes.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogTypes.Copied:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogTypes.Changes:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            Console.Write($"[{types.ToString()}] - {message} \n");
        }

        private static void Log(WatcherChangeTypes type, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now}] - [{type.ToString()}] - {message} \n");
        }

        private static string AskForString(string message)
        {
            try
            {
                //Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"[{DateTime.Now}] {message}: ");
                return Console.ReadLine();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static void FileCopy(string filename, string sourceFile, string destFile)
        {
            try
            {
                //Log(LogTypes.CHANGES, filename);
                File.Copy(sourceFile, destFile, true);
                Log(LogTypes.Copied, filename);

            }
            catch (Exception e)
            {
                Log(LogTypes.Error, filename);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static bool IsFile(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
                return false;
            else
                return true;
        }

        private static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"______    _     _             _   _           _       _            ");
            Console.WriteLine(@"|  ___|  | |   | |           | | | |         | |     | |           ");
            Console.WriteLine(@"| |_ ___ | | __| | ___ _ __  | | | |_ __   __| | __ _| |_ ___ _ __ ");
            Console.WriteLine(@"|  _/ _ \| |/ _` |/ _ \ '__| | | | | '_ \ / _` |/ _` | __/ _ \ '__|");
            Console.WriteLine(@"| || (_) | | (_| |  __/ |    | |_| | |_) | (_| | (_| | ||  __/ |   ");
            Console.WriteLine(@"\_| \___/|_|\__,_|\___|_|     \___/| .__/ \__,_|\__,_|\__\___|_|   ");
            Console.WriteLine(@"                                   | |                             ");
            Console.WriteLine(@"                                   |_|                             ");
            Console.WriteLine();
            Console.WriteLine("Source code: https://github.com/emimontesdeoca/dev-folder-updater");
            Console.WriteLine();
        }
        #endregion

    }
}
