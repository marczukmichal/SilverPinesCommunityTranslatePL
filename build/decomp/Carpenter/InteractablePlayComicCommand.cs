using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class InteractablePlayComicCommand : BaseInteractableCommand
{
	[SerializeField]
	private ComicMetadata m_comic;

	private bool m_finishedComic;

	public static InteractablePlayComicCommand CreateInstance(ComicMetadata comicMetadata)
	{
		return new InteractablePlayComicCommand
		{
			m_comic = comicMetadata
		};
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		if (m_comic == null)
		{
			Debug.LogWarning("InteractablePlayComicCommand is missing a voiced event, don't do anything");
			yield return null;
		}
		bool flag = false;
		m_finishedComic = false;
		if (GameDebugCommands.SKIP_STORY)
		{
			flag = true;
		}
		if (!flag)
		{
			GlobalReferences.Instance.EventChannels.Comic.PlayComic.Raise(m_comic);
			GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Register(OnComicStateChanged);
			yield return new WaitUntil(() => m_finishedComic);
		}
		OnExit();
	}

	private void OnComicStateChanged(bool isPlaying)
	{
		if (!isPlaying)
		{
			m_finishedComic = true;
		}
	}

	private void OnExit()
	{
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Unregister(OnComicStateChanged);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}
}
