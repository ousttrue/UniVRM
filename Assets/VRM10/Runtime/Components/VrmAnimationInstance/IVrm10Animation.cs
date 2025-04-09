using System;
using System.Collections.Generic;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public interface IVrm10Animation : IDisposable
    {
        (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; }
        IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap { get; }
        LookAtInput? LookAt { get; }
        Animator Animator { get; }

        /// <summary>
        /// 骨格描画用。
        /// SkinnedMeshRenderer.sharedMesh の管理に注意
        /// </summary>
        public SkinnedMeshRenderer MakeBoxMan()
        {
            var animator = this.Animator;
            if (animator == null)
            {
                return null;
            }
            var BoxMan = SkeletonMeshUtility.CreateRenderer(animator);
            return BoxMan;
        }
    }
}