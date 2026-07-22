using System;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;
using VRM10.MToon10;


namespace UniVRM10
{
    public enum ShaderPropertyType
    {
        //
        // 概要:
        //     Color Property.
        Color = 0,
        //
        // 概要:
        //     Vector Property.
        Vector = 1,
        //
        // 概要:
        //     Float Property.
        Float = 2,
        //
        // 概要:
        //     Range Property.
        Range = 3,
        //
        // 概要:
        //     Texture Property.
        TexEnv = 4
    }

    [Serializable]
    public struct PropItem
    {
        public string Name;
        public ShaderPropertyType PropertyType;
        public Vector4 DefaultValues;
    }

    /// <summary>
    /// Material 一つ分のプロパティを蓄えている
    /// </summary>
    [Serializable]
    public class MaterialItem
    {
        public readonly Material Material;
        public Vector4 DefaultUVScaleOffset;
        public Dictionary<UniGLTF.Extensions.VRMC_vrm.MaterialColorType, PropItem> PropMap = new();

        public MaterialItem(Material material)
        {
            Material = material;

            // uv default value
            var s = material.mainTextureScale;
            var o = material.mainTextureOffset;
            DefaultUVScaleOffset = new(s.x, s.y, o.x, o.y);
        }

        public void Clear()
        {
            // clear Color
            foreach (var _kv in PropMap)
            {
                Material.SetColor(_kv.Value.Name, _kv.Value.DefaultValues);
            }

            // clear UV
            Material.mainTextureScale = new(DefaultUVScaleOffset.x, DefaultUVScaleOffset.y);
            Material.mainTextureOffset = new(DefaultUVScaleOffset.z, DefaultUVScaleOffset.w);
        }

        public void AddScaleOffset(Vector4 scaleOffset, float weight)
        {
            var s = Material.mainTextureScale;
            var o = Material.mainTextureOffset;
            var value = new Vector4(s.x, s.y, o.x, o.y);
            value += (scaleOffset - DefaultUVScaleOffset) * weight;
            Material.mainTextureOffset = new(value.z, value.w);
            Material.mainTextureScale = new(value.x, value.y);
        }
    }

    /// <summary>
    /// 複数のMaterial のプロパティを保持する
    ///
    /// * PreviewSceneManager で使う
    /// * MaterialValueBindingMerger で使う
    ///
    /// </summary>
    [Serializable]
    public sealed class PreviewMaterialItem
    {
        /// <summary>
        /// https://github.com/vrm-c/UniVRM/pull/2685 により、ひとつの sharedMaterial から複数のコピーが派生しうる。
        /// すべてのコピーを保持できるように修正した。
        /// https://github.com/vrm-c/UniVRM/issues/2769
        /// `v0.131.2`
        /// </summary>
        public readonly List<MaterialItem> Materials = new();

        public PreviewMaterialItem(Material material)
        {
            Materials.Add(new MaterialItem(material));
        }

        public string[] PropNames
        {
            get;
            set;
        }

        public static readonly string COLOR_PROPERTY = MToon10Prop.BaseColorFactor.ToUnityShaderLabName();
        public static readonly string EMISSION_COLOR_PROPERTY = MToon10Prop.EmissiveFactor.ToUnityShaderLabName();
        public static readonly string RIM_COLOR_PROPERTY = MToon10Prop.ParametricRimColorFactor.ToUnityShaderLabName();
        public static readonly string OUTLINE_COLOR_PROPERTY = MToon10Prop.OutlineColorFactor.ToUnityShaderLabName();
        public static readonly string SHADE_COLOR_PROPERTY = MToon10Prop.ShadeColorFactor.ToUnityShaderLabName();
        public static readonly string MATCAP_COLOR_PROPERTY = MToon10Prop.MatcapColorFactor.ToUnityShaderLabName();

        public static bool TryGetBindType(string property, out MaterialColorType type)
        {
            if (property == COLOR_PROPERTY)
            {
                type = MaterialColorType.color;
            }
            else if (property == EMISSION_COLOR_PROPERTY)
            {
                type = MaterialColorType.emissionColor;
            }
            else if (property == RIM_COLOR_PROPERTY)
            {
                type = MaterialColorType.rimColor;
            }
            else if (property == OUTLINE_COLOR_PROPERTY)
            {
                type = MaterialColorType.outlineColor;
            }
            else if (property == SHADE_COLOR_PROPERTY)
            {
                type = MaterialColorType.shadeColor;
            }
            else if (property == MATCAP_COLOR_PROPERTY)
            {
                type = MaterialColorType.matcapColor;
            }
            else
            {
                type = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// [Preview] 積算する前の初期値にクリアする
        /// </summary>
        public void Clear()
        {
            foreach (var item in Materials)
            {
                item.Clear();
            }
        }

        /// <summary>
        /// [Preview] scaleOffset を weight で重みを付けて加える
        /// </summary>
        /// <param name="scaleOffset"></param>
        /// <param name="weight"></param>
        public void AddScaleOffset(Vector4 scaleOffset, float weight)
        {
            foreach (var item in Materials)
            {
                item.AddScaleOffset(scaleOffset, weight);
            }
        }
    }
}