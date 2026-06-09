using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterDirection : MonoBehaviour, IPersistentComponent
{
	public enum Facing
	{
		None,
		Right,
		Left
	}

	[Serializable]
	private class PersistentData
	{
		public Facing m_direction;
	}

	[SerializeField]
	private Transform[] m_flipTranforms;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private bool m_setFlippedShaderValue;

	[SerializeField]
	private string m_flippedShaderPropertyName = "_Flipped";

	private Rigidbody2D m_rigidbody;

	private int m_shaderPropertyId;

	[ShowInDesignerInspector]
	[SerializeField]
	private Facing m_startingDirection;

	private Facing m_currentDirection = Facing.Right;

	private Facing m_desiredDirection = Facing.Right;

	private bool m_isTurning;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public SpriteRenderer SpriteRenderer => m_spriteRenderer;

	public Facing CurrentDirection
	{
		get
		{
			return m_currentDirection;
		}
		set
		{
			if (m_currentDirection == value)
			{
				return;
			}
			m_desiredDirection = (m_currentDirection = value);
			if (m_persistentData != null)
			{
				m_persistentData.m_direction = value;
			}
			switch (m_currentDirection)
			{
			case Facing.Right:
			{
				m_spriteRenderer.flipX = false;
				for (int j = 0; j < m_flipTranforms.Length; j++)
				{
					m_flipTranforms[j].localScale = new Vector3(1f, 1f, 1f);
				}
				break;
			}
			case Facing.Left:
			{
				m_spriteRenderer.flipX = true;
				for (int i = 0; i < m_flipTranforms.Length; i++)
				{
					m_flipTranforms[i].localScale = new Vector3(-1f, 1f, 1f);
				}
				break;
			}
			}
			if (m_setFlippedShaderValue)
			{
				m_spriteRenderer.material.SetFloat(m_shaderPropertyId, (m_currentDirection == Facing.Left) ? 1 : 0);
			}
		}
	}

	public Facing DesiredDirection
	{
		get
		{
			return m_desiredDirection;
		}
		set
		{
			if (m_desiredDirection != value)
			{
				m_desiredDirection = value;
				if (!HasTurnAnimation)
				{
					CurrentDirection = value;
				}
			}
		}
	}

	public bool IsTurning
	{
		get
		{
			return m_isTurning;
		}
		set
		{
			m_isTurning = value;
		}
	}

	public bool HasTurnAnimation { get; set; }

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			CurrentDirection = m_persistentData.m_direction;
			DesiredDirection = CurrentDirection;
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			SetFromStartingDirection();
		}
	}

	public Vector3 GetForwardVector()
	{
		return GetForwardVector(m_currentDirection);
	}

	public Vector3 GetForwardVector(Facing direction)
	{
		return direction switch
		{
			Facing.Right => Vector3.right, 
			Facing.Left => Vector3.left, 
			_ => Vector3.zero, 
		};
	}

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody2D>();
		if (m_spriteRenderer == null)
		{
			m_spriteRenderer = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
		}
		m_shaderPropertyId = Shader.PropertyToID(m_flippedShaderPropertyName);
	}

	private void Start()
	{
		if (m_currentDirection == Facing.None)
		{
			SetFromStartingDirection();
		}
	}

	private void SetFromStartingDirection()
	{
		m_currentDirection = Facing.None;
		Facing facing = ((m_startingDirection == Facing.None) ? Facing.Right : m_startingDirection);
		Facing facing3 = (m_desiredDirection = (CurrentDirection = facing));
	}

	public void ApplyDirectionChange()
	{
		CurrentDirection = DesiredDirection;
	}

	public bool IsFacingPoint(Vector2 position)
	{
		float xForward = position.x - base.transform.position.x;
		return IsFacingDirection(xForward);
	}

	public bool IsFacingDirection(float xForward)
	{
		if (xForward < 0f)
		{
			return CurrentDirection == Facing.Left;
		}
		return CurrentDirection == Facing.Right;
	}

	public static Facing GetOppositeDirection(Facing facing)
	{
		return facing switch
		{
			Facing.Right => Facing.Left, 
			Facing.Left => Facing.Right, 
			_ => Facing.None, 
		};
	}
}
