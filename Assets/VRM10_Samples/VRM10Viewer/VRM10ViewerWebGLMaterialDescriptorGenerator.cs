using UniGLTF;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10ViewerWebGLMaterialDescriptorGenerator : IMaterialDescriptorGenerator
    {
        /// <summary>
        /// PBRとMToonのマテリアルをカスタマイズする。
        /// unlit は既存のものを使う。
        /// </summary>
        /// <param name="Lit"></param>
        /// <param name="MToon"></param>
        public VRM10ViewerWebGLMaterialDescriptorGenerator(Material Lit, Material MToon)
        {

        }

        public MaterialDescriptor Get(GltfData data, int i)
        {
            throw new System.NotImplementedException();
        }

        public MaterialDescriptor GetGltfDefault(string materialName = null)
        {
            throw new System.NotImplementedException();
        }
    }
}