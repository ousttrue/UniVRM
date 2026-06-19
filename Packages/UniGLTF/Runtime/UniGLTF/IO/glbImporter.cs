using System;
using System.Collections.Generic;
using System.Text;


namespace UniGLTF
{
    public static class glbImporter
    {
        public const string GLB_MAGIC = "glTF";
        public const uint GLB_VERSION = 2;

        public static readonly ReadOnlyMemory<byte> GLB_MAGIC_BYTES = Encoding.ASCII.GetBytes(GLB_MAGIC);
        public static readonly ReadOnlyMemory<byte> GLB_JSON_BYTES = BitConverter.GetBytes((uint)GlbChunkType.JSON);
        public static readonly ReadOnlyMemory<byte> GLB_BIN_BYTES = BitConverter.GetBytes((uint)GlbChunkType.BIN);

        public static GlbChunkType ToChunkType(this string src)
        {
            switch (src)
            {
                case "BIN":
                    return GlbChunkType.BIN;

                case "JSON":
                    return GlbChunkType.JSON;

                default:
                    throw new FormatException("unknown chunk type: " + src);
            }
        }

        public static string ToChunkTypeString(this GlbChunkType type)
        {
            switch (type)
            {
                case GlbChunkType.JSON:
                    return "JSON";
                case GlbChunkType.BIN:
                    return "BIN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static GlbChunkType ToChunkType(this ReadOnlyMemory<byte> src)
        {
            if (src.Length != 4)
            {
                throw new FormatException("invalid chunk type: " + src);
            }
            if (src.Span.SequenceEqual(GLB_JSON_BYTES.Span))
            {
                return GlbChunkType.JSON;
            }
            if (src.Span.SequenceEqual(GLB_BIN_BYTES.Span))
            {
                return GlbChunkType.BIN;
            }
            throw new FormatException("unknown chunk type: " + src);
        }

        [Obsolete("Use ParseGlbChunks(bytes)")]
        public static List<GlbChunk> ParseGlbChanks(Byte[] bytes)
        {
            return ParseGlbChunks(bytes);
        }

        public static List<GlbChunk> ParseGlbChunks(ReadOnlyMemory<byte> bytes)
        {
            //
            // glb header(12byte)
            //
            if (bytes.Length < 12)
            {
                throw new GlbParseException("glb header not found");
            }

            int pos = 0;
            if (!bytes.Span.StartsWith(GLB_MAGIC_BYTES.Span))
            {
                throw new GlbParseException("invalid magic");
            }
            pos += 4;

            var version = BitConverter.ToUInt32(bytes[pos..].Span);
            if (version != GLB_VERSION)
            {
                throw new GlbParseException($"unknown version: {version}");
            }
            pos += 4;

            var totalLength = BitConverter.ToUInt32(bytes[pos..].Span);
            if (bytes.Length < totalLength)
            {
                throw new GlbParseException($"not enough size: {bytes.Length} < {totalLength}");
            }
            pos += 4;

            var chunks = new List<GlbChunk>();
            while (pos < bytes.Length)
            {
                var chunkDataSize = BitConverter.ToInt32(bytes[pos..].Span);
                pos += 4;

                var chunkTypeBytes = bytes.Slice(pos, 4);
                pos += 4;

                chunks.Add(new GlbChunk(chunkTypeBytes, bytes.Slice(pos, chunkDataSize)));

                pos += chunkDataSize;
            }

            return chunks;
        }
    }
}
