using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraversalPrompt : MonoBehaviour
{
	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[SerializeField]
	private GameObject m_toggleContainer;

	[SerializeField]
	private RectTransform m_positionTransform;

	[SerializeField]
	private RectTransform m_parentRect;

	[SerializeField]
	private Canvas m_parentCanvas;

	[SerializeField]
	private Image m_traversalImage;

	[Header("Traversal Sprite")]
	[SerializeField]
	private Sprite m_stairsUpIcon;

	[SerializeField]
	private Sprite m_stairsDownIcon;

	[SerializeField]
	private Sprite m_stairsBothIcon;

	[SerializeField]
	private Sprite m_ladderUpIcon;

	[SerializeField]
	private Sprite m_ladderDownIcon;

	[SerializeField]
	private Sprite m_ropeUpIcon;

	[SerializeField]
	private Sprite m_ropeDownIcon;

	[SerializeField]
	private Sprite m_leapIcon;

	private void Start()
	{
		m_toggleContainer.SetActive(value: false);
	}

	private bool IsPlayerInValidState()
	{
		if (m_playerAnchor != null && m_playerAnchor.Item != null)
		{
			if (m_playerAnchor.Item.GetComponent<CharacterInputPlayer>() != null)
			{
				return true;
			}
			CharacterInputCompanion component = m_playerAnchor.Item.GetComponent<CharacterInputCompanion>();
			if (component != null)
			{
				return component.IsPlayerControlled();
			}
		}
		return false;
	}

	private void PositionOnStairs(List<Stairs> overlappingStairs, Transform playerTransform)
	{
		Vector3 zero = Vector3.zero;
		Vector3 position = playerTransform.position;
		foreach (Stairs overlappingStair in overlappingStairs)
		{
			Vector2 vector = ((!(Vector2.Distance(position, overlappingStair.TopStairsWorldPosition) < Vector2.Distance(position, overlappingStair.BottomStairsWorldPosition))) ? ((Vector2)overlappingStair.BottomStairsWorldPosition) : ((Vector2)overlappingStair.TopStairsWorldPosition));
			Vector3 vector2 = vector;
			vector2.z = overlappingStair.transform.position.z;
			zero += vector2;
		}
		if (overlappingStairs.Count > 0)
		{
			zero /= (float)overlappingStairs.Count;
		}
		zero.z = position.z;
		PositionAtWorldPosition(zero);
	}

	private void PositionOnLadder(Transform playerTransform, Ladder ladder, Ladder.LadderState ladderState)
	{
		Bounds ladderBounds = ladder.LadderBounds;
		Vector3 position = playerTransform.position;
		position = ladderBounds.ClosestPoint(position);
		position.x = ladder.transform.position.x;
		position.z = ladder.transform.position.z;
		if (ladderState.m_isNearBottom)
		{
			position.y += 2f;
		}
		PositionAtWorldPosition(position);
	}

	private void PositionOnRope(Transform playerTransform, ClimbableRope climbableRope, ClimbableRope.RopeState ropeState)
	{
		Bounds ropeBounds = climbableRope.RopeBounds;
		Vector3 position = playerTransform.position;
		position = ropeBounds.ClosestPoint(position);
		position.x = climbableRope.transform.position.x;
		position.z = climbableRope.transform.position.z;
		if (ropeState.m_isNearBottom)
		{
			position.y += 2f;
		}
		PositionAtWorldPosition(position);
	}

	private void PositionAtWorldPosition(Vector3 worldPosition)
	{
		if (!(Camera.main == null))
		{
			Vector3 vector = Camera.main.WorldToScreenPoint(worldPosition);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRect, vector, m_parentCanvas.worldCamera, out var localPoint);
			m_positionTransform.anchoredPosition = localPoint;
		}
	}

	private void Update()
	{
		bool active = false;
		if (IsPlayerInValidState())
		{
			CharacterTraversalUtils component = m_playerAnchor.Item.GetComponent<CharacterTraversalUtils>();
			CharacterMovement component2 = m_playerAnchor.Item.GetComponent<CharacterMovement>();
			if (component != null)
			{
				CharacterStance component3 = m_playerAnchor.Item.GetComponent<CharacterStance>();
				LadderCheck componentInChildren = m_playerAnchor.Item.GetComponentInChildren<LadderCheck>();
				RopeCheck componentInChildren2 = m_playerAnchor.Item.GetComponentInChildren<RopeCheck>();
				if (component2.CanUseOptionalStairs())
				{
					active = true;
					Sprite sprite = null;
					switch (component2.StairsDirection)
					{
					case CharacterMovement.AvailableStairsDirection.TravelDown:
						sprite = m_stairsDownIcon;
						break;
					case CharacterMovement.AvailableStairsDirection.TravelUp:
						sprite = m_stairsUpIcon;
						break;
					case CharacterMovement.AvailableStairsDirection.Both:
						sprite = m_stairsBothIcon;
						break;
					}
					m_traversalImage.sprite = sprite;
					PositionOnStairs(component2.OverlappingStairs, m_playerAnchor.Item.transform);
				}
				else if (componentInChildren != null && componentInChildren.IsChecking && (bool)componentInChildren.DetectedLadder)
				{
					Ladder detectedLadder = componentInChildren.DetectedLadder;
					if (detectedLadder != null)
					{
						Ladder.LadderState ladderState = detectedLadder.GetLadderState(m_playerAnchor.Item.transform.position);
						active = true;
						m_traversalImage.sprite = (ladderState.m_isAtTop ? m_ladderDownIcon : m_ladderUpIcon);
						PositionOnLadder(m_playerAnchor.Item.transform, componentInChildren.DetectedLadder, ladderState);
					}
				}
				else if (componentInChildren2 != null && componentInChildren2.IsChecking && (bool)componentInChildren2.DetectedRope)
				{
					ClimbableRope detectedRope = componentInChildren2.DetectedRope;
					if (detectedRope != null)
					{
						ClimbableRope.RopeState ropeState = detectedRope.GetRopeState(component3);
						active = true;
						m_traversalImage.sprite = (ropeState.m_isAtTop ? m_ropeDownIcon : m_ropeUpIcon);
						PositionOnRope(m_playerAnchor.Item.transform, componentInChildren2.DetectedRope, ropeState);
					}
				}
			}
			CharacterTargetedLeap component4 = m_playerAnchor.Item.GetComponent<CharacterTargetedLeap>();
			if (component4 != null && component4.AvailableLeapType != 0 && component4.IsChecking)
			{
				active = true;
				m_traversalImage.sprite = m_leapIcon;
			}
		}
		m_toggleContainer.SetActive(active);
	}
}
