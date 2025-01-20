using UnityEngine;
using UnityEngine.Playables;


namespace UniVRM10.Vrm10Track
{
    public class VrmaBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Debug.Log($"Time: {playable.GetTime()}");
        }
    }
}