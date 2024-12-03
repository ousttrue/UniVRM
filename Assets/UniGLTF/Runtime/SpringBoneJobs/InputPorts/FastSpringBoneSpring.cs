using System;
using System.Linq;
using UnityEngine;

namespace UniGLTF.SpringBoneJobs.InputPorts
{
    [Serializable]
    public struct FastSpringBoneSpring
    {
        public Transform center;
        public FastSpringBoneJoint[] joints;
        public FastSpringBoneCollider[] colliders;

        public int? GetClosestParentJointIndex(FastSpringBoneJoint joint)
        {
            // joints は親子順に sort 済みとする
            var parent = joints.Select((x, i) => (x, i)).Where(xi => xi.x.Transform != joint.Transform && joint.Transform.IsChildOf(xi.x.Transform)).LastOrDefault();
            if (parent.x.Transform == null)
            {
                return default;
            }
            return parent.i;
        }
    }
}