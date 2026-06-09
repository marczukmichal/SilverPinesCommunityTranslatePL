using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class GameUtils
{
	public static class Constants
	{
		public static readonly float s_inputMoveDeadzoneMinValue = 0.6f;

		public static readonly int s_numProfiles = 3;

		public static readonly int s_saveGameSlots = 4;

		public static readonly int s_quickSaveSlot = 9;

		public static readonly int s_saveGameThumbnailWidth = 450;

		public static readonly int s_saveGameThumbnailHeight = 225;

		public static readonly int s_maxPhotos = 30;

		public static readonly int s_maxMarkers = 50;

		public static readonly float s_lowBatteryPoint = 10f;

		public static readonly int s_batteryPipCount = 6;
	}

	[Serializable]
	public struct RemapPair
	{
		public float m_worldReferenceValue;

		public float m_mapReferenceValue;
	}

	public static bool IsPlayer(GameObject gameObject)
	{
		CharacterIdentifier componentInParent = gameObject.GetComponentInParent<CharacterIdentifier>();
		if (componentInParent != null)
		{
			return componentInParent.gameObject.CompareTag(Tags.Player);
		}
		return false;
	}

	public static bool IsOnTraversalLayer(GameObject gameObject)
	{
		return gameObject.layer == GameLayers.TraversalLayer;
	}

	public static bool IsCurrentLevelDark()
	{
		TimeOfDaySceneLighting activeSceneLighting = TimeOfDaySceneLighting.ActiveSceneLighting;
		if (activeSceneLighting != null)
		{
			return activeSceneLighting.ObjectVisibilitySettings.IsDark;
		}
		return false;
	}

	public static Vector3 GetWorldPosFromScreen(Vector3 screenPosition, float zPos)
	{
		if (Camera.main == null)
		{
			Debug.LogWarning("Tried to get the world position from screen when there is no active main camera!");
			return screenPosition;
		}
		if (Camera.main.orthographic)
		{
			return Camera.main.ScreenToWorldPoint(screenPosition);
		}
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		float distance = zPos - Camera.main.transform.position.z;
		return ray.GetPoint(distance);
	}

	public static Vector2 CanvasChildTransformToScreenSpace(RectTransform transform, Canvas canvas)
	{
		RectTransform component = canvas.GetComponent<RectTransform>();
		Vector2 result = canvas.transform.InverseTransformPoint(transform.position);
		result.x += component.rect.width * 0.5f;
		result.y += component.rect.height * 0.5f;
		result.x *= component.localScale.x;
		result.y *= component.localScale.y;
		return result;
	}

	public static float RemapClamped(float value, float from1, float to1, float from2, float to2)
	{
		float value2 = (value - from1) / (to1 - from1);
		value2 = Mathf.Clamp01(value2);
		return from2 + (to2 - from2) * value2;
	}

	public static float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	public static float MapPositionRemap(float normalizedInput, RemapPair[] remapPairs)
	{
		normalizedInput = Mathf.Clamp01(normalizedInput);
		RemapPair remapPair = remapPairs[0];
		RemapPair remapPair2 = remapPairs[^1];
		for (int i = 0; i < remapPairs.Length; i++)
		{
			RemapPair remapPair3 = remapPairs[i];
			if (remapPair3.m_worldReferenceValue >= normalizedInput)
			{
				remapPair2 = remapPair3;
				break;
			}
			remapPair = remapPair3;
		}
		float t = Mathf.InverseLerp(remapPair.m_worldReferenceValue, remapPair2.m_worldReferenceValue, normalizedInput);
		return Mathf.Lerp(remapPair.m_mapReferenceValue, remapPair2.m_mapReferenceValue, t);
	}

	public static float InverseLerp(Vector3 start, Vector3 end, Vector3 value)
	{
		Vector3 vector = end - start;
		return Mathf.Clamp01(Vector3.Dot(value - start, vector) / Vector3.Dot(vector, vector));
	}

	public static bool ShouldIgnoreRaycastHitDueToPlatformEffector(RaycastHit2D hit)
	{
		PlatformEffector2D component = hit.collider.GetComponent<PlatformEffector2D>();
		if (component != null)
		{
			if (hit.distance <= 0f)
			{
				return true;
			}
			Vector2 up = Vector2.up;
			up = component.transform.localRotation * up;
			up = up.Rotate(component.rotationalOffset);
			float num = component.surfaceArc / 2f;
			if (Vector2.Angle(hit.normal, up) >= num)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool BoxCastAgainstEnvironment(Vector2 position, Vector2 forwardDir, Vector2 boxSize, int layerMask, float checkDistance = 1f)
	{
		float angle = Vector2.SignedAngle(Vector2.right, forwardDir);
		RaycastHit2D[] array = Physics2D.BoxCastAll(position, boxSize, angle, forwardDir, checkDistance, layerMask);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!(raycastHit2D.collider.GetComponent<PlatformEffector2D>() != null) && Vector2.Dot(raycastHit2D.normal, forwardDir) < -0.5f)
			{
				return true;
			}
		}
		return false;
	}

	public static uint NameToRenderingLayerMask(string name)
	{
		string[] definedRenderingLayerNames = RenderingLayerMask.GetDefinedRenderingLayerNames();
		for (int i = 0; i < definedRenderingLayerNames.Length; i++)
		{
			if (definedRenderingLayerNames[i].Equals(name))
			{
				return (uint)(1 << i);
			}
		}
		return 0u;
	}

	public static IEnumerator TempBlockInputActionIfPressed(InputAction action)
	{
		if (action.IsPressed())
		{
			action.Disable();
			yield return new WaitUntil(() => !action.IsPressed());
		}
		action.Enable();
	}

	public static bool IsHoveringSelectable(Vector2 currentPosition, GraphicRaycaster raycaster)
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = currentPosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		raycaster.Raycast(eventData, list);
		foreach (RaycastResult item in list)
		{
			GameObject gameObject = item.gameObject;
			Selectable componentInParent = gameObject.GetComponentInParent<Selectable>();
			if (componentInParent != null && componentInParent.enabled && componentInParent.interactable)
			{
				Interactable component = componentInParent.GetComponent<Interactable>();
				if (component != null && !component.CanInteract(null))
				{
					return false;
				}
				return true;
			}
			BaseMinigameHoldInteract componentInParent2 = gameObject.GetComponentInParent<BaseMinigameHoldInteract>();
			if (componentInParent2 != null && componentInParent2.enabled)
			{
				return true;
			}
		}
		return false;
	}

	public static List<MonoBehaviour> GetMinigameSelectables(GameObject gameObject)
	{
		GameObject gameObject2 = gameObject;
		List<MonoBehaviour> list = new List<MonoBehaviour>();
		if (gameObject2 != null)
		{
			MinigameScene componentInParent = gameObject2.GetComponentInParent<MinigameScene>();
			if (componentInParent != null)
			{
				gameObject2 = componentInParent.gameObject;
			}
			InventoryInteractionPanel componentInParent2 = gameObject2.GetComponentInParent<InventoryInteractionPanel>();
			if (componentInParent2 != null)
			{
				gameObject2 = componentInParent2.gameObject;
			}
			Selectable[] componentsInChildren = gameObject2.GetComponentsInChildren<Selectable>(includeInactive: true);
			BaseMinigameHoldInteract[] componentsInChildren2 = gameObject2.GetComponentsInChildren<BaseMinigameHoldInteract>(includeInactive: true);
			foreach (BaseMinigameHoldInteract item in componentsInChildren2)
			{
				list.Add(item);
			}
			Selectable[] array = componentsInChildren;
			foreach (Selectable item2 in array)
			{
				list.Add(item2);
			}
		}
		return list;
	}
}
