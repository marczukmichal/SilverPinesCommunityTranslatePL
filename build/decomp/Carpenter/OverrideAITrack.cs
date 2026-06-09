using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(0.2f, 1f, 0.2f)]
[TrackClipType(typeof(OverrideAIClip))]
[TrackBindingType(typeof(GameObject))]
public class OverrideAITrack : TrackAsset
{
}
