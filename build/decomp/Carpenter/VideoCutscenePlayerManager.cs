using UnityEngine;

public class VideoCutscenePlayerManager : MonoBehaviour
{
	[SerializeField]
	private GameObject m_videoCanvasPrefab;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayVideoCutscene.Register(StartVideoClip);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayVideoCutscene.Unregister(StartVideoClip);
	}

	private void StartVideoClip(VideoMetadata video)
	{
		Object.Instantiate(m_videoCanvasPrefab).GetComponent<VideoController>().StartVideoClip(video);
	}
}
