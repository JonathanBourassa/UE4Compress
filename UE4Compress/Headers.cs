using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4Compress
{
    public static class HeaderHelper
    {
        
        
    }

    public class FileHeader
    {
        public const int FileHeaderSize = 32;
        public const int ChunkHeaderSize = 16;
        public byte[] ECompressionFlags { get; set; }

        public long ChunkSize { get; set; }

        public long CompressedSize { get; set; }

        public long UncompressedSize { get; set; }

        public List<ChunkHeader> ChunkHeaders { get; set; }

        /// <summary>
        /// Construct FileHeader from saved file.
        /// </summary>
        /// <param name="fileData">Byte Array</param>
        public FileHeader(byte[] fileData)
        {
            ECompressionFlags = fileData.Take(8).ToArray();
            ChunkSize = BitConverter.ToInt64(fileData.Skip(8).Take(8).ToArray(), 0);
            CompressedSize = BitConverter.ToInt64(fileData.Skip(16).Take(8).ToArray(), 0);
            UncompressedSize = BitConverter.ToInt64(fileData.Skip(24).Take(8).ToArray(), 0);
            var uncompressedBuffer = ChunkSize;
            var chunkIndex = 0;
            ChunkHeaders = new List<ChunkHeader>();
            while (uncompressedBuffer == ChunkSize)
            {
                var chunkHeader = new ChunkHeader(fileData.Skip(FileHeaderSize + (ChunkHeaderSize * chunkIndex)).Take(ChunkHeaderSize).ToArray());
                uncompressedBuffer = chunkHeader.UncompressedBuffer;
                ChunkHeaders.Add(chunkHeader);
                chunkIndex++;
            }
        }

        /// <summary>
        /// Construct FileHeader from Compression Data.
        /// </summary>
        /// <param name="chunkSize">Max Size of chunk uncompressed.</param>
        /// <param name="compressedSize">Total Size of the File Compressed.</param>
        /// <param name="uncompressedSize">Total Size of the File Uncompressed.</param>
        /// <param name="chunkHeaders">Array of ChunkHeaders</param>
        public FileHeader(long chunkSize, long compressedSize, long uncompressedSize, List<ChunkHeader> chunkHeaders)
        {
            //ECompressionFlags. 
            //  Hardcoded the bytes because I don't know exactly what they are, 
            //  didn't find the ECompressionFlags class file but all cooked assets use the same set of flags.
            ECompressionFlags = new byte[] {193, 131, 42, 158, 0, 0, 0, 0};
            ChunkSize = chunkSize;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
            ChunkHeaders = chunkHeaders;
        }
    }

    public class ChunkHeader
    {
        public long CompressedBuffer { get; set; }

        public long UncompressedBuffer { get; set; }

        /// <summary>
        /// Construct Chunk Header From Saved File Data.
        /// </summary>
        /// <param name="chunkData">byte array</param>
        public ChunkHeader(byte[] chunkData)
        {
            CompressedBuffer = BitConverter.ToInt64(chunkData.Take(8).ToArray(), 0);
            UncompressedBuffer = BitConverter.ToInt64(chunkData.Skip(8).Take(8).ToArray(), 0);
        }

        /// <summary>
        /// Construct chunk Header from Compression Data.
        /// </summary>
        /// <param name="compressedBuffer">Size of the chunk compressed.</param>
        /// <param name="uncompressedBuffer">Size of the chunk Uncompressed.</param>
        public ChunkHeader(long compressedBuffer, long uncompressedBuffer)
        {
            CompressedBuffer = compressedBuffer;
            UncompressedBuffer = uncompressedBuffer;
        }
    }
}
