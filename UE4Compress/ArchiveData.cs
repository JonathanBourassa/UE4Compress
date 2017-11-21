using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4Compress
{
    public class ArchiveData
    {
        public FileHeader FileHeader { get; set; }

        public List<byte[]> Chunks { get; set; }

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
    }
}
