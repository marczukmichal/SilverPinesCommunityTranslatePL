using System;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasErrorManager : MonoBehaviour
{
	private bool m_hasErrored;

	private void OnEnable()
	{
		SpriteAtlasManager.atlasRequested += OnAtlasRequested;
	}

	private void OnDisable()
	{
		SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
	}

	private void OnAtlasRequested(string tag, Action<SpriteAtlas> action)
	{
		if (m_hasErrored)
		{
			Debug.LogWarning("Error loading sprite atlas " + tag + " - ignoring future errors");
			m_hasErrored = true;
		}
	}
}
