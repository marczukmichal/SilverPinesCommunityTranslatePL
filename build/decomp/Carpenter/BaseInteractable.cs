using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseInteractable : MonoBehaviour
{
	public enum LookDirection
	{
		None,
		LookAtOrigin,
		ForceLeft,
		ForceRight
	}

	[Serializable]
	public struct InteractAnimation
	{
		public InteractAnimationType m_playerAnimation;

		public UnityEvent m_startUseAnimationEvent;

		public UnityEvent m_stopUseAnimationEvent;

		public LookDirection m_lookDirection;

		[SerializeField]
		public bool m_useWalkToPosition;

		[SerializeField]
		public Vector2 m_walkToPositionOffset;

		public bool UseWalkToPosition => m_useWalkToPosition;
	}

	[Serializable]
	public struct NearbyHighlightSettings
	{
		[SerializeField]
		public UnityEvent m_highlightShowEvent;

		[SerializeField]
		public UnityEvent m_highlightHideEvent;

		[SerializeField]
		public GameObject m_highlightEnableObject;
	}

	[SerializeField]
	protected InteractAnimation m_animationSettings;

	[SerializeField]
	protected NearbyHighlightSettings m_highlightSettings;

	[SerializeField]
	protected bool m_autoInteract;

	private bool m_autoInteractOnce;

	[Header("Interact Prompt")]
	[SerializeField]
	protected Vector3 m_promptPositionOffset;

	[SerializeField]
	protected Transform m_promptPositionTransform;

	[SerializeField]
	private bool m_perspectiveAdjustedInteract = true;

	private Collider2D m_interactCollider;

	private Vector2 m_colliderOffsetStarting;

	public InteractAnimation AnimationSettings => m_animationSettings;

	public bool UseInteractPosition => AnimationSettings.m_useWalkToPosition;

	public bool AutoInteract => m_autoInteract;

	public Vector3 ColliderCenterPosition
	{
		get
		{
			if (m_interactCollider != null)
			{
				return m_interactCollider.bounds.center;
			}
			return base.transform.position;
		}
	}

	public Vector3 InteractPromptPosition
	{
		get
		{
			if (m_promptPositionTransform != null)
			{
				return m_promptPositionTransform.position;
			}
			Vector3 vector = base.transform.TransformVector(m_promptPositionOffset);
			return base.transform.position + vector;
		}
	}

	public abstract string InteractString { get; }

	public virtual int InteractPriority => 0;

	public virtual bool UseFSMInteractFlow => true;

	public virtual InteractButtonType GetInteractButonType()
	{
		return InteractButtonType.Normal;
	}

	public void SetAutoInteractOnce(bool enabled)
	{
		m_autoInteractOnce = enabled;
	}

	public Vector3 GetWalkToInteractPosition(bool ignorePerpsectiveAdjustment = false)
	{
		bool num = m_perspectiveAdjustedInteract && !ignorePerpsectiveAdjustment;
		Vector3 vector2;
		if (AnimationSettings.m_useWalkToPosition)
		{
			Vector3 vector = base.transform.TransformVector(AnimationSettings.m_walkToPositionOffset);
			vector.z = 0f;
			vector2 = base.transform.position + vector;
		}
		else
		{
			vector2 = base.transform.position;
		}
		if (!num)
		{
			return vector2;
		}
		return AdjustPositionForCameraPerspective(vector2);
	}

	public static Vector3 AdjustPositionForCameraPerspective(Vector3 position)
	{
		Vector3 result = position;
		float z = 0f;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			z = item.transform.position.z;
		}
		Camera main = Camera.main;
		if (main != null)
		{
			Vector3 pos = main.WorldToScreenPoint(position);
			Ray ray = main.ScreenPointToRay(pos);
			if (new Plane(Vector3.forward, new Vector3(0f, 0f, z)).Raycast(ray, out var enter))
			{
				result.x = ray.GetPoint(enter).x;
			}
		}
		return result;
	}

	public bool ShouldAutoInteract(BaseInteractor interactor)
	{
		if (CanInteract(interactor))
		{
			if (!m_autoInteract)
			{
				return m_autoInteractOnce;
			}
			return true;
		}
		return false;
	}

	public abstract void Interact(BaseInteractor interactor);

	protected virtual void Start()
	{
		m_interactCollider = GetComponent<Collider2D>();
		if (m_interactCollider != null)
		{
			m_colliderOffsetStarting = m_interactCollider.offset;
		}
		if (m_highlightSettings.m_highlightEnableObject != null)
		{
			m_highlightSettings.m_highlightEnableObject.SetActive(value: false);
		}
	}

	public virtual InteractType GetInteractType()
	{
		return InteractType.Basic;
	}

	public virtual InteractPromptIconType GetInteractPromptIconType()
	{
		return InteractPromptIconType.None;
	}

	public virtual bool ShouldShowInteractPrompt()
	{
		return true;
	}

	public virtual bool CanInteract(BaseInteractor interactor)
	{
		return base.enabled;
	}

	public void ForcePlayerInteract()
	{
		CharacterInteractor characterInteractor = UnityEngine.Object.FindAnyObjectByType<CharacterInteractor>();
		if (characterInteractor != null)
		{
			characterInteractor.DoForceInteract(this);
		}
	}

	public virtual void FinishedInteract()
	{
	}

	public virtual void HighlightInteractable(bool highlighted)
	{
		if (highlighted)
		{
			m_highlightSettings.m_highlightShowEvent?.Invoke();
		}
		else
		{
			m_highlightSettings.m_highlightHideEvent?.Invoke();
		}
		if (m_highlightSettings.m_highlightEnableObject != null)
		{
			m_highlightSettings.m_highlightEnableObject.SetActive(highlighted);
		}
	}

	protected bool ShowLevelTransitionName(LevelTransition levelTransition)
	{
		bool flag = true;
		bool flag2 = false;
		if (levelTransition.TargetLevel != null)
		{
			BaseMapAreaMetadata primaryMapAreaMetadata = levelTransition.TargetLevel.GetPrimaryMapAreaMetadata();
			if (primaryMapAreaMetadata != null)
			{
				flag = GlobalReferences.Instance.MapDynamicData.HasEnteredArea(primaryMapAreaMetadata);
				flag2 = primaryMapAreaMetadata.LabelString != null && !primaryMapAreaMetadata.LabelString.IsEmpty;
			}
		}
		return flag && flag2;
	}

	protected string GetLevelTransitionStringArgument(LevelTransition levelTransition)
	{
		bool flag = true;
		if (levelTransition.TargetLevel != null)
		{
			BaseMapAreaMetadata primaryMapAreaMetadata = levelTransition.TargetLevel.GetPrimaryMapAreaMetadata();
			if (primaryMapAreaMetadata != null)
			{
				flag = GlobalReferences.Instance.MapDynamicData.HasEnteredArea(primaryMapAreaMetadata);
			}
		}
		if (flag)
		{
			return levelTransition.TargetLevel.DisplayName;
		}
		return "?";
	}

	private void LateUpdate()
	{
		if (m_perspectiveAdjustedInteract && m_interactCollider != null)
		{
			AdjustColliderForPerpsective();
		}
	}

	private void AdjustColliderForPerpsective()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = AdjustPositionForCameraPerspective(position);
		Vector2 colliderOffsetStarting = m_colliderOffsetStarting;
		colliderOffsetStarting.x += vector.x - position.x;
		colliderOffsetStarting.x /= base.transform.lossyScale.x;
		m_interactCollider.offset = colliderOffsetStarting;
	}
}
