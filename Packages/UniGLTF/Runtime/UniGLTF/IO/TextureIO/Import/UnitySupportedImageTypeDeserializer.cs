using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Unity の ImageConversion.LoadImage を用いて PNG/JPG の読み込みを実現する
    /// </summary>
    public sealed class UnitySupportedImageTypeDeserializer : ITextureDeserializer
    {
        /// <summary>
        /// `true` を指定すると、テクスチャを Non-Readable なものとしてデシリアライズする。
        /// </summary>
        /// <remarks>
        /// `UnityEngine.ImageConversion.LoadImage` の第二引数 `markNonReadable` に相当。
        /// テクスチャ編集を行わないアプリケーションプログラム等では、
        /// この値を `true` にすることでメモリ使用量の削減を期待できる。
        /// このフラグの効用については `UnityEngine.Texture2D.Apply` に記述がある。
        /// 
        /// v0.128.4 ImportedTexturesAccessibility を参照
        /// </remarks>
        public ImportedTexturesAccessibility ImportedTexturesAccessibility { get; } = ImportedTexturesAccessibility.Auto;

        public UnitySupportedImageTypeDeserializer(ImportedTexturesAccessibility importedTexturesAccessibility)
        {
            ImportedTexturesAccessibility = importedTexturesAccessibility;
        }

        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            if (!textureInfo.ImageData.IsCreated) return null;

            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, textureInfo.UseMipmap, textureInfo.ColorSpace == ColorSpace.Linear);
#if UNITY_6000_0_OR_NEWER
                // Unity 6000.0.42f1 以降は LoadImage が ReadOnlySpan を受け取れるため、NativeArray からのコピーを避ける。
                texture.LoadImage(textureInfo.ImageData.AsReadOnlySpan(), ImportedTexturesAccessibility.ToMarkNonReadable());
#else
                texture.LoadImage(textureInfo.ImageData.ToArray(), ImportedTexturesAccessibility.ToMarkNonReadable());
#endif
                await awaitCaller.NextFrame();

                texture.wrapModeU = textureInfo.WrapModeU;
                texture.wrapModeV = textureInfo.WrapModeV;
                texture.filterMode = textureInfo.FilterMode;
                return texture;
            }
            catch (Exception e)
            {
                UniGLTFLogger.Exception(e);
                return null;
            }
        }
    }
}