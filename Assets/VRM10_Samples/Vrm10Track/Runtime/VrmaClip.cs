using UnityEngine;
using UnityEngine.Playables;


namespace UniVRM10.Vrm10Track
{
    public class VrmaClip : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return ScriptPlayable<VrmaBehaviour>.Create(graph);
        }
    }
}