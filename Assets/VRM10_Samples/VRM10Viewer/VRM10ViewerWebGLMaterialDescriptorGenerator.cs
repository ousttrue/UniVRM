using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerWebGLMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        private readonly Material _Lit;
        private readonly Material _MToon;

        /// <summary>
        /// PBRとMToonのマテリアルをカスタマイズする。
        /// unlit は既存のものを使う。
        /// </summary>
        /// <param name="Lit"></param>
        /// <param name="MToon"></param>
        public VRM10ViewerWebGLMaterialDescriptorGenerator(Material Lit, Material MToon)
        {
            if (Lit == null) throw new ArgumentNullException("Lit");
            _Lit = Lit;

            if (MToon == null) throw new ArgumentNullException("MToon");
            _MToon = MToon;
        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            var matDesc = new MaterialDescriptor(
                $"Material#{i}",
                _Lit.shader,
                null,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>()
            );
            return matDesc;
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null)
        {
            // FIXME
            return new MaterialDescriptor(
                string.IsNullOrEmpty(materialName) ? "__default__" : materialName,
                _Lit.shader,
                default,
                new Dictionary<string, TextureDescriptor>(),
                new Dictionary<string, float>(),
                new Dictionary<string, Color>(),
                new Dictionary<string, Vector4>(),
                new List<Action<Material>>()
            );
        }
    }
}