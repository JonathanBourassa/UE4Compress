using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace UE4Compress
{
    public class ArchiveData
    {
        public FileHeader FileHeader { get; set; }

        public List<byte[]> Chunks { get; set; }

        public ArchiveData()
        {
            FileHeader = new FileHeader(131072, 0, 0, new List<ChunkHeader>());
            Chunks = new List<byte[]>();
        }

        public ArchiveData(byte[] archiveData)
        {
            FileHeader = new FileHeader(archiveData);
            Chunks = new List<byte[]>();
            var chunkStartIndex = (long)FileHeader.FileHeaderSize + FileHeader.ChunkHeaderSize * FileHeader.ChunkHeaders.Count;
            foreach (var chunkHeader in FileHeader.ChunkHeaders)
            {
                var curChunkData = new byte[chunkHeader.CompressedBuffer];
                Array.Copy(archiveData, chunkStartIndex, curChunkData, 0, chunkHeader.CompressedBuffer);
                Chunks.Add(curChunkData);
                chunkStartIndex += chunkHeader.CompressedBuffer;
            }
        }

        public byte[] Decompress()
        {
            if (Chunks.Count <= 0) throw new Exception("Archive is empty.");
            var data = new List<byte>();
            foreach (var chunk in Chunks)
            {
                data.AddRange(Ionic.Zlib.ZlibStream.UncompressBuffer(chunk));
            }
            return data.ToArray();
        }

        public byte[] Compress(byte[] data)
        {
            FileHeader.UncompressedSize = data.LongLength;
            var nbOfChunks = data.LongLength / FileHeader.ChunkSize;
            if (nbOfChunks * FileHeader.ChunkSize < data.LongLength) nbOfChunks++;
            var compress = new Func<byte[], byte[]>(a => {
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var compressor =
                        new ZlibStream(ms,
                            CompressionMode.Compress,
                            CompressionLevel.Default))
                    {
                        compressor.Write(a, 0, a.Length);
                    }

                    return ms.ToArray();
                }
            });
            for (var i = 0; i < nbOfChunks; i++)
            {
                var curChunkStart = i * FileHeader.ChunkSize;
                var curChunkSize = curChunkStart + FileHeader.ChunkSize <= data.LongLength
                    ? FileHeader.ChunkSize
                    : data.LongLength - curChunkStart;
                var decompressedChunk = new byte[curChunkSize];
                Array.Copy(data, curChunkStart, decompressedChunk, 0, curChunkSize);
                var compressedChunk = compress(decompressedChunk);
                FileHeader.ChunkHeaders.Add(new ChunkHeader(compressedChunk.LongLength, decompressedChunk.LongLength));
                Chunks.Add(compressedChunk);
            }
            FileHeader.CompressedSize = Chunks.Sum(b => b.LongLength);
            var serializedData = new List<byte>();
            serializedData.AddRange(FileHeader.ECompressionFlags);
            serializedData.AddRange(BitConverter.GetBytes(FileHeader.ChunkSize));
            serializedData.AddRange(BitConverter.GetBytes(FileHeader.CompressedSize));
            serializedData.AddRange(BitConverter.GetBytes(FileHeader.UncompressedSize));
            foreach (var chunkHeader in FileHeader.ChunkHeaders)
            {
                serializedData.AddRange(BitConverter.GetBytes(chunkHeader.CompressedBuffer));
                serializedData.AddRange(BitConverter.GetBytes(chunkHeader.UncompressedBuffer));
            }
            foreach (var chunk in Chunks)
            {
                serializedData.AddRange(chunk);
            }
            return serializedData.ToArray();
        }
    }
}
