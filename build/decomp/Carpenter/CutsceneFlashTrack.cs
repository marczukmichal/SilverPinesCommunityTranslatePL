using UnityEngine.Timeline;

[TrackColor(0.1f, 0.1f, 0.1f)]
[TrackClipType(typeof(CutsceneFlashImageClip))]
[TrackClipType(typeof(CutsceneFlashBlackClip))]
[TrackBindingType(typeof(CutsceneFlashes))]
public class CutsceneFlashTrack : TrackAsset
{
}
