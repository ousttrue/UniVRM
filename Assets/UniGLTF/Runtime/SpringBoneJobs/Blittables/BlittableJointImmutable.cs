using System;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Reconstruct に対して Immutable。
    /// Jointの増減、初期姿勢の変更など構成の変更は Reconstruct が必要。
    /// 変わりにくいスコープ。
    /// </summary>
    [Serializable]
    public struct BlittableJointImmutable
    {
        public int headTransformIndex;
        public int tailTransformIndex;
        public float length;
        public Quaternion initRotation;
        public Vector3 initPosition;

        // initPosition.noamlized of tail
        public Vector3 boneAxis;

        public int parentJointIndex;
        public int headJointIndex;
        public int tailJointIndex;

        public void DrawGizmo(BlittableTransform t, BlittableJointMutable m)
        {
            Gizmos.matrix = t.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Vector3.zero, m.radius);
        }
    }
}