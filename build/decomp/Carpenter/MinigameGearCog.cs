using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CircleCollider2D))]
public class MinigameGearCog : MonoBehaviour
{
	public enum RotationDirection
	{
		Clockwise,
		AntiClockwise
	}

	public enum AllowedRotation
	{
		Any,
		ClowckwiseOnly,
		AntiClockwiseOnly
	}

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onPoweredEvent;

	[SerializeField]
	private UnityEvent m_onNotPoweredEvent;

	[Header("Connection Graphic")]
	[SerializeField]
	private Graphic m_connectionGraphic;

	[SerializeField]
	private Color m_connectionGraphicOnColor = Color.green;

	[SerializeField]
	private Color m_connectionGraphicOffColor = Color.gray;

	private CircleCollider2D m_collider;

	private bool m_powered;

	private Collider2D[] m_results = new Collider2D[16];

	private List<MinigameGearCog> m_cogResults = new List<MinigameGearCog>();

	[Header("Cog Movement")]
	[SerializeField]
	private float m_poweredRotationSpeed;

	[SerializeField]
	private RotationDirection m_rotationDirection;

	[SerializeField]
	private AllowedRotation m_allowedRotationDirection;

	public bool IsPowered => m_powered;

	public RotationDirection Direction => m_rotationDirection;

	public bool CanTurnInDirection(RotationDirection direction)
	{
		if (m_allowedRotationDirection == AllowedRotation.ClowckwiseOnly)
		{
			return direction == RotationDirection.Clockwise;
		}
		if (m_allowedRotationDirection == AllowedRotation.AntiClockwiseOnly)
		{
			return direction == RotationDirection.AntiClockwise;
		}
		return true;
	}

	private void Awake()
	{
		m_collider = GetComponent<CircleCollider2D>();
	}

	public void SetPowered(bool powered)
	{
		m_powered = powered;
		if (powered)
		{
			DOTween.Kill(base.transform);
			m_onPoweredEvent?.Invoke();
		}
		else
		{
			m_onNotPoweredEvent?.Invoke();
		}
		if (m_connectionGraphic != null)
		{
			m_connectionGraphic.color = (powered ? m_connectionGraphicOnColor : m_connectionGraphicOffColor);
		}
	}

	public void SetRotationDirection(RotationDirection direction)
	{
		m_rotationDirection = direction;
	}

	private void Update()
	{
		if (m_powered)
		{
			float num = ((m_rotationDirection == RotationDirection.AntiClockwise) ? m_poweredRotationSpeed : (0f - m_poweredRotationSpeed));
			num *= Time.unscaledDeltaTime;
			base.transform.Rotate(0f, 0f, num);
		}
	}

	public List<MinigameGearCog> GetOverlappingCogs()
	{
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.layerMask = GameLayers.MinigameMask;
		contactFilter.useLayerMask = true;
		int num = m_collider.Overlap(contactFilter, m_results);
		if (num > m_results.Length)
		{
			Debug.LogError("Array too small for GetOverlappingCogs");
		}
		m_cogResults.Clear();
		for (int i = 0; i < num; i++)
		{
			if ((bool)m_results[i].GetComponent<MinigameGearCog>())
			{
				m_cogResults.Add(m_results[i].GetComponent<MinigameGearCog>());
			}
		}
		return m_cogResults;
	}

	public void PlayFailTurnAnimation(RotationDirection direction)
	{
		DOTween.Kill(base.transform);
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		float num = ((direction == RotationDirection.AntiClockwise) ? 1f : (-1f));
		num *= 10f;
		eulerAngles.z += num;
		base.transform.DOLocalRotate(eulerAngles, 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBounce)
			.SetUpdate(isIndependentUpdate: true);
	}
}
