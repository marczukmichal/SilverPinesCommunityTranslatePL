using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Map3D : MonoBehaviour
{
	private enum MapZoomInput
	{
		None,
		ZoomIn,
		ZoomOut,
		WaitForReset
	}

	public struct PlayerPositionData
	{
		public Vector3 m_mapWorldPosition;

		public Quaternion m_baseRotation;

		public bool m_showPlayerPosition;

		public bool m_unmappedLocation;
	}

	[SerializeField]
	private Camera m_camera;

	[SerializeField]
	private Transform m_cameraLookTransform;

	[SerializeField]
	private float m_inputCameraMoveSpeed = 500f;

	[Header("Zoom")]
	[SerializeField]
	private float m_quickMapZoomLevel = 45f;

	[SerializeField]
	private float m_boatQuickMapZoomLevel = 45f;

	[SerializeField]
	private float[] m_zoomLevels;

	[SerializeField]
	private int m_defaultZoomLevelIndex;

	[SerializeField]
	private float m_moveToZoomSpeed = 1f;

	[Header("Player Position")]
	[SerializeField]
	private Transform m_playerPositionMarker;

	[SerializeField]
	private SpriteRenderer m_playerPositionSprite;

	[SerializeField]
	private Mesh m_playerFogOfWarRevealMesh;

	[SerializeField]
	private float m_playerFogOfWarRevealMeshScale = 16f;

	[Header("Container")]
	[SerializeField]
	private GameObject m_container;

	[Header("Frame Time")]
	[SerializeField]
	private float m_frameTimeDuration = 0.1f;

	[Header("World Bounds")]
	[SerializeField]
	private WorldMapBounds m_activeWorldMapBounds;

	[Header("Lines")]
	[SerializeField]
	private MapLinesManager m_mapLinesManager;

	[Header("Background Terrain Reference")]
	[SerializeField]
	private MeshRenderer m_backgroundTerrain;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_mapRevealAudioEvent;

	[Header("Floors")]
	[SerializeField]
	private List<Map3DFloor> m_floors;

	private int m_currentFloorIndex;

	[Header("Metadata")]
	[SerializeField]
	private MapViewMetadata m_metadata;

	[SerializeField]
	private bool m_useFogOfWar;

	[Header("Shared Settings")]
	[SerializeField]
	private MapVisualsSettings m_visualsSettings;

	private MapArea3D[] m_mapAreas3D;

	private MapAreaConnectionLine[] m_connectionLines;

	private WorldMapRevealerMesh[] m_mapRevealerMeshes;

	private MapRegionFocalPoint[] m_focalPoints;

	private MapDetailsLayer[] m_mapDetailsLayers;

	private Vector2 m_moveInput;

	private float m_targetOrthoSize;

	private int m_zoomLevelIndex;

	private float m_frameTimer;

	private bool m_mapEnabled;

	private MapType m_mapType;

	private Vector3 m_gamepadCursorWorldPosition;

	private bool m_isAnimating;

	[DebugCommand("always_render_map", "Force World Map to always render every frame", "always_render_map <true/false>", typeof(bool), false)]
	public static bool ALWAYS_RENDER_MAP = true;

	private BaseMapAreaMetadata m_activePlayerArea;

	private MapZoomInput m_zoomInput;

	private Coroutine m_animationCoroutine;

	public Camera Camera => m_camera;

	public List<Map3DFloor> Floors => m_floors;

	public bool HasFloors => m_floors.Count > 0;

	public Map3DFloor ActiveFloor
	{
		get
		{
			if (!HasFloors)
			{
				return null;
			}
			return m_floors[m_currentFloorIndex];
		}
	}

	public int ActiveFloorIndex
	{
		get
		{
			if (!HasFloors)
			{
				return -1;
			}
			return m_currentFloorIndex;
		}
	}

	public MapViewMetadata Metadata => m_metadata;

	public MapLinesManager MapLinesManager => m_mapLinesManager;

	public bool UseFogOfWar => m_useFogOfWar;

	public MapVisualsSettings.MapAreaVisuals VisualsSettings => m_visualsSettings.m_areaVisuals;

	public Vector3 CameraWorldPosition => m_camera.transform.position;

	public MapArea3D[] MapAreas => m_mapAreas3D;

	public BaseMapAreaMetadata ActivePlayerArea => m_activePlayerArea;

	public bool IsAnimating
	{
		get
		{
			return m_isAnimating;
		}
		set
		{
			m_isAnimating = value;
			GlobalReferences.Instance.EventChannels.Generic.SetGamepadCursorAllowed.Raise(!value);
		}
	}

	public int GetUniqueID()
	{
		if (!(m_metadata == null))
		{
			return m_metadata.UniqueID;
		}
		return -1;
	}

	public void SetActiveWorldMapBounds(WorldMapBounds bounds)
	{
		m_activeWorldMapBounds = bounds;
	}

	private float GetOrthoCameraSizeFromZoomLevel(int zoomLevelIndex)
	{
		return m_zoomLevels[zoomLevelIndex];
	}

	private void Awake()
	{
		m_camera.enabled = false;
		GetReferences();
		m_zoomLevelIndex = m_defaultZoomLevelIndex;
		float num = (m_targetOrthoSize = (m_camera.orthographicSize = GetOrthoCameraSizeFromZoomLevel(m_zoomLevelIndex)));
		m_container.gameObject.SetActive(value: false);
		if (m_floors != null && m_floors.Count > 0)
		{
			for (int i = 0; i < m_floors.Count; i++)
			{
				m_floors[i].gameObject.SetActive(i == m_currentFloorIndex);
			}
			if (m_floors[m_currentFloorIndex].MapBackgroundMaterial != null)
			{
				m_backgroundTerrain.sharedMaterial = m_floors[m_currentFloorIndex].MapBackgroundMaterial;
			}
		}
	}

	public void SetRenderTexture(RenderTexture renderTexture)
	{
		m_camera.targetTexture = renderTexture;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Set(this);
		MapDetailsLayer[] mapDetailsLayers = m_mapDetailsLayers;
		foreach (MapDetailsLayer obj in mapDetailsLayers)
		{
			obj.OnZoomLevelUpdate(m_camera.orthographicSize);
			obj.OnFloorChanged(ActiveFloor);
		}
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Set(null);
		UnregisterInputs();
	}

	private IEnumerator AnimateMapRevealPickup(MapRevealPickup mapReveal)
	{
		GlobalReferences.Instance.MapDynamicData.ClearAnimateMapRevealPickup();
		yield return new WaitForEndOfFrame();
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		foreach (MapArea3D mapArea3D in mapAreas3D)
		{
			if (mapReveal.IsKnown(mapArea3D.MapAreaMetadata) && !mapArea3D.IsKnown(ignoreMapRevealPickups: true))
			{
				mapArea3D.gameObject.SetActive(value: false);
			}
		}
		MapAreaConnectionLine[] connectionLines = m_connectionLines;
		foreach (MapAreaConnectionLine mapAreaConnectionLine in connectionLines)
		{
			if (mapAreaConnectionLine.IsKnownByRevealMapPickup(mapReveal))
			{
				mapAreaConnectionLine.gameObject.SetActive(value: false);
			}
		}
		yield return new WaitForSeconds(1f);
		if (m_mapRevealAudioEvent != null)
		{
			m_mapRevealAudioEvent.Play2D();
		}
		mapAreas3D = m_mapAreas3D;
		foreach (MapArea3D mapArea3D2 in mapAreas3D)
		{
			if (mapReveal.IsKnown(mapArea3D2.MapAreaMetadata) && !mapArea3D2.IsKnown(ignoreMapRevealPickups: true))
			{
				mapArea3D2.gameObject.SetActive(value: true);
			}
		}
		connectionLines = m_connectionLines;
		foreach (MapAreaConnectionLine mapAreaConnectionLine2 in connectionLines)
		{
			if (mapAreaConnectionLine2.IsKnownByRevealMapPickup(mapReveal))
			{
				mapAreaConnectionLine2.gameObject.SetActive(value: true);
			}
		}
	}

	private void Update()
	{
		if (!m_mapEnabled)
		{
			return;
		}
		float orthographicSize = m_camera.orthographicSize;
		float num = Mathf.Abs(m_targetOrthoSize - m_camera.orthographicSize);
		m_camera.orthographicSize = Mathf.MoveTowards(m_camera.orthographicSize, m_targetOrthoSize, m_moveToZoomSpeed * Time.unscaledDeltaTime * num);
		if (Mathf.Abs(num) > 0f)
		{
			MapDetailsLayer[] mapDetailsLayers = m_mapDetailsLayers;
			for (int i = 0; i < mapDetailsLayers.Length; i++)
			{
				mapDetailsLayers[i].OnZoomLevelUpdate(m_camera.orthographicSize);
			}
		}
		if (Mathf.Abs(orthographicSize - m_camera.orthographicSize) > float.Epsilon)
		{
			ClampCameraToBounds();
			SetCameraDirty();
		}
	}

	private void LateUpdate()
	{
		if (m_mapEnabled)
		{
			bool flag = false;
			m_frameTimer -= Time.unscaledDeltaTime;
			if (m_frameTimer <= 0f)
			{
				m_frameTimer = m_frameTimeDuration;
				flag = true;
			}
			if (m_isAnimating)
			{
				flag = true;
			}
			if (ALWAYS_RENDER_MAP)
			{
				flag = true;
			}
			if (flag)
			{
				UniversalRenderPipelineAsset obj = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
				float renderScale = obj.renderScale;
				obj.renderScale = 1f;
				m_camera.Render();
				obj.renderScale = renderScale;
				GlobalReferences.Instance.EventChannels.Map.OnMapCameraRendered.Raise();
			}
			if (!IsAnimating && GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
			{
				GamepadModeMoveCameraTowardsCursor();
			}
		}
	}

	public IMapRaycastTooltipElement GetRaycastTooltipElement()
	{
		IMapRaycastTooltipElement result = null;
		RaycastHit[] array;
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			Vector3 gamepadCursorWorldPosition = m_gamepadCursorWorldPosition;
			gamepadCursorWorldPosition.y += 10f;
			array = Physics.RaycastAll(gamepadCursorWorldPosition, Vector3.down, 100f, GameLayers.MapMask, QueryTriggerInteraction.Collide);
		}
		else
		{
			if (Mouse.current == null)
			{
				return null;
			}
			array = Physics.RaycastAll(m_camera.ScreenPointToRay(Mouse.current.position.value), 1000f, GameLayers.MapMask, QueryTriggerInteraction.Collide);
		}
		int num = -1;
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			IMapRaycastTooltipElement component = raycastHit.collider.GetComponent<IMapRaycastTooltipElement>();
			if (component.TooltipActive && component.TooltipPriority > num)
			{
				num = component.TooltipPriority;
				result = component;
			}
		}
		return result;
	}

	public void GamepadModeMoveCameraTowardsCursor()
	{
		Vector3 gamepadCursorWorldPosition = m_gamepadCursorWorldPosition;
		Vector3 position = m_cameraLookTransform.position;
		Vector3 vector = gamepadCursorWorldPosition - position;
		m_camera.transform.Translate(vector * Time.unscaledDeltaTime * 2.5f, Space.World);
		ClampCameraToBounds();
		SetCameraDirty();
	}

	private void RegisterInputs()
	{
		GameInputManager.GameInputActions.UI.MapScroll.performed += OnMapScrollInputAction;
		GameInputManager.GameInputActions.UI.MapZoom.performed += OnMapZoomInputAction;
	}

	private void UnregisterInputs()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.MapScroll.performed -= OnMapScrollInputAction;
			GameInputManager.GameInputActions.UI.MapZoom.performed -= OnMapZoomInputAction;
		}
		m_moveInput = Vector2.zero;
		m_zoomInput = MapZoomInput.None;
	}

	public void Populate3DMapAreaReferences()
	{
		if (ActiveFloor != null)
		{
			m_mapAreas3D = ActiveFloor.GetComponentsInChildren<MapArea3D>(includeInactive: true);
			m_connectionLines = ActiveFloor.GetComponentsInChildren<MapAreaConnectionLine>(includeInactive: true);
		}
		else
		{
			m_mapAreas3D = GetComponentsInChildren<MapArea3D>(includeInactive: true);
			m_connectionLines = GetComponentsInChildren<MapAreaConnectionLine>(includeInactive: true);
		}
	}

	public void Refresh()
	{
		RefreshPlayerArea();
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		for (int i = 0; i < mapAreas3D.Length; i++)
		{
			mapAreas3D[i].Refresh(m_activePlayerArea);
		}
		MapAreaConnectionLine[] connectionLines = m_connectionLines;
		for (int i = 0; i < connectionLines.Length; i++)
		{
			connectionLines[i].RefreshState();
		}
	}

	private void GetReferences()
	{
		Populate3DMapAreaReferences();
		if (m_focalPoints == null)
		{
			m_focalPoints = GetComponentsInChildren<MapRegionFocalPoint>();
		}
		if (m_mapRevealerMeshes == null)
		{
			m_mapRevealerMeshes = GetComponentsInChildren<WorldMapRevealerMesh>(includeInactive: true);
		}
		if (m_mapDetailsLayers == null)
		{
			m_mapDetailsLayers = GetComponentsInChildren<MapDetailsLayer>();
		}
	}

	private void SetCameraDirty()
	{
		m_frameTimer = 0f;
	}

	public void SetMapActive(bool enable)
	{
		m_mapEnabled = enable;
		m_container.SetActive(enable);
		if (enable)
		{
			RegisterInputs();
			GetReferences();
			Refresh();
			MapRevealPickup mapRevealPickupToAnimate = GlobalReferences.Instance.MapDynamicData.MapRevealPickupToAnimate;
			if (mapRevealPickupToAnimate != null)
			{
				StartCoroutine(AnimateMapRevealPickup(mapRevealPickupToAnimate));
			}
		}
		else
		{
			UnregisterInputs();
		}
		SetCameraDirty();
		GlobalReferences.Instance.EventChannels.Map.MapSceneLoaded.Raise();
	}

	private void ClampGamepadCursorToBounds()
	{
		if (m_activeWorldMapBounds != null)
		{
			Vector3 vector = (m_gamepadCursorWorldPosition = m_activeWorldMapBounds.WorldBounds.ClosestPoint(m_gamepadCursorWorldPosition));
		}
	}

	private void ClampCameraToBounds()
	{
		if (m_activeWorldMapBounds != null)
		{
			Vector3 position = m_cameraLookTransform.position;
			Bounds worldBounds = m_activeWorldMapBounds.WorldBounds;
			_ = worldBounds.min;
			_ = worldBounds.max;
			Vector3 size = worldBounds.size;
			size.x -= m_camera.orthographicSize * 2f;
			size.y -= m_camera.orthographicSize * 2f;
			size.z -= m_camera.orthographicSize * 2f;
			size.x = Mathf.Max(size.x, 0f);
			size.y = Mathf.Max(size.y, 0f);
			size.z = Mathf.Max(size.z, 0f);
			worldBounds.size = size;
			position.y = worldBounds.center.y;
			Vector3 translation = worldBounds.ClosestPoint(position) - m_cameraLookTransform.position;
			translation.y = 0f;
			m_camera.transform.Translate(translation, Space.World);
		}
	}

	private float GetMaxZoom()
	{
		if (m_activeWorldMapBounds != null)
		{
			return m_activeWorldMapBounds.MaxZoom;
		}
		return m_zoomLevels[m_zoomLevels.Length - 1];
	}

	public bool InputUpdate()
	{
		bool result = false;
		InputState.Mode inputMode = GlobalReferences.Instance.InputState.InputMode;
		if (m_mapEnabled)
		{
			if (m_moveInput.magnitude > 0f)
			{
				Vector3 vector = new Vector3(m_moveInput.x, 0f, m_moveInput.y);
				vector *= m_inputCameraMoveSpeed * m_targetOrthoSize;
				switch (inputMode)
				{
				case InputState.Mode.KeyboardMouse:
					m_camera.transform.Translate(vector * Time.unscaledDeltaTime, Space.World);
					ClampCameraToBounds();
					SetCameraDirty();
					m_gamepadCursorWorldPosition = GetWorldPositionFromScreenPosition(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
					break;
				case InputState.Mode.Gamepad:
					vector *= 0.25f;
					m_gamepadCursorWorldPosition += vector * Time.unscaledDeltaTime;
					ClampGamepadCursorToBounds();
					break;
				}
				result = true;
			}
			if (m_zoomInput == MapZoomInput.ZoomIn || m_zoomInput == MapZoomInput.ZoomOut)
			{
				int num = m_zoomLevelIndex;
				if (m_zoomInput == MapZoomInput.ZoomOut)
				{
					num++;
				}
				else if (m_zoomInput == MapZoomInput.ZoomIn)
				{
					num--;
				}
				num = (m_zoomLevelIndex = Mathf.Clamp(num, 0, m_zoomLevels.Length - 1));
				m_zoomInput = MapZoomInput.WaitForReset;
				if (inputMode == InputState.Mode.KeyboardMouse)
				{
					StartCoroutine(ResetZoomInputFlagDelay());
				}
				float targetOrthoSize = m_targetOrthoSize;
				m_targetOrthoSize = GetOrthoCameraSizeFromZoomLevel(num);
				if (Mathf.Abs(m_targetOrthoSize - targetOrthoSize) > 1f)
				{
					SetCameraDirty();
					result = true;
				}
			}
			if (inputMode == InputState.Mode.KeyboardMouse)
			{
				result = UpdateMouseDrag();
			}
		}
		return result;
	}

	public void GamepadCursorMoveTowardsScreenPosition(Vector3 screenPosition)
	{
		m_gamepadCursorWorldPosition = Vector3.MoveTowards(m_gamepadCursorWorldPosition, GetWorldPositionFromScreenPosition(screenPosition), Time.unscaledDeltaTime * 100f);
	}

	public void SetZoomLevel(float zoomLevel)
	{
		m_camera.orthographicSize = (m_targetOrthoSize = zoomLevel);
	}

	public void SetQuickMapZoomLevel()
	{
		SetZoomLevel(m_quickMapZoomLevel);
	}

	public void SetQuickMapBoatTravelZoom()
	{
		SetZoomLevel(m_boatQuickMapZoomLevel);
	}

	public void SetMapType(MapType mapType)
	{
		m_mapType = mapType;
		m_mapLinesManager.gameObject.SetActive(mapType == MapType.MainMap);
	}

	private Vector3 GetWorldPositionFromScreenPosition(Vector2 screenPosition)
	{
		Vector3 position = new Vector3(screenPosition.x, screenPosition.y, Vector3.Distance(m_camera.transform.position, m_cameraLookTransform.position));
		return m_camera.ScreenToWorldPoint(position);
	}

	private bool UpdateMouseDrag()
	{
		if (Mouse.current.leftButton.isPressed)
		{
			Vector2 value = Mouse.current.delta.value;
			if (value.magnitude > float.Epsilon)
			{
				Vector3 worldPositionFromScreenPosition = GetWorldPositionFromScreenPosition(Mouse.current.position.value);
				Vector3 worldPositionFromScreenPosition2 = GetWorldPositionFromScreenPosition(Mouse.current.position.value - value);
				worldPositionFromScreenPosition.y = 0f;
				worldPositionFromScreenPosition2.y = 0f;
				Vector3 translation = worldPositionFromScreenPosition2 - worldPositionFromScreenPosition;
				translation.y = 0f;
				m_camera.transform.Translate(translation, Space.World);
				m_gamepadCursorWorldPosition = GetWorldPositionFromScreenPosition(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
				ClampCameraToBounds();
				SetCameraDirty();
				return true;
			}
		}
		return false;
	}

	private void OnMapScrollInputAction(InputAction.CallbackContext input)
	{
		if (Application.isFocused && !m_isAnimating)
		{
			Vector2 vector = (m_moveInput = input.ReadValue<Vector2>());
		}
	}

	private void OnMapZoomInputAction(InputAction.CallbackContext input)
	{
		if (!Application.isFocused || m_isAnimating)
		{
			return;
		}
		float num = input.ReadValue<float>();
		if (m_zoomInput == MapZoomInput.WaitForReset)
		{
			if (Mathf.Abs(num) < 0.25f)
			{
				m_zoomInput = MapZoomInput.None;
			}
		}
		else if (m_zoomInput == MapZoomInput.None)
		{
			if (num > 0.5f)
			{
				m_zoomInput = MapZoomInput.ZoomIn;
			}
			else if (num < -0.5f)
			{
				m_zoomInput = MapZoomInput.ZoomOut;
			}
		}
	}

	private IEnumerator ResetZoomInputFlagDelay()
	{
		yield return new WaitForSecondsRealtime(0.1f);
		if (m_zoomInput == MapZoomInput.WaitForReset)
		{
			m_zoomInput = MapZoomInput.None;
		}
	}

	private void RefreshPlayerArea()
	{
		m_activePlayerArea = null;
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item != null)
		{
			m_activePlayerArea = item.GetActivePlayerMapAreaMetadata();
		}
	}

	public PlayerPositionData GetPlayerPositionData(bool instant)
	{
		RefreshPlayerArea();
		PlayerPositionData result = default(PlayerPositionData);
		result.m_showPlayerPosition = false;
		result.m_baseRotation = Quaternion.identity;
		result.m_unmappedLocation = true;
		if (m_activePlayerArea != null)
		{
			if (m_floors != null)
			{
				bool flag = false;
				foreach (Map3DFloor floor in m_floors)
				{
					if (floor.IncludesArea(m_activePlayerArea))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					result.m_unmappedLocation = false;
				}
			}
			bool flag2 = false;
			MapArea3D[] mapAreas3D = m_mapAreas3D;
			foreach (MapArea3D mapArea3D in mapAreas3D)
			{
				if (mapArea3D.MapAreaMetadata == m_activePlayerArea)
				{
					if (mapArea3D.ShouldShowPlayerPosition())
					{
						result.m_showPlayerPosition = true;
					}
					mapArea3D.GetPlayerMarkerWorldPosition(ref result.m_mapWorldPosition, ref result.m_baseRotation);
					flag2 = true;
					result.m_unmappedLocation = false;
					break;
				}
			}
			if (!flag2)
			{
				mapAreas3D = m_mapAreas3D;
				foreach (MapArea3D mapArea3D2 in mapAreas3D)
				{
					if (mapArea3D2.IsMatchingUnmappedAreaLocationParent())
					{
						result.m_mapWorldPosition = mapArea3D2.transform.position;
						result.m_unmappedLocation = true;
					}
				}
			}
		}
		return result;
	}

	public bool TracksLevel(LevelMetadata level)
	{
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		for (int i = 0; i < mapAreas3D.Length; i++)
		{
			if (mapAreas3D[i].TracksLevel(level))
			{
				return true;
			}
		}
		return false;
	}

	public bool TracksMapArea(BaseMapAreaMetadata mapArea)
	{
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		for (int i = 0; i < mapAreas3D.Length; i++)
		{
			if (mapAreas3D[i].MapAreaMetadata == mapArea)
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 Get3DMapWorldPositionFromLevelPosition(BaseMapAreaMetadata mapArea, Vector3 worldPosition)
	{
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		foreach (MapArea3D mapArea3D in mapAreas3D)
		{
			if (mapArea3D.MapAreaMetadata == mapArea)
			{
				return mapArea3D.Get3DMapWorldPositionFromLevelPosition(worldPosition);
			}
		}
		return Vector3.zero;
	}

	public Vector3 Get3DMapWorldPositionFromCursor()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		Vector3 position = new Vector3(vector.x, vector.y, Vector3.Distance(m_camera.transform.position, m_cameraLookTransform.position));
		return m_camera.ScreenToWorldPoint(position);
	}

	public Vector2 GetWorldToViewportPoint(Vector3 worldPosition)
	{
		return m_camera.WorldToViewportPoint(worldPosition);
	}

	public void FocusCameraOnMapPosition(Vector3 mapPosition, bool ignoreClamping)
	{
		Vector3 translation = mapPosition - m_cameraLookTransform.position;
		m_camera.transform.Translate(translation, Space.World);
		if (!ignoreClamping)
		{
			ClampCameraToBounds();
		}
		m_gamepadCursorWorldPosition = mapPosition;
		SetCameraDirty();
	}

	public IEnumerator FocusCameraOnMapPositionAnimated(Vector3 mapPosition, float speed = 100f)
	{
		if (m_animationCoroutine != null)
		{
			StopCoroutine(m_animationCoroutine);
			DOTween.Kill(m_camera.transform);
		}
		m_gamepadCursorWorldPosition = mapPosition;
		m_animationCoroutine = StartCoroutine(FocusCameraOnMapPositionAnimated_Coroutine(mapPosition, speed));
		yield return m_animationCoroutine;
	}

	private IEnumerator FocusCameraOnMapPositionAnimated_Coroutine(Vector3 mapPosition, float speed = 100f)
	{
		Vector3 vector = mapPosition - m_cameraLookTransform.position;
		Vector3 endValue = m_camera.transform.position + vector;
		IsAnimating = true;
		yield return m_camera.transform.DOMove(endValue, speed).SetSpeedBased(isSpeedBased: true).WaitForCompletion();
		yield return new WaitForEndOfFrame();
		IsAnimating = false;
	}

	public bool FocusOnRegionIfPossible(LevelRegionSettings activeRegion)
	{
		MapRegionFocalPoint[] focalPoints = m_focalPoints;
		foreach (MapRegionFocalPoint mapRegionFocalPoint in focalPoints)
		{
			if (mapRegionFocalPoint.AssociatedRegion == activeRegion)
			{
				SetZoomLevel(mapRegionFocalPoint.ZoomLevel);
				FocusCameraOnMapPosition(mapRegionFocalPoint.transform.position, ignoreClamping: true);
				return true;
			}
		}
		return false;
	}

	public void UpdateObjectives()
	{
		ActiveObjective activeObjective = GlobalReferences.Instance.ActiveObjective;
		BaseMapObjective[] componentsInChildren = GetComponentsInChildren<BaseMapObjective>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateForObjectiveState(activeObjective.ActiveObjectives, activeObjective.DirtyFlag);
		}
		activeObjective.ClearDirtyFlag();
	}

	public void HideObjectives()
	{
		BaseMapObjective[] componentsInChildren = GetComponentsInChildren<BaseMapObjective>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateForObjectiveState(MapObjectiveState.None, MapObjectiveState.None);
		}
	}

	public Vector2 GetGamepadCursorScreenPosition()
	{
		return m_camera.WorldToScreenPoint(m_gamepadCursorWorldPosition);
	}

	public void DrawFogOfWarVisibilityMeshes(CommandBuffer cmd, Material material)
	{
		if (m_playerPositionMarker != null && m_playerPositionMarker.gameObject.activeInHierarchy)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetTRS(m_playerPositionMarker.transform.position, Quaternion.identity, Vector3.one * m_playerFogOfWarRevealMeshScale);
			cmd.DrawMesh(m_playerFogOfWarRevealMesh, identity, material, 0, 0);
		}
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		for (int i = 0; i < mapAreas3D.Length; i++)
		{
			mapAreas3D[i].DrawFogOfWarVisibilityMeshes(cmd, material);
		}
		MapAreaConnectionLine[] connectionLines = m_connectionLines;
		for (int i = 0; i < connectionLines.Length; i++)
		{
			connectionLines[i].DrawFogOfWarVisibilityMeshes(cmd, material);
		}
		WorldMapRevealerMesh[] mapRevealerMeshes = m_mapRevealerMeshes;
		foreach (WorldMapRevealerMesh worldMapRevealerMesh in mapRevealerMeshes)
		{
			if (worldMapRevealerMesh.isActiveAndEnabled)
			{
				worldMapRevealerMesh.DrawFogOfWarVisibilityMeshes(cmd, material);
			}
		}
	}

	public void DrawBackgroundTerrain(CommandBuffer cmd, Material backgroundMeshMaterial)
	{
		Matrix4x4 localToWorldMatrix = m_backgroundTerrain.transform.localToWorldMatrix;
		cmd.DrawMesh(m_backgroundTerrain.GetComponent<MeshFilter>().mesh, localToWorldMatrix, m_backgroundTerrain.material, 0, 0);
		MapArea3D[] mapAreas3D = m_mapAreas3D;
		for (int i = 0; i < mapAreas3D.Length; i++)
		{
			mapAreas3D[i].DrawBackgroundMeshes(cmd, backgroundMeshMaterial);
		}
		MapAreaConnectionLine[] connectionLines = m_connectionLines;
		for (int i = 0; i < connectionLines.Length; i++)
		{
			connectionLines[i].DrawBackgroundMeshes(cmd, backgroundMeshMaterial);
		}
	}

	public void SwitchToPlayerFloor()
	{
		RefreshPlayerArea();
		if (m_floors == null || m_floors.Count <= 0 || !(m_activePlayerArea != null))
		{
			return;
		}
		int num = 0;
		bool flag = false;
		foreach (Map3DFloor floor in m_floors)
		{
			if (floor.IncludesArea(m_activePlayerArea))
			{
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			num = 0;
			Debug.LogWarning("Cannot find which floor the player is on!");
		}
		SelectActiveFloor(num, isInitial: true);
	}

	private void SelectActiveFloor(int index, bool isInitial = false)
	{
		if (index == m_currentFloorIndex)
		{
			return;
		}
		for (int i = 0; i < m_floors.Count; i++)
		{
			if (index != i)
			{
				m_floors[i].gameObject.SetActive(value: false);
			}
		}
		m_currentFloorIndex = index;
		m_floors[m_currentFloorIndex].gameObject.SetActive(value: true);
		m_floors[m_currentFloorIndex].SetMode(FloorMode.Active);
		if (m_currentFloorIndex > 0)
		{
			m_floors[m_currentFloorIndex - 1].gameObject.SetActive(value: true);
			m_floors[m_currentFloorIndex - 1].SetMode(FloorMode.Below);
		}
		if (isInitial)
		{
			GlobalReferences.Instance.EventChannels.Map.InitialFloorMapApplied.Raise(m_floors[m_currentFloorIndex]);
		}
		else
		{
			GlobalReferences.Instance.EventChannels.Map.ActiveMapFloorChanged.Raise(m_floors[m_currentFloorIndex]);
		}
		if (m_floors[m_currentFloorIndex].MapBackgroundMaterial != null)
		{
			m_backgroundTerrain.sharedMaterial = m_floors[m_currentFloorIndex].MapBackgroundMaterial;
		}
		MapDetailsLayer[] mapDetailsLayers = m_mapDetailsLayers;
		for (int j = 0; j < mapDetailsLayers.Length; j++)
		{
			mapDetailsLayers[j].OnFloorChanged(m_floors[index]);
		}
		Populate3DMapAreaReferences();
		Refresh();
		if (m_currentFloorIndex > 0)
		{
			m_floors[m_currentFloorIndex - 1].RefreshChildrenAsBelowFloor();
		}
	}

	public void SelectActiveFloor(Map3DFloor floor)
	{
		if (m_floors.Contains(floor))
		{
			SelectActiveFloor(m_floors.IndexOf(floor));
		}
	}

	public void UpFloorPressed()
	{
		for (int i = m_currentFloorIndex + 1; i < m_floors.Count; i++)
		{
			if (m_floors[i].ShouldFloorBeVisible())
			{
				SelectActiveFloor(i);
				break;
			}
		}
	}

	public void DownFloorPressed()
	{
		for (int num = m_currentFloorIndex - 1; num >= 0; num--)
		{
			if (m_floors[num].ShouldFloorBeVisible())
			{
				SelectActiveFloor(num);
				break;
			}
		}
	}

	public bool SwitchToFloorForLore(LoreEntry loreEntry)
	{
		bool flag = false;
		if (m_floors.Count > 0)
		{
			int num = 0;
			foreach (Map3DFloor floor in m_floors)
			{
				if (floor.ContainsLore(loreEntry))
				{
					flag = true;
					break;
				}
				num++;
			}
			if (flag)
			{
				SelectActiveFloor(num);
			}
		}
		return flag;
	}

	public bool SwitchToFloorForMapArea(BaseMapAreaMetadata mapArea)
	{
		bool flag = false;
		if (m_floors.Count > 0)
		{
			int num = 0;
			foreach (Map3DFloor floor in m_floors)
			{
				if (floor.IncludesArea(mapArea))
				{
					flag = true;
					break;
				}
				num++;
			}
			if (flag)
			{
				SelectActiveFloor(num);
			}
		}
		return flag;
	}
}
