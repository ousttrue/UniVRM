using System;
using System.Collections.Generic;
using System.IO;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10.Cloth.Viewer
{
    public class BvhMotion : IVrm10Animation
    {
        BvhImporterContext m_context;
        public Transform Root => m_context?.Root.transform;
        (INormalizedPoseProvider, ITPoseProvider) m_controlRig;
        (INormalizedPoseProvider, ITPoseProvider) IVrm10Animation.ControlRig => m_controlRig;
        IDictionary<ExpressionKey, Func<float>> _ExpressionMap = new Dictionary<ExpressionKey, Func<float>>();
        public IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap => (IReadOnlyDictionary<ExpressionKey, Func<float>>)_ExpressionMap;

        public LookAtInput? LookAt { get; set; }

        public Animator Animator => m_context.Root.GetComponent<Animator>();

        public BvhMotion(BvhImporterContext context)
        {
            m_context = context;
            var provider = new AnimatorPoseProvider(m_context.Root.transform, m_context.Root.GetComponent<Animator>());
            m_controlRig = (provider, provider);
        }

        public static BvhMotion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new BvhMotion(context);
        }
        public static BvhMotion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        public void Dispose()
        {
            GameObject.Destroy(m_context.Root);
        }
    }
}
