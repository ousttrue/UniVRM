using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.SpringBoneJobs.Blittables;
using UnityEngine;


namespace UniGLTF.SpringBoneJobs.InputPorts
{
    public static class FastSpringBoneBufferBuilder
    {
        static List<BlittableSpring> blittableSprings = new();
        static List<BlittableJointMutable> blittableJoints = new();
        static List<BlittableCollider> blittableColliders = new();
        static List<BlittableJointImmutable> blittableLogics = new();
        public static FastSpringBoneBuffer Flattern(Transform model, FastSpringBoneSpring[] springs)
        {
            blittableSprings.Clear();
            blittableJoints.Clear();
            blittableColliders.Clear();
            blittableLogics.Clear();

            var transforms = MakeFlattenTransformList(springs);
            foreach (var spring in springs)
            {
                var blittableSpring = new BlittableSpring
                {
                    colliderSpan = new BlittableSpan
                    {
                        startIndex = blittableColliders.Count,
                        count = spring.colliders.Length,
                    },
                    logicSpan = new BlittableSpan
                    {
                        startIndex = blittableJoints.Count,
                        count = spring.joints.Length - 1,
                    },
                    centerTransformIndex = Array.IndexOf(transforms, spring.center),
                };
                blittableSprings.Add(blittableSpring);

                foreach (var collider in spring.colliders)
                {
                    var blittable = collider.Collider;
                    blittable.transformIndex = Array.IndexOf(transforms, collider.Transform);
                    blittableColliders.Add(blittable);
                }

                for (int i = 0; i < spring.joints.Length - 1; ++i)
                {
                    var joint = spring.joints[i];
                    var blittable = joint.Joint;
                    blittableJoints.Add(blittable);
                }

                // vrm-1.0 では末端の joint は tail で処理対象でないのに注意!
                for (int i = 0; i < spring.joints.Length - 1; ++i)
                {
                    var joint = spring.joints[i];
                    var tailJoint = spring.joints[i + 1];
                    var localPosition = tailJoint.Transform.localPosition;

                    var scale = tailJoint.Transform.lossyScale;
                    var localChildPosition = new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                        );

                    var parentJoint = spring.GetClosestParentJointIndex(joint);
                    if (parentJoint.HasValue && parentJoint.Value + 1 != i)
                    {
                        Debug.Log($"枝 [{i}] {parentJoint.Value} => {spring.joints[parentJoint.Value].Transform}");
                    }

                    blittableLogics.Add(new BlittableJointImmutable
                    {
                        headTransformIndex = Array.IndexOf(transforms, joint.Transform),
                        parentTransformIndex = Array.IndexOf(transforms, joint.Transform.parent),
                        tailTransformIndex = Array.IndexOf(transforms, tailJoint.Transform),
                        localRotation = joint.DefaultLocalRotation,
                        boneAxis = localChildPosition.normalized,
                        length = localChildPosition.magnitude,
                    });
                }
            }

            return new FastSpringBoneBuffer(model, transforms,
                blittableSprings.ToArray(),
                blittableJoints.ToArray(),
                blittableColliders.ToArray(),
                blittableLogics.ToArray());
        }

        /// <summary>
        /// Joint, Collider, Center の Transform のリスト
        /// - 重複を除去
        /// - Jointに関しては parent も登録
        /// この Array により各 Transform の index を決める 
        /// </summary>
        /// <param name="springs"></param>
        /// <returns></returns>
        static Transform[] MakeFlattenTransformList(FastSpringBoneSpring[] springs)
        {
            var transformHashSet = new HashSet<Transform>();
            foreach (var spring in springs)
            {
                foreach (var joint in spring.joints)
                {
                    transformHashSet.Add(joint.Transform);
                    if (joint.Transform.parent != null) transformHashSet.Add(joint.Transform.parent);
                }
                foreach (var collider in spring.colliders)
                {
                    transformHashSet.Add(collider.Transform);
                }
                if (spring.center != null) transformHashSet.Add(spring.center);
            }
            return transformHashSet.ToArray();
        }
    }
}