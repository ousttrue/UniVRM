using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UniGLTF.SpringBoneJobs.Blittables;


namespace UniGLTF.SpringBoneJobs.InputPorts
{
    /// <summary>
    /// ひとつのVRMに紐づくFastSpringBoneに関連したバッファを保持するクラス
    /// </summary>
    public class FastSpringBoneBuffer : IDisposable
    {
        /// <summary>
        /// model root
        /// </summary>
        public Transform Model { get; }
        // NOTE: これらはFastSpringBoneBufferCombinerによってバッチングされる
        public NativeArray<BlittableSpring> Springs { get; }
        public NativeArray<BlittableJointMutable> Joints { get; }
        public NativeArray<BlittableCollider> Colliders { get; }
        public NativeArray<BlittableJointImmutable> Logics { get; }
        private NativeArray<Vector3> _currentTailsBackup;
        private NativeArray<Vector3> _nextTailsBackup;
        public Transform[] Transforms { get; }

        public FastSpringBoneBuffer(Transform model, Transform[] transforms,
            BlittableSpring[] blittableSprings,
            BlittableJointMutable[] blittableJoints,
            BlittableCollider[] blittableColliders,
            BlittableJointImmutable[] blittableLogics
        )
        {
            Model = model;
            Transforms = transforms;
            Springs = new NativeArray<BlittableSpring>(blittableSprings, Allocator.Persistent);
            Joints = new NativeArray<BlittableJointMutable>(blittableJoints, Allocator.Persistent);
            Colliders = new NativeArray<BlittableCollider>(blittableColliders, Allocator.Persistent);
            Logics = new NativeArray<BlittableJointImmutable>(blittableLogics, Allocator.Persistent);
        }

        public void BackupCurrentTails(NativeArray<Vector3> currentTails, NativeArray<Vector3> nextTails, int offset)
        {
            if (!Logics.IsCreated || Logics.Length == 0)
            {
                return;
            }
            if (!_currentTailsBackup.IsCreated)
            {
                _currentTailsBackup = new(Logics.Length, Allocator.Persistent);
            }
            if (!_nextTailsBackup.IsCreated)
            {
                _nextTailsBackup = new(Logics.Length, Allocator.Persistent);
            }
            NativeArray<Vector3>.Copy(currentTails, offset, _currentTailsBackup, 0, Logics.Length);
            NativeArray<Vector3>.Copy(nextTails, offset, _nextTailsBackup, 0, Logics.Length);
        }

        public void RestoreCurrentTails(NativeArray<Vector3> currentTails, NativeArray<Vector3> nextTails, int offset)
        {
            if (_currentTailsBackup.IsCreated)
            {
                NativeArray<Vector3>.Copy(_currentTailsBackup, 0, currentTails, offset, Logics.Length);
                NativeArray<Vector3>.Copy(_nextTailsBackup, 0, nextTails, offset, Logics.Length);
            }
            else
            {
                var end = offset + Logics.Length;
                for (int i = offset; i < end; ++i)
                {
                    // mark velocity zero
                    currentTails.GetSubArray(offset, Logics.Length).AsSpan().Fill(new Vector3(float.NaN, float.NaN, float.NaN));
                }
            }
        }

        public void Dispose()
        {
            if (Springs.IsCreated) Springs.Dispose();
            if (Joints.IsCreated) Joints.Dispose();
            if (Colliders.IsCreated) Colliders.Dispose();
            if (Logics.IsCreated) Logics.Dispose();
            if (_currentTailsBackup.IsCreated) _currentTailsBackup.Dispose();
            if (_nextTailsBackup.IsCreated) _nextTailsBackup.Dispose();
        }
    }
}