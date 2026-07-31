using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;

namespace UniGLTF
{
    /// <summary>
    /// Read a file directly into a NativeArray via AsyncReadManager without a managed byte[].
    /// Ported from Unio (https://github.com/hadashiA/Unio) MIT License.
    /// </summary>
    public static class NativeFile
    {
        public static unsafe NativeArray<byte> ReadAllBytes(string filePath)
        {
            FileInfoResult fileInfoResult;
            var fileInfoHandle = AsyncReadManager.GetFileInfo(filePath, &fileInfoResult);
            fileInfoHandle.JobHandle.Complete();

            if (fileInfoResult.FileState == FileState.Absent)
            {
                throw new FileNotFoundException(filePath);
            }

            var size = fileInfoResult.FileSize;
            var buffer = (byte*)UnsafeUtility.Malloc(size, 16, Allocator.Persistent);
            var readCommand = new ReadCommand
            {
                Offset = 0,
                Size = size,
                Buffer = buffer,
            };

            var readHandle = AsyncReadManager.Read(filePath, &readCommand, 1);
            try
            {
                readHandle.JobHandle.Complete();
                if (readHandle.Status != ReadStatus.Complete)
                {
                    UnsafeUtility.Free(buffer, Allocator.Persistent);
                    throw new IOException($"Read operation failed ({readHandle.Status}). {filePath}");
                }
            }
            finally
            {
                if (readHandle.IsValid())
                {
                    readHandle.Dispose();
                }
            }

            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(buffer, (int)size, Allocator.Persistent);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
            return array;
        }
    }
}