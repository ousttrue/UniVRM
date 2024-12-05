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
                    parentTransformIndex = Array.IndexOf(transforms, spring.joints[0].Transform.parent),
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

                for (int tailJointIndex = 1; tailJointIndex < spring.joints.Length; ++tailJointIndex)
                {
                    var tailJoint = spring.joints[tailJointIndex];
                    var headJointIndex = spring.GetClosestParentJointIndex(tailJoint);
                    if (headJointIndex == -1)
                    {
                        // データ不正。spring.joints[0] の子孫でない？
                        continue;
                    }
                    var joint = spring.joints[headJointIndex];

                    // v0.128.1 joint が連続でない場合対応
                    var localPosition = joint.Transform.worldToLocalMatrix.MultiplyPoint(tailJoint.Transform.position);

                    var scale = tailJoint.Transform.lossyScale;
                    var localChildPosition = new Vector3(
                            localPosition.x * scale.x,
                            localPosition.y * scale.y,
                            localPosition.z * scale.z
                        );

                    var parentJointIndex = spring.GetClosestParentJointIndex(joint);
                    if (parentJointIndex != -1 && parentJointIndex + 1 != headJointIndex)
                    {
                        // 枝がある場合は連番でない
                        Debug.Log($"枝 [{tailJointIndex}] {parentJointIndex} => {spring.joints[headJointIndex].Transform}");
                    }

                    blittableLogics.Add(new BlittableJointImmutable
                    {
                        headTransformIndex = Array.IndexOf(transforms, joint.Transform),
                        tailTransformIndex = Array.IndexOf(transforms, tailJoint.Transform),
                        initRotation = joint.DefaultLocalRotation,
                        initPosition = joint.DefaultLocalPosition,
                        boneAxis = localChildPosition.normalized,
                        length = localChildPosition.magnitude,
                        parentJointIndex = parentJointIndex,
                        headJointIndex = headJointIndex,
                        tailJointIndex = tailJointIndex,
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
                // root parent
                transformHashSet.Add(spring.joints[0].Transform.parent);
                // joints
                foreach (var joint in spring.joints)
                {
                    transformHashSet.Add(joint.Transform);
                }
                // colliders
                foreach (var collider in spring.colliders)
                {
                    transformHashSet.Add(collider.Transform);
                }
                // center
                if (spring.center != null)
                {
                    transformHashSet.Add(spring.center);
                }
            }
            return transformHashSet.ToArray();
        }
    }
}