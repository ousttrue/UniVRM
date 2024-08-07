using UniGLTF;

namespace VRM
{
    public static class VrmMaterialDescriptorGeneratorUtility
    {
        public static IMaterialDescriptorGenerator GetValidVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm)
        {
            return GetVrmMaterialDescriptorGenerator(vrm, RenderPipelineUtility.GetRenderPipelineType());
        }

        public static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(glTF_VRM_extensions vrm, RenderPipelineTypes renderPipelineType)
        {
            return renderPipelineType switch
            {
                RenderPipelineTypes.UniversalRenderPipeline => new UrpVrmMaterialDescriptorGenerator(vrm),
                RenderPipelineTypes.BuiltinRenderPipeline => new BuiltInVrmMaterialDescriptorGenerator(vrm),
                _ => new BuiltInVrmMaterialDescriptorGenerator(vrm),
            };
        }
    }
}