using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.SpriteRenderer)]
public class AllowDecalsOnSpriteRenderer : FsmStateAction
{
	public SpriteRenderer m_spriteRenderer;

	public bool m_enable = true;

	private static uint m_mask;

	private static bool m_hasMask;

	private static uint GetMask()
	{
		if (!m_hasMask)
		{
			m_mask = GameUtils.NameToRenderingLayerMask("Accepts Decals");
			m_hasMask = true;
		}
		return m_mask;
	}

	public override void OnEnter()
	{
		if (m_enable)
		{
			m_spriteRenderer.renderingLayerMask |= GetMask();
		}
		else
		{
			m_spriteRenderer.renderingLayerMask ^= GetMask();
		}
		Finish();
	}
}
