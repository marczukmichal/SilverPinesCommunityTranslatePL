using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Detectable : MonoBehaviour
{
	public enum TargetArea
	{
		TransformRoot,
		Chest,
		Head,
		Legs
	}

	public enum Visibility
	{
		Full,
		Reduced,
		Hidden
	}

	[SerializeField]
	private List<Transform> m_detectableTransforms;

	[Header("Stance Scaling")]
	[SerializeField]
	private Transform m_scaleTransform;

	[SerializeField]
	private Vector3 m_normalScale;

	[SerializeField]
	private Vector3 m_crouchedScale;

	[Header("Aim Nodes")]
	[SerializeField]
	private Transform m_headTransform;

	[SerializeField]
	private Transform m_chestTransform;

	[SerializeField]
	private Transform m_legsTransform;

	private Visibility m_currentVisibility;

	private bool m_isHiding;

	public List<Transform> DetectableTransforms => m_detectableTransforms;

	public Visibility CurrentVisibility => m_currentVisibility;

	public bool IsHiding
	{
		get
		{
			return m_isHiding;
		}
		set
		{
			if (m_isHiding != value)
			{
				m_isHiding = value;
				UpdateVisibility(GetComponent<CharacterStance>().CurrentStance);
			}
		}
	}

	public Vector3 GetAimPosition(TargetArea area)
	{
		Transform transform = null;
		switch (area)
		{
		case TargetArea.TransformRoot:
			transform = base.transform;
			break;
		case TargetArea.Chest:
			transform = m_chestTransform;
			break;
		case TargetArea.Head:
			transform = m_headTransform;
			break;
		case TargetArea.Legs:
			transform = m_legsTransform;
			break;
		}
		if (transform == null)
		{
			transform = base.transform;
		}
		if (transform != null)
		{
			return transform.position;
		}
		return base.transform.position;
	}

	private void OnEnable()
	{
		CharacterStance component = GetComponent<CharacterStance>();
		if (component != null)
		{
			component.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Combine(component.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(StanceChanged));
		}
	}

	private void OnDisable()
	{
		CharacterStance component = GetComponent<CharacterStance>();
		if (component != null)
		{
			component.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Remove(component.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(StanceChanged));
		}
	}

	private void StanceChanged(CharacterStance.Stance newStance)
	{
		UpdateVisibility(newStance);
	}

	private void UpdateVisibility(CharacterStance.Stance currentStance)
	{
		switch (currentStance)
		{
		case CharacterStance.Stance.Standing:
		case CharacterStance.Stance.NoCollision:
			m_scaleTransform.localScale = m_normalScale;
			m_currentVisibility = Visibility.Full;
			break;
		case CharacterStance.Stance.Crouching:
			m_scaleTransform.localScale = m_crouchedScale;
			m_currentVisibility = Visibility.Reduced;
			break;
		default:
			Debug.LogError("Unhandled stance change for stance " + currentStance);
			break;
		}
		if (m_isHiding)
		{
			m_currentVisibility = Visibility.Hidden;
		}
	}
}
