using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalHitReactEffect : MonoBehaviour, IDynamicallySpawnedEventHandler
{
	[SerializeField]
	private float m_fadeTime = 1f;

	[SerializeField]
	private float m_progressionTime;

	[Header("Reload Scene")]
	[SerializeField]
	private float m_reloadSceneAlpha = -1f;

	public void OnInitialSpawn()
	{
		DecalProjector decalProjector = GetComponent<DecalProjector>();
		float fadeFactor = decalProjector.fadeFactor;
		decalProjector.fadeFactor = 0f;
		DOTween.To(() => decalProjector.fadeFactor, delegate(float x)
		{
			decalProjector.fadeFactor = x;
		}, fadeFactor, m_fadeTime);
		if (m_progressionTime > 0f)
		{
			StartCoroutine(ProgressionTime(decalProjector));
		}
	}

	public void OnReloadSpawn()
	{
		DecalProjector component = GetComponent<DecalProjector>();
		if (m_reloadSceneAlpha > 0f)
		{
			component.fadeFactor = m_reloadSceneAlpha;
		}
	}

	private IEnumerator ProgressionTime(DecalProjector decalProjector)
	{
		Material originalMaterial = decalProjector.material;
		decalProjector.material = new Material(decalProjector.material);
		decalProjector.material.SetFloat("_Progression", 0f);
		yield return decalProjector.material.DOFloat(1f, "_Progression", m_progressionTime).WaitForCompletion();
		Object.DestroyImmediate(decalProjector.material);
		decalProjector.material = originalMaterial;
	}
}
