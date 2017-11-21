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
            var testFile = File.ReadAllBytes(".\\RedWood_Tree_2_D_Test.uasset.z");
            var archive = new ArchiveData(testFile);
            Console.WriteLine($"This Archive has {archive.Chunks.Count} chunks.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
