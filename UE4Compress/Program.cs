using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4Compress
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid Arguments");
                Environment.Exit(1);
            }
            if ((args.Length >= 2 || args.Length <= 4 ) && (args[0].ToLower().Equals("-c") || args[0].ToLower().Equals("-d") || args[0].ToLower().Equals("-cmod")))
            {
                if (!args[0].ToLower().Equals("-cmod") && !File.Exists(args[1]))
                {
                    Console.WriteLine($"The file {args[1]} does not exits.");
                    Environment.Exit(1);
                }
                if (args[0].ToLower().Equals("-c"))
                {
                    var sourceFileData = File.ReadAllBytes(args[1]);
                    var archiver = new ArchiveData();
                    var compressedData = archiver.Compress(sourceFileData);
                    var fileName = Path.GetFileName(args[1]);
                    var destinationFolder = args[1].Replace($"\\{fileName}", "");
                    if (args.Length == 3)
                    {
                        if (!Directory.Exists(args[2]))
                            Directory.CreateDirectory(args[2]);
                        destinationFolder = args[2];
                    }
                    File.WriteAllBytes($"{destinationFolder}\\{fileName}.z", compressedData);
                    File.WriteAllText($"{destinationFolder}\\{fileName}.z.uncompressed_size", $"{archiver.FileHeader.UncompressedSize}\r\n");
                }
                else if(args[0].ToLower().Equals("-d"))
                {
                    var sourceFileData = File.ReadAllBytes(args[1]);
                    var archiver = new ArchiveData(sourceFileData);
                    var uncompressedData = archiver.Decompress();
                    var fileName = Path.GetFileName(args[1]).Replace(".z", "");
                    var destinationFolder = args[1].Replace($"\\{fileName}", "");
                    if (args.Length == 3)
                    {
                        if (!Directory.Exists(args[2]))
                            Directory.CreateDirectory(args[2]);
                        destinationFolder = args[2];
                    }
                    File.WriteAllBytes($"{destinationFolder}\\{fileName}", uncompressedData);
                }
                else
                {
                    if (args.Length != 4)
                    {
                        Console.WriteLine("Invalid arguments.");
                        Console.WriteLine("Usage: UE4Compress.exe -cmod \"sourcePath\" \"destinationPath\"");
                        Environment.Exit(1);
                    }
                    if (!Directory.Exists(args[1]))
                    {
                        Console.WriteLine("Windows Source Folder does not exist.");
                        Environment.Exit(1);
                    }
                    if (!Directory.Exists(args[2]))
                    {
                        Console.WriteLine("Linux Source Folder does not exist.");
                        Environment.Exit(1);
                    }
                    var windowsSourceFolder = args[1].TrimEnd('\\');
                    var linuxSourceFolder = args[2].TrimEnd('\\');
                    var destinationDirectory = args[3].TrimEnd('\\');

                    var modFolderName = new DirectoryInfo(windowsSourceFolder).GetDirectories().FirstOrDefault();
                    if (modFolderName == null)
                    {
                        Console.WriteLine("Windows Source Folder is empty.");
                        Environment.Exit(1);
                    }
                    var windowsFileList = WalkDirectoryTree(modFolderName);
                    if (!Directory.Exists($"{destinationDirectory}\\WindowsNoEditor"))
                        Directory.CreateDirectory($"{destinationDirectory}\\WindowsNoEditor");
                    CompressModFolder(windowsFileList, $"{windowsSourceFolder}\\{modFolderName}", $"{destinationDirectory}\\WindowsNoEditor");

                    modFolderName = new DirectoryInfo(linuxSourceFolder).GetDirectories().FirstOrDefault();
                    if (modFolderName == null)
                    {
                        Console.WriteLine("Linux Source Folder is empty.");
                        Environment.Exit(1);
                    }
                    var lunuxFileList = WalkDirectoryTree(modFolderName);
                    if (!Directory.Exists($"{destinationDirectory}\\LinuxNoEditor"))
                        Directory.CreateDirectory($"{destinationDirectory}\\LinuxNoEditor");
                    CompressModFolder(lunuxFileList, $"{linuxSourceFolder}\\{modFolderName}" , $"{destinationDirectory}\\LinuxNoEditor");
                }
            }
            else if (args.Length == 1)
            {
                Console.WriteLine("Invalid argument.");
            }
            else
            {
                Console.WriteLine("Invalid command parameter. must either be -c or -d");
                Console.WriteLine("-c = compress");
                Console.WriteLine("-d = decompress");
                Console.WriteLine("-cmod compress whole mod folder just like Ark Dev Kit to destination directory");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private static void CompressModFolder(List<string> fileList, string sourceFolder, string destinationDirectory)
        {
            foreach (var file in fileList)
            {
                var fileName = Path.GetFileName(file);
                var sourceFileData = File.ReadAllBytes(file);
                if (string.IsNullOrEmpty(fileName)) continue;
                if (fileName.ToLower().Equals("mod.info") ||
                    fileName.ToLower().Equals("modmeta.info"))
                {
                    if (destinationDirectory.ToLower().Contains("linux"))
                        File.WriteAllBytes($"{destinationDirectory}\\..\\{fileName}", sourceFileData);
                    File.WriteAllBytes($"{destinationDirectory}\\{fileName}", sourceFileData);
                }
                else
                {
                    var finalPath = file.Replace(sourceFolder, destinationDirectory);
                    if (!Directory.Exists(finalPath.Replace($"\\{fileName}", string.Empty)))
                        Directory.CreateDirectory(finalPath.Replace($"\\{fileName}", string.Empty));
                    var archiver = new ArchiveData();
                    var destinationFileData = archiver.Compress(sourceFileData);
                    File.WriteAllBytes($"{finalPath}.z", destinationFileData);
                    File.WriteAllText($"{finalPath}.z.uncompressed_size", $"{archiver.FileHeader.UncompressedSize}\r\n");
                }
            }
        }

        private static List<string> WalkDirectoryTree(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            var fileList = new List<string>();
            if (files != null)
                fileList = files.Select(fi => fi.FullName).ToList();

            subDirs = root.GetDirectories();

            foreach (var dirInfo in subDirs)
            {
                fileList.AddRange(WalkDirectoryTree(dirInfo));
            }
            return fileList;
        }
    }
}
