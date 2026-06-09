using System.Collections.Generic;
using UnityEngine;

public class WheeledPushableObject : BasePushableObject
{
	[SerializeField]
	private Rigidbody2D m_rigidbody;

	[SerializeField]
	private WheelJoint2D[] m_wheels;

	[SerializeField]
	private float m_uprightTorque = 10f;

	[SerializeField]
	private float m_dampingTorque = 5f;

	[SerializeField]
	private float m_maxTorque = 100f;

	[SerializeField]
	private float m_maxRotationAngle = 45f;

	private List<WheeledPushableObject> m_heldPushables;

	public Rigidbody2D Rigidbody => m_rigidbody;

	protected override void Awake()
	{
		base.Awake();
		m_heldPushables = new List<WheeledPushableObject>();
	}

	public override bool CanBePushed()
	{
		if (!base.CanBePushed())
		{
			return false;
		}
		if (m_rigidbody.linearVelocity.magnitude < 1f)
		{
			return Mathf.Abs(m_rigidbody.rotation) < 25f;
		}
		return false;
	}

	private void Clamp()
	{
		float rotation = m_rigidbody.rotation;
		float num = Mathf.Clamp(rotation, 0f - m_maxRotationAngle, m_maxRotationAngle);
		if (rotation != num)
		{
			m_rigidbody.rotation = num;
			m_rigidbody.angularVelocity = 0f;
		}
	}

	private void FixedUpdate()
	{
		Clamp();
		float target = 0f;
		float num = Mathf.DeltaAngle(m_rigidbody.rotation, target);
		float num2 = m_uprightTorque * num;
		float num3 = (0f - m_dampingTorque) * m_rigidbody.angularVelocity;
		float value = num2 + num3;
		value = Mathf.Clamp(value, 0f - m_maxTorque, m_maxTorque);
		m_rigidbody.AddTorque(value);
		UpdateHeldPushables();
		foreach (WheeledPushableObject heldPushable in m_heldPushables)
		{
			Vector2 linearVelocity = heldPushable.Rigidbody.linearVelocity;
			float num4 = Rigidbody.linearVelocity.x * 0.8f;
			if (Mathf.Abs(linearVelocity.x) < Mathf.Abs(num4))
			{
				linearVelocity.x = num4;
			}
			heldPushable.Rigidbody.linearVelocity = linearVelocity;
		}
	}

	private void UpdateHeldPushables()
	{
		m_heldPushables.Clear();
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.layerMask = GameLayers.PushableMask;
		contactFilter.useNormalAngle = true;
		contactFilter.minNormalAngle = -15f;
		contactFilter.maxNormalAngle = 15f;
		contactFilter.useTriggers = false;
		List<Collider2D> list = new List<Collider2D>();
		m_rigidbody.Overlap(contactFilter, list);
		foreach (Collider2D item in list)
		{
			WheeledPushableObject component = item.GetComponent<WheeledPushableObject>();
			if (component != null && component != this && component.transform.position.y > base.transform.position.y)
			{
				m_heldPushables.Add(component);
			}
		}
	}

	public void ClampVelocityToPusherDirection(CharacterDirection.Facing direction)
	{
		Vector2 linearVelocity = m_rigidbody.linearVelocity;
		switch (direction)
		{
		case CharacterDirection.Facing.Right:
			linearVelocity.x = Mathf.Max(linearVelocity.x, 0f);
			break;
		case CharacterDirection.Facing.Left:
			linearVelocity.x = Mathf.Min(linearVelocity.x, 0f);
			break;
		}
		m_rigidbody.linearVelocity = linearVelocity;
	}

	public override void SetMoveSpeed(float speed)
	{
		WheelJoint2D[] wheels = m_wheels;
		foreach (WheelJoint2D obj in wheels)
		{
			obj.useMotor = true;
			JointMotor2D motor = obj.motor;
			motor.motorSpeed = speed;
			obj.motor = motor;
		}
	}

	public override void DisableMovement()
	{
		WheelJoint2D[] wheels = m_wheels;
		for (int i = 0; i < wheels.Length; i++)
		{
			wheels[i].useMotor = false;
		}
	}

	public override float GetCurrentMoveSpeed()
	{
		return m_rigidbody.linearVelocity.x;
	}

	public override bool CheckForForcedExit(float maxAllowedVelocity)
	{
		if (!(Rigidbody.linearVelocity.y < -0.5f))
		{
			return Rigidbody.linearVelocity.magnitude > maxAllowedVelocity;
		}
		return true;
	}

	public override void TrySlowDown(float basePushSpeed)
	{
		SetMoveSpeed(basePushSpeed * GetCurrentMoveSpeed());
	}
}
