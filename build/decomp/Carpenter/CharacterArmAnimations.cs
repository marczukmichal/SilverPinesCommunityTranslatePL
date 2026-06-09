using PowerTools;
using UnityEngine;

public class CharacterArmAnimations : MonoBehaviour
{
	private SpriteAnim m_spriteAnim;

	public SpriteAnim SpriteAnim => m_spriteAnim;

	public float Speed
	{
		get
		{
			if (m_spriteAnim == null)
			{
				return 1f;
			}
			return m_spriteAnim.Speed;
		}
		set
		{
			if (m_spriteAnim != null)
			{
				m_spriteAnim.Speed = value;
			}
		}
	}

	private void Awake()
	{
		m_spriteAnim = GetComponent<SpriteAnim>();
	}

	public void Play(AnimationClip animation)
	{
		if (animation != null)
		{
			base.gameObject.SetActive(value: true);
			m_spriteAnim.Play(animation);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Pause()
	{
		if (base.gameObject.activeInHierarchy && m_spriteAnim != null)
		{
			m_spriteAnim.Pause();
		}
	}

	public void Resume()
	{
		if (base.gameObject.activeInHierarchy && m_spriteAnim != null)
		{
			m_spriteAnim.Resume();
		}
	}
}
