using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
#if USE_COM_UNITY_CLOUD_KTX
using KtxUnity;
#endif

namespace UniGLTF
{
    public sealed class KtxTextureDeserializer : ITextureDeserializer
    {
#pragma warning disable 1998
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
#pragma warning restore 1998
        {
#if USE_COM_UNITY_CLOUD_KTX
            if (!textureInfo.ImageData.IsCreated) return null;

            // NOTE: IAwaitCaller を無視するので、同期読み込みを期待する環境で同期読み込みができない
            try
            {
                var ktxTexture = new KtxTexture();
                // NOTE: GltfData が保持する NativeArray をそのまま渡す。
                //       KtxUnity は Open() 内で同期的に読み取るだけで所有権を取らないため、コピー不要。
                var result = await ktxTexture.LoadFromBytes(
                    textureInfo.ImageData,
                    linear: textureInfo.ColorSpace == ColorSpace.Linear,
                    mipChain: textureInfo.UseMipmap
                );
                if (result is { errorCode: ErrorCode.Success })
                {
                    result.texture.wrapModeU = textureInfo.WrapModeU;
                    result.texture.wrapModeV = textureInfo.WrapModeV;
                    result.texture.filterMode = textureInfo.FilterMode;
                    return result.texture;
                }

                return null;
            }
            catch (Exception e)
            {
                UniGLTFLogger.Exception(e);
                return null;
            }
#else
            return null;
#endif
        }
    }
}