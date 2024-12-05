using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.Utils;
using UnityEngine;
using UnityEngine.Profiling;


namespace UniVRM10
{
    public static class FastSpringBoneBufferFactory
    {
        static List<FastSpringBoneSpring> springs = new();
        static List<FastSpringBoneJoint> joints = new();
        static List<FastSpringBoneCollider> colliders = new();
        static HashSet<VRM10SpringBoneCollider> colliderSet = new();

        /// <summary>
        /// このVRMに紐づくSpringBone関連のバッファを構築する。
        /// </summary>
        /// <param name="awaitCaller"></param>
        /// <param name="fastSpringBoneBuffer">TODO: 再利用する</param>
        /// <returns></returns>
        public static async Task<FastSpringBoneBuffer> ConstructSpringBoneAsync(IAwaitCaller awaitCaller, Vrm10Instance vrm,
            FastSpringBoneBuffer fastSpringBoneBuffer = null)
        {
            // TODO: Dispose せずに再利用する最適化
            // new FastSpringBoneBuffer にも構築ロジックがあるので合体して整理する必要あり。
            // GC 軽減と await 挟み込み
            if (fastSpringBoneBuffer != null)
            {
                fastSpringBoneBuffer.Dispose();
                fastSpringBoneBuffer = null;
            }

            Func<Transform, TransformState> GetOrAddDefaultTransformState = (Transform tf) =>
            {
                if (vrm.DefaultTransformStates.TryGetValue(tf, out var defaultTransformState))
                {
                    return defaultTransformState;
                }

                Debug.LogWarning($"{tf.name} does not exist on load.");
                return new TransformState(null);
            };

            Profiler.BeginSample("FastSpringBone.ConstructSpringBoneAsync");

            Profiler.BeginSample("FastSpringBone.ConstructSpringBoneAsync springs");
            springs.Clear();
            foreach (var spring in vrm.SpringBone.Springs)
            {
                if (spring == null)
                {
                    // Debug.LogWarning("null spring", vrm.transform);
                    continue;
                }
                if (spring.Joints.Count == 0)
                {
                    // Debug.LogWarning("empty spring", vrm.transform);
                    continue;
                }

                joints.Clear();

                // JOINT が hierarchy 順に並んでいること確実にする
                var root = spring.Joints[0].transform;
                foreach (var transform in root.GetComponentsInChildren<Transform>())
                {
                    var joint = spring.GetJointForTransform(transform);
                    if (joint == null)
                    {
                        continue;
                    }
                    if (joints.Any(x => x.Transform == joint.transform))
                    {
                        // Debug.LogWarning("dup joint", joint);
                        continue;
                    }

                    joints.Add(new FastSpringBoneJoint
                    {
                        Transform = joint.transform,
                        Joint = new BlittableJointMutable
                        {
                            radius = joint.m_jointRadius,
                            dragForce = joint.m_dragForce,
                            gravityDir = joint.m_gravityDir,
                            gravityPower = joint.m_gravityPower,
                            stiffnessForce = joint.m_stiffnessForce
                        },
                        DefaultLocalRotation = GetOrAddDefaultTransformState(joint.transform).LocalRotation,
                        DefaultLocalPosition = GetOrAddDefaultTransformState(joint.transform).LocalPosition,
                    });
                }

                colliders.Clear();
                colliderSet.Clear();
                foreach (var colliderGroup in spring.ColliderGroups)
                {
                    if (colliderGroup == null)
                    {
                        // Debug.LogWarning("null collider group", vrm);
                        continue;
                    }
                    if (colliderGroup.Colliders.Count == 0)
                    {
                        // Debug.LogWarning("empty colliderGroup", colliderGroup);
                        continue;
                    }

                    foreach (var collider in colliderGroup.Colliders)
                    {
                        if (collider == null)
                        {
                            // Debug.LogWarning("null collider", colliderGroup);
                            continue; ;
                        }
                        if (colliderSet.Contains(collider))
                        {
                            // Debug.LogWarning("dup collider", collider);
                            continue;
                        }

                        colliderSet.Add(collider);
                        colliders.Add(new FastSpringBoneCollider
                        {
                            Transform = collider.transform,
                            Collider = new BlittableCollider
                            {
                                offset = collider.Offset,
                                radius = collider.Radius,
                                tailOrNormal = collider.TailOrNormal,
                                colliderType = TranslateColliderType(collider.ColliderType)
                            }
                        });
                    }
                }

                springs.Add(new FastSpringBoneSpring
                {
                    joints = joints.ToArray(),
                    colliders = colliders.ToArray(),
                    center = spring.Center,
                });
            }
            Profiler.EndSample();

            await awaitCaller.NextFrame();

            Profiler.BeginSample("FastSpringBone.ConstructSpringBoneAsync FastSpringBoneBufferBuilder.Flattern");
            var buf = FastSpringBoneBufferBuilder.Flattern(vrm.transform, springs.ToArray());
            Profiler.EndSample();

            Profiler.EndSample();
            return buf;
        }

        private static BlittableColliderType TranslateColliderType(VRM10SpringBoneColliderTypes colliderType)
        {
            switch (colliderType)
            {
                case VRM10SpringBoneColliderTypes.Sphere:
                    return BlittableColliderType.Sphere;
                case VRM10SpringBoneColliderTypes.Capsule:
                    return BlittableColliderType.Capsule;
                case VRM10SpringBoneColliderTypes.Plane:
                    return BlittableColliderType.Plane;
                case VRM10SpringBoneColliderTypes.SphereInside:
                    return BlittableColliderType.SphereInside;
                case VRM10SpringBoneColliderTypes.CapsuleInside:
                    return BlittableColliderType.CapsuleInside;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}