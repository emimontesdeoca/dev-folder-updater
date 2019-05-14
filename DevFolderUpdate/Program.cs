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

        public enum LogTypes
        {
            ERROR,
            SUCCESS,
            CHANGES
        }

        static void Main(string[] args)
        {
            DEV_PATH = AskForString("Enter developer folder path");
            WEBSITE_PATH = AskForString("Enter website folder path");

            if (String.IsNullOrEmpty(DEV_PATH) || string.IsNullOrEmpty(WEBSITE_PATH))
            {
                Log(LogTypes.ERROR, "Error in values");
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
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                watcher.Filter = "*.*";

                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);


                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the sample.");
                while (Console.Read() != 'q') ;
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Log(LogTypes.CHANGES, e.Name);

          
            string path = Path.Combine(WEBSITE_PATH, e.Name);

            if (IsFile(e.FullPath))
            {
                FileCopy(e.Name, e.FullPath, path);
            }
            else
            {
                DirectoryCopy(e.FullPath, path, true);
            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Log(LogTypes.CHANGES, e.Name);

            string path = Path.Combine(WEBSITE_PATH, e.Name);

            if (IsFile(e.FullPath))
            {
                FileCopy(e.Name, e.FullPath, path);
            }
            else
            {
                DirectoryCopy(e.FullPath, path, true);
            }
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Log(LogTypes.CHANGES, e.Name);

            string path = Path.Combine(WEBSITE_PATH, e.Name);

            if (IsFile(path))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }


        #region UTILS

        private static void Log(LogTypes types,  string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now}] - ");

            switch (types)
            {
                case LogTypes.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogTypes.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogTypes.CHANGES:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            Console.Write($"[{types.ToString()}] - {message} \n");
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
                Log(LogTypes.CHANGES, filename);
                File.Copy(sourceFile, destFile,true);
                Log(LogTypes.SUCCESS, filename);

            }
            catch (Exception e)
            {
                Log(LogTypes.ERROR, filename);
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


        #endregion

    }
}
