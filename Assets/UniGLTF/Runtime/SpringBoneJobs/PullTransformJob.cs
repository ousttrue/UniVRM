using Unity.Collections;
using UnityEngine.Jobs;
using UniGLTF.SpringBoneJobs.Blittables;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif

namespace UniGLTF.SpringBoneJobs
{

#if ENABLE_SPRINGBONE_BURST
    [BurstCompile]
#endif
    public struct PullTransformJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<BlittableTransform> Transforms;

        public void Execute(int index, TransformAccess transform)
        {
            Transforms[index] = BlittableTransform.FromTransformAccess(transform);
        }
    }
}
    