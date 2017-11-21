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
                Console.WriteLine("Usage: EU4Compress.exe (-c|-d) FilePath OutputFolder");
                Console.WriteLine("Example: UE4Compress.exe -c \"C:\\example.uasset\" \"C:\\CompressedFolder");
                Console.WriteLine("   Will compress example.uasset to output folder C:\\CompressedFolder");
                Console.WriteLine("   C:\\CompressedFolder will contain two new file name:");
                Console.WriteLine("      example.uasset.z");
                Console.WriteLine("      example.uasset.z.uncompressed_size");
            }
            if ((args.Length >= 2 || args.Length <= 3 ) && (args[0].ToLower().Equals("-c") || args[0].ToLower().Equals("-d")))
            {
                if (!File.Exists(args[1]))
                {
                    Console.WriteLine($"The file {args[1]} does not exits.");
                    Environment.Exit(1);
                }
                var sourceFileData = File.ReadAllBytes(args[1]);
                if (args[0].ToLower().Equals("-c"))
                {
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
                else
                {
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
            }
        }
    }
}
