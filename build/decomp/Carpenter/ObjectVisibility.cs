using UnityEngine;

public class ObjectVisibility : MonoBehaviour
{
	[SerializeField]
	private Transform m_positionTransform;

	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private CharacterStance m_stance;

	[SerializeField]
	private Vector2 m_positionOffset;

	private bool m_isSceneLit;

	private IObjectVisibilityListener[] m_listeners;

	private bool m_isVisible;

	public bool IsVisible => m_isVisible;

	public Vector2 Position
	{
		get
		{
			Vector2 vector = ((m_stance != null) ? ((Vector2)m_stance.GetActiveColliderBounds().center) : ((!(m_collider != null)) ? ((Vector2)m_positionTransform.position) : ((Vector2)m_collider.bounds.center)));
			return vector + m_positionOffset;
		}
	}

	public Collider2D Collider => m_collider;

	public Bounds Bounds
	{
		get
		{
			if (m_stance != null)
			{
				return m_stance.GetActiveColliderBounds();
			}
			if (m_collider != null)
			{
				return m_collider.bounds;
			}
			return new Bounds(base.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
		}
	}

	private void Reset()
	{
		GetComponentReferences();
	}

	public void GetComponentReferences()
	{
		if (m_positionTransform == null)
		{
			m_positionTransform = GetComponent<Transform>();
		}
		if (m_collider == null)
		{
			m_collider = GetComponentInChildren<Collider2D>();
		}
		if (m_stance == null)
		{
			m_stance = GetComponent<CharacterStance>();
		}
	}

	private void Start()
	{
		UpdateSceneLitStatus();
		m_isVisible = m_isSceneLit;
		m_listeners = GetComponentsInChildren<IObjectVisibilityListener>();
		OnVisibilityStateChanged();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.FieldOfView.ObjectVisibilitySet.Add(this);
		GlobalReferences.Instance.EventChannels.Generic.TimeOfDaySceneLightingChanged.Register(OnTimeOfDaySceneLightingChanged);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.FieldOfView.ObjectVisibilitySet.Remove(this);
		GlobalReferences.Instance.EventChannels.Generic.TimeOfDaySceneLightingChanged.Unregister(OnTimeOfDaySceneLightingChanged);
	}

	private void UpdateSceneLitStatus()
	{
		m_isSceneLit = true;
		TimeOfDaySceneLighting activeSceneLighting = TimeOfDaySceneLighting.ActiveSceneLighting;
		if (activeSceneLighting != null && activeSceneLighting.ObjectVisibilitySettings.IsDark)
		{
			m_isSceneLit = false;
		}
	}

	private void OnTimeOfDaySceneLightingChanged()
	{
		UpdateSceneLitStatus();
		if (m_isSceneLit)
		{
			SetVisible(visible: true);
		}
	}

	private void Update()
	{
		if (!m_isSceneLit)
		{
			bool flag = GlobalReferences.Instance.Variables.Generic.PlayerFlashlightActive.Value;
			if (!flag)
			{
				flag = GlobalReferences.Instance.Sets.Generic.VisibilityLightSourcesSet.IsLit(Bounds);
			}
			SetVisible(flag);
		}
	}

	private void OnVisibilityStateChanged()
	{
		bool isVisible = IsVisible;
		if (m_listeners != null)
		{
			IObjectVisibilityListener[] listeners = m_listeners;
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].SetObjectVisibility(isVisible);
			}
		}
		UpdateSpriteMaterialState();
	}

	private void UpdateSpriteMaterialState()
	{
		if (!(m_spriteRenderer == null) && !(m_spriteRenderer.material == null))
		{
			m_spriteRenderer.material.SetFloat("_OutlineThickness", m_isVisible ? 4f : 0f);
		}
	}

	private void SetVisible(bool visible)
	{
		if (m_isVisible != visible)
		{
			m_isVisible = visible;
			OnVisibilityStateChanged();
		}
	}

	public static void AddToGameObjectIfMissing(GameObject gameObject)
	{
		if (!(gameObject.GetComponent<ObjectVisibility>() != null) && !(gameObject.GetComponentInParent<MinigameScene>() != null))
		{
			gameObject.AddComponent<ObjectVisibility>().GetComponentReferences();
		}
	}
}
