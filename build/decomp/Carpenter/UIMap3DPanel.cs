using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class UIMap3DPanel : MonoBehaviour
{
	private enum RepopulateRequest
	{
		None,
		NewMap,
		FloorChanged,
		FloorChangedInitialUpdate
	}

	[SerializeField]
	private GraphicRaycaster m_graphicRaycaster;

	[SerializeField]
	private RawImage m_rawMapRenderImage;

	[SerializeField]
	private UserMapData m_userMapData;

	[Header("UI Panels")]
	[SerializeField]
	private MapViewInfoPanel m_mapViewInfoPanel;

	[SerializeField]
	private MapFloorsListInfo m_mapFloorsListInfo;

	[SerializeField]
	private GameObject m_locationUnknownPanel;

	[Header("Icons & Photos")]
	[SerializeField]
	private RectTransform m_iconsParent;

	[SerializeField]
	private MapVisualsSettings m_mapIconSettings;

	[SerializeField]
	private MapGeneralIcon m_mapGeneralIconPrefab;

	[SerializeField]
	private MapPlayerIcon m_mapPlayerIconPrefab;

	[SerializeField]
	private MapItemIcon m_mapItemIconPrefab;

	[SerializeField]
	private MapUserMarkerIcon m_mapUserMarkerPrefab;

	[SerializeField]
	private MapPhoto m_mapPhotoPrefab;

	[SerializeField]
	private MapLoreEntryIcon m_mapLoreEntryIconPrefab;

	[SerializeField]
	private MapPointOfInterestIcon m_mapPointOfInterestPrefab;

	[SerializeField]
	private PhotoViewer m_photoViewer;

	[SerializeField]
	private MapMarkerOptionsPanel m_markerOptionsPanel;

	[SerializeField]
	private MapRegionAreaIcon m_mapRegionAreaPrefab;

	[Header("Scripts")]
	[SerializeField]
	private StringVariable m_mapScriptTrigger;

	[Header("Info Panel")]
	[SerializeField]
	private MapHighlightInfoPanel m_highlightInfoPanel;

	[SerializeField]
	private MapIconActionsPanel m_iconActionsPanel;

	[SerializeField]
	private MapType m_mapType;

	[Header("Strigns")]
	[SerializeField]
	private LocalizedString m_stringTooManyMarkers;

	private List<MapUserMarkerIcon> m_activeUserMarkers;

	private List<MapPhoto> m_activePhotos;

	private List<MapGeneralIcon> m_activeGeneralIcons;

	private List<Map3DIcon> m_activeIcons;

	private List<MapRegionAreaIcon> m_activeMapRegionAreaIcons;

	private MapPlayerIcon m_playerIcon;

	private Map3DIcon m_hoveredMapIcon;

	public UnityAction<Map3DIcon> OnMapIconHovered;

	public UnityAction<Map3DIcon> OnMapIconClicked;

	public UnityAction OnMapMoved;

	private readonly List<Map3DIcon> m_hoveredIcons = new List<Map3DIcon>();

	private int m_hoverCycleIndex = -1;

	private bool m_mapViewActive;

	private RepopulateRequest m_repopulateRequest;

	private LoreEntry m_focusOnLoreAfterFloorChanged;

	private PhotoData m_focusOnPhotoAfterFloorChanged;

	private bool m_shouldMainMapReposition;

	private LoreEntry m_queuedFocusLoreEntry;

	private float m_blockSelectMapMarkerTime;

	private List<MapIconGroup> m_iconGroups = new List<MapIconGroup>();

	private bool m_waitingForMapChange;

	public bool IsHoveringIcon => m_hoveredMapIcon != null;

	public bool MapViewActive
	{
		get
		{
			return m_mapViewActive;
		}
		set
		{
			if (m_mapViewActive == value)
			{
				return;
			}
			m_mapViewActive = value;
			if (m_mapViewActive)
			{
				GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Raise(GetStartingMapViewMetadata());
			}
			GlobalReferences.Instance.EventChannels.Map.SetMapActive.Raise(value);
			Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
			if (m_mapType == MapType.MainMap)
			{
				GlobalReferences.Instance.EventChannels.Camera.SetMainCameraEnabled.Raise(!m_mapViewActive);
				if (m_mapViewActive && item != null)
				{
					item.UpdateObjectives();
				}
			}
			else if (m_mapViewActive && item != null)
			{
				item.HideObjectives();
			}
			if (m_mapViewActive)
			{
				m_repopulateRequest = RepopulateRequest.NewMap;
			}
			if (item != null)
			{
				item.IsAnimating = false;
			}
		}
	}

	private float GroupingProximityThreshold => 48f / (float)Screen.width;

	private MapViewMetadata GetStartingMapViewMetadata()
	{
		LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		MapViewMetadata[] mapviewMetadatas = GlobalReferences.Instance.MapviewMetadatas;
		foreach (MapViewMetadata mapViewMetadata in mapviewMetadatas)
		{
			if (mapViewMetadata.EncompassesLevel(item))
			{
				return mapViewMetadata;
			}
		}
		return null;
	}

	public void SetMapShouldReposition()
	{
		m_shouldMainMapReposition = true;
	}

	public void SetQueuedFocusLoreEntry(LoreEntry loreEntry)
	{
		m_queuedFocusLoreEntry = loreEntry;
	}

	private void Awake()
	{
		m_activeUserMarkers = new List<MapUserMarkerIcon>();
		m_activePhotos = new List<MapPhoto>();
		m_activeIcons = new List<Map3DIcon>();
		m_activeGeneralIcons = new List<MapGeneralIcon>();
		m_activeMapRegionAreaIcons = new List<MapRegionAreaIcon>();
		m_playerIcon = UnityEngine.Object.Instantiate(m_mapPlayerIconPrefab, m_iconsParent);
		m_activeIcons.Add(m_playerIcon);
		MapPlayerIcon playerIcon = m_playerIcon;
		playerIcon.OnStartedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(playerIcon.OnStartedHovered, new UnityAction<Map3DIcon>(OnMapIconStartHovered));
		MapPlayerIcon playerIcon2 = m_playerIcon;
		playerIcon2.OnStoppedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(playerIcon2.OnStoppedHovered, new UnityAction<Map3DIcon>(OnMapIconStopHovered));
		if (m_photoViewer != null)
		{
			PhotoViewer photoViewer = m_photoViewer;
			photoViewer.OnFocusOnPhoto = (UnityAction<PhotoData>)Delegate.Combine(photoViewer.OnFocusOnPhoto, new UnityAction<PhotoData>(FocusOnPhoto));
			PhotoViewer photoViewer2 = m_photoViewer;
			photoViewer2.OnDeletePhoto = (UnityAction<PhotoData>)Delegate.Combine(photoViewer2.OnDeletePhoto, new UnityAction<PhotoData>(DeletePhoto));
		}
	}

	private void Start()
	{
		if (m_iconActionsPanel != null)
		{
			MapIconActionsPanel iconActionsPanel = m_iconActionsPanel;
			iconActionsPanel.OnCancelClicked = (UnityAction)Delegate.Combine(iconActionsPanel.OnCancelClicked, new UnityAction(CloseIconActionsPanel));
			MapIconActionsPanel iconActionsPanel2 = m_iconActionsPanel;
			iconActionsPanel2.OnViewClicked = (UnityAction)Delegate.Combine(iconActionsPanel2.OnViewClicked, new UnityAction(ViewIconClicked));
			MapIconActionsPanel iconActionsPanel3 = m_iconActionsPanel;
			iconActionsPanel3.OnDeleteClicked = (UnityAction)Delegate.Combine(iconActionsPanel3.OnDeleteClicked, new UnityAction(DeleteIconClicked));
			MapIconActionsPanel iconActionsPanel4 = m_iconActionsPanel;
			iconActionsPanel4.OnEditClicked = (UnityAction)Delegate.Combine(iconActionsPanel4.OnEditClicked, new UnityAction(EditIconClicked));
			MapIconActionsPanel iconActionsPanel5 = m_iconActionsPanel;
			iconActionsPanel5.OnActionsPanelClosed = (UnityAction)Delegate.Combine(iconActionsPanel5.OnActionsPanelClosed, new UnityAction(ActionPanelClosedCallback));
		}
	}

	private void OnEnable()
	{
		RectTransform component = GetComponent<RectTransform>();
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (component != null && componentInParent != null)
		{
			component.sizeDelta = componentInParent.GetComponent<RectTransform>().sizeDelta;
		}
		GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Register(OnMapViewChanged);
		GlobalReferences.Instance.EventChannels.Map.ActiveMapFloorChanged.Register(OnFloorChanged);
		GlobalReferences.Instance.EventChannels.Map.InitialFloorMapApplied.Register(OnFloorChangedInitial);
		GlobalReferences.Instance.EventChannels.Map.OnMapCameraRendered.Register(OnMapCameraRendered);
		m_rawMapRenderImage.texture = MapSceneManager.RenderTexture;
		if (m_mapType == MapType.MainMap)
		{
			GameInputManager.GameInputActions.UI.MapUpFloor.performed += OnUpFloorInput;
			GameInputManager.GameInputActions.UI.MapDownFloor.performed += OnDownFloorInput;
			GameInputManager.GameInputActions.UI.MapCycleSelection.performed += OnMapCycleIcon;
			GameInputManager.GameInputActions.UI.MapViewPhotos.performed += OnViewPhotos;
			GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Set(this);
		}
		if (m_markerOptionsPanel != null)
		{
			MapMarkerOptionsPanel markerOptionsPanel = m_markerOptionsPanel;
			markerOptionsPanel.OnClosedAction = (UnityAction)Delegate.Combine(markerOptionsPanel.OnClosedAction, new UnityAction(OnClosedMapMarkerPanel));
		}
		PopulateFloors();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Unregister(OnMapViewChanged);
		GlobalReferences.Instance.EventChannels.Map.ActiveMapFloorChanged.Unregister(OnFloorChanged);
		GlobalReferences.Instance.EventChannels.Map.InitialFloorMapApplied.Unregister(OnFloorChangedInitial);
		GlobalReferences.Instance.EventChannels.Map.OnMapCameraRendered.Unregister(OnMapCameraRendered);
		if (m_photoViewer != null)
		{
			m_photoViewer.Close();
		}
		MapViewActive = false;
		if (m_markerOptionsPanel != null)
		{
			m_markerOptionsPanel.Close();
			MapMarkerOptionsPanel markerOptionsPanel = m_markerOptionsPanel;
			markerOptionsPanel.OnClosedAction = (UnityAction)Delegate.Remove(markerOptionsPanel.OnClosedAction, new UnityAction(OnClosedMapMarkerPanel));
		}
		if (m_mapType == MapType.MainMap)
		{
			GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Set(null);
			if (GameInputManager.GameInputActions != null)
			{
				GameInputManager.GameInputActions.UI.MapUpFloor.performed -= OnUpFloorInput;
				GameInputManager.GameInputActions.UI.MapDownFloor.performed -= OnDownFloorInput;
				GameInputManager.GameInputActions.UI.MapCycleSelection.performed -= OnMapCycleIcon;
				GameInputManager.GameInputActions.UI.MapViewPhotos.performed -= OnViewPhotos;
			}
		}
	}

	private void OnMapCameraRendered()
	{
		PositionIcons();
	}

	public void ClearHoveredIcon()
	{
		if (m_hoveredMapIcon != null)
		{
			m_hoveredMapIcon = null;
			OnMapIconHovered?.Invoke(null);
		}
	}

	public void ClearActiveActionPanel()
	{
		if (m_iconActionsPanel.isActiveAndEnabled)
		{
			CloseIconActionsPanel();
		}
	}

	private void OnMapViewChanged(Map3D map3D)
	{
		if (map3D != null)
		{
			ClearHoveredIcon();
			PopulateFloors();
			m_repopulateRequest = RepopulateRequest.NewMap;
		}
	}

	private void OnAddMapMarkerInput(InputAction.CallbackContext input)
	{
		if (!m_mapViewActive || m_photoViewer.IsActive || m_markerOptionsPanel.IsActive || (bool)m_iconActionsPanel.ActiveIcon || !(m_hoveredMapIcon == null))
		{
			return;
		}
		if (m_userMapData.GetAvailableMarkerCount() <= 0)
		{
			GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Raise(new MenuInfoMessageData
			{
				m_stringReference = m_stringTooManyMarkers
			});
			return;
		}
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (item != null)
		{
			Vector3 worldPosition = item.Get3DMapWorldPositionFromCursor();
			MapUserMarkerData markerData = m_userMapData.AddNewUserMarker(worldPosition, item.GetUniqueID(), item.ActiveFloorIndex);
			MapUserMarkerIcon mapUserMarkerIcon = (MapUserMarkerIcon)(m_hoveredMapIcon = GenerateUserMarkerIcon(markerData));
			m_highlightInfoPanel.SetMap3DIcon(mapUserMarkerIcon);
			m_highlightInfoPanel.ShowButtonPrompts(show: false);
			ShowMapMarkerEditPanel(mapUserMarkerIcon);
			GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
		}
	}

	private void OnClosedMapMarkerPanel()
	{
		m_blockSelectMapMarkerTime = Time.realtimeSinceStartup + 0.15f;
		if (m_hoveredMapIcon != null)
		{
			m_highlightInfoPanel.SetMap3DIcon(m_hoveredMapIcon);
			m_highlightInfoPanel.ShowButtonPrompts(show: true);
		}
	}

	public bool IsInSubMenu()
	{
		if (m_markerOptionsPanel != null && m_markerOptionsPanel.IsActive)
		{
			return true;
		}
		if (m_photoViewer != null && m_photoViewer.IsActive)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (item == null || !m_mapViewActive)
		{
			return;
		}
		if (m_repopulateRequest != 0)
		{
			item.Populate3DMapAreaReferences();
			bool focusOnPlayer = m_repopulateRequest == RepopulateRequest.NewMap || m_repopulateRequest == RepopulateRequest.FloorChangedInitialUpdate;
			if (m_mapType == MapType.MainMap && (!m_shouldMainMapReposition || m_queuedFocusLoreEntry != null))
			{
				focusOnPlayer = false;
			}
			m_shouldMainMapReposition = false;
			PopulateMap(m_mapType == MapType.QuickMap, focusOnPlayer);
			m_repopulateRequest = RepopulateRequest.None;
			if (m_queuedFocusLoreEntry != null)
			{
				FocusOnLoreIcon(m_queuedFocusLoreEntry);
				m_queuedFocusLoreEntry = null;
			}
		}
		if (m_mapType == MapType.MainMap)
		{
			bool flag = true;
			Vector2 position = Mouse.current.position.ReadValue();
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = position;
			pointerEventData.radius = new Vector2(100f, 100f);
			List<RaycastResult> list = new List<RaycastResult>();
			m_graphicRaycaster.Raycast(pointerEventData, list);
			List<Map3DIcon> list2 = new List<Map3DIcon>();
			foreach (RaycastResult item2 in list)
			{
				if ((bool)item2.gameObject.GetComponentInParent<Button>())
				{
					flag = false;
					break;
				}
				Map3DIcon componentInParent = item2.gameObject.GetComponentInParent<Map3DIcon>();
				if (componentInParent != null && !list2.Contains(componentInParent))
				{
					list2.Add(componentInParent);
				}
			}
			if (!ListsContainSameElements(m_hoveredIcons, list2))
			{
				m_hoveredIcons.Clear();
				m_hoveredIcons.AddRange(list2);
				m_hoverCycleIndex = 0;
				m_highlightInfoPanel.SetGroupInfo(m_hoveredIcons.Count, m_hoverCycleIndex);
			}
			bool flag2 = flag;
			Map3DIcon map3DIcon = null;
			if (m_photoViewer != null && m_photoViewer.IsActive)
			{
				flag2 = false;
			}
			if (m_markerOptionsPanel != null && m_markerOptionsPanel.IsActive)
			{
				flag2 = false;
				map3DIcon = m_markerOptionsPanel.ActiveMarkerIcon;
			}
			else if (m_iconActionsPanel != null && (bool)m_iconActionsPanel.ActiveIcon)
			{
				flag2 = false;
				map3DIcon = m_iconActionsPanel.ActiveIcon;
			}
			if (item.IsAnimating)
			{
				flag2 = false;
			}
			if (flag2)
			{
				if (item.InputUpdate())
				{
					OnMapMoved?.Invoke();
				}
				else if (m_hoveredMapIcon != null)
				{
					item.GamepadCursorMoveTowardsScreenPosition(m_hoveredMapIcon.FocusPoint);
				}
			}
			foreach (Map3DIcon activeIcon in m_activeIcons)
			{
				if (!(activeIcon.ParentGroup != null))
				{
					activeIcon.IsMuted = map3DIcon != null && map3DIcon != activeIcon;
					activeIcon.IsSelected = map3DIcon == activeIcon;
				}
			}
		}
		else if (m_mapType == MapType.QuickMap)
		{
			PositionPlayer(focusCamera: false, snap: false);
		}
		if (m_highlightInfoPanel != null)
		{
			UpdateHighlightInfoPanel();
		}
	}

	private bool ListsContainSameElements(List<Map3DIcon> a, List<Map3DIcon> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		return new HashSet<Map3DIcon>(a).SetEquals(b);
	}

	public Map3D.PlayerPositionData PositionPlayer(bool focusCamera, bool snap)
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (item != null)
		{
			Map3D.PlayerPositionData playerPositionData = item.GetPlayerPositionData(m_mapType == MapType.MainMap);
			if (focusCamera && !playerPositionData.m_unmappedLocation)
			{
				item.FocusCameraOnMapPosition(playerPositionData.m_mapWorldPosition, ignoreClamping: false);
			}
			if (playerPositionData.m_showPlayerPosition)
			{
				m_playerIcon.gameObject.SetActive(value: true);
				m_playerIcon.MapWorldPosition = playerPositionData.m_mapWorldPosition;
				m_playerIcon.UpdateRotation(playerPositionData.m_baseRotation, snap);
				m_playerIcon.transform.SetAsLastSibling();
			}
			else
			{
				m_playerIcon.gameObject.SetActive(value: false);
			}
			return playerPositionData;
		}
		return default(Map3D.PlayerPositionData);
	}

	public void PopulateMap(bool isQuickMap, bool focusOnPlayer)
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (item == null)
		{
			return;
		}
		if (m_mapViewInfoPanel != null)
		{
			m_mapViewInfoPanel.SetActiveMapView(item.Metadata);
		}
		foreach (MapIconGroup iconGroup in m_iconGroups)
		{
			iconGroup.Cleanup();
		}
		m_iconGroups.Clear();
		ClearHoveredIcon();
		PopulateGeneralIcons();
		PopulatePhotos();
		PopulateUserMarkers();
		PopulateWorldMapRegions();
		Map3D.PlayerPositionData playerPositionData = PositionPlayer(focusOnPlayer, snap: true);
		if (m_locationUnknownPanel != null)
		{
			m_locationUnknownPanel.SetActive(playerPositionData.m_unmappedLocation);
		}
		PositionIcons();
		bool flag = false;
		LevelMetadata item2 = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item2 != null && item.FocusOnRegionIfPossible(item2.Region))
		{
			flag = true;
		}
		if (!flag && isQuickMap)
		{
			if (item2 != null && item2.LevelType == LevelMetadata.MetadataLevelType.BoatTravel)
			{
				item.SetQuickMapBoatTravelZoom();
			}
			else
			{
				item.SetQuickMapZoomLevel();
			}
		}
		item.SetMapType(isQuickMap ? MapType.QuickMap : MapType.MainMap);
	}

	private void GroupIcons(Map3DIcon iconA, Map3DIcon iconB)
	{
		if (iconA.ParentGroup == null && iconB.ParentGroup == null)
		{
			MapIconGroup item = MapIconGroup.Create(m_iconsParent, iconA, iconB);
			m_iconGroups.Add(item);
		}
		else if (iconA.ParentGroup != null && iconB.ParentGroup == null)
		{
			iconA.ParentGroup.AddIcon(iconB);
		}
		else if (iconA.ParentGroup == null && iconB.ParentGroup != null)
		{
			iconB.ParentGroup.AddIcon(iconA);
		}
		else if (iconA.ParentGroup != null)
		{
			_ = iconB.ParentGroup != null;
		}
	}

	private void PositionIcons()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		float zoomAlpha = m_mapIconSettings.IconAlphaCurve.Evaluate(item.Camera.orthographicSize);
		float orthographicSize = item.Camera.orthographicSize;
		float num = 64f;
		Vector3 localScale = Vector3.one * num / orthographicSize;
		foreach (Map3DIcon activeIcon in m_activeIcons)
		{
			if (activeIcon.ParentGroup == null)
			{
				Vector2 vector2 = (activeIcon.RectTransform.anchorMin = (activeIcon.RectTransform.anchorMax = item.GetWorldToViewportPoint(activeIcon.MapWorldPosition)));
			}
			if (activeIcon.ShouldScale)
			{
				activeIcon.transform.localScale = localScale;
			}
			activeIcon.ZoomAlpha = zoomAlpha;
		}
		foreach (MapIconGroup iconGroup in m_iconGroups)
		{
			iconGroup.PositionGroup(item);
		}
	}

	private void ClearExistingPhotos()
	{
		foreach (MapPhoto activePhoto in m_activePhotos)
		{
			UnityEngine.Object.Destroy(activePhoto.gameObject);
			m_activeIcons.Remove(activePhoto);
		}
		m_activePhotos.Clear();
	}

	private void PopulateFloors()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		bool flag = item != null && item.HasFloors;
		if (flag)
		{
			item.SwitchToPlayerFloor();
		}
		if (m_mapFloorsListInfo != null)
		{
			m_mapFloorsListInfo.SetFloorsData(flag ? item.Floors : null, item ? item.ActivePlayerArea : null);
		}
		if (flag)
		{
			if (m_mapViewInfoPanel != null)
			{
				m_mapViewInfoPanel.SetActiveFloor(item.ActiveFloor);
			}
			if (m_mapFloorsListInfo != null)
			{
				m_mapFloorsListInfo.SetActiveFloor(item.ActiveFloor, item.ActivePlayerArea);
			}
		}
		else if (m_mapViewInfoPanel != null)
		{
			m_mapViewInfoPanel.SetActiveFloor(null);
		}
	}

	private void SharedFloorChanged(Map3DFloor floor)
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (m_mapViewInfoPanel != null)
		{
			m_mapViewInfoPanel.SetActiveFloor(floor);
		}
		if (m_mapFloorsListInfo != null)
		{
			m_mapFloorsListInfo.SetActiveFloor(floor, item.ActivePlayerArea);
		}
	}

	private void OnFloorChanged(Map3DFloor floor)
	{
		SharedFloorChanged(floor);
		m_repopulateRequest = RepopulateRequest.FloorChanged;
	}

	private void OnFloorChangedInitial(Map3DFloor floor)
	{
		SharedFloorChanged(floor);
		m_repopulateRequest = RepopulateRequest.FloorChangedInitialUpdate;
		m_shouldMainMapReposition = true;
	}

	private void OnUpFloorInput(InputAction.CallbackContext obj)
	{
		UpFloorPressed();
	}

	public void UpFloorPressed()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (!(item == null))
		{
			item.UpFloorPressed();
		}
	}

	private void OnDownFloorInput(InputAction.CallbackContext obj)
	{
		DownFloorPressed();
	}

	public void DownFloorPressed()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (!(item == null))
		{
			item.DownFloorPressed();
		}
	}

	private void PopulatePhotos()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		ClearExistingPhotos();
		foreach (PhotoData photo in m_userMapData.Photos)
		{
			if (item.TracksMapArea(photo.MapAreaMetadata))
			{
				MapPhoto mapPhoto = UnityEngine.Object.Instantiate(m_mapPhotoPrefab, m_iconsParent);
				mapPhoto.SetPhotoData(photo);
				mapPhoto.MapWorldPosition = item.Get3DMapWorldPositionFromLevelPosition(photo.MapAreaMetadata, photo.WorldPosition);
				mapPhoto.OnClicked = (UnityAction<Map3DIcon>)Delegate.Combine(mapPhoto.OnClicked, new UnityAction<Map3DIcon>(OnPhotoClicked));
				mapPhoto.OnStartedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapPhoto.OnStartedHovered, new UnityAction<Map3DIcon>(OnMapIconStartHovered));
				mapPhoto.OnStoppedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapPhoto.OnStoppedHovered, new UnityAction<Map3DIcon>(OnMapIconStopHovered));
				m_activeIcons.Add(mapPhoto);
				m_activePhotos.Add(mapPhoto);
				if (m_focusOnPhotoAfterFloorChanged != null && photo == m_focusOnPhotoAfterFloorChanged)
				{
					FocusOnPhoto(m_focusOnPhotoAfterFloorChanged);
					m_focusOnPhotoAfterFloorChanged = null;
				}
			}
		}
	}

	private void ClearExistingUserMarkers()
	{
		foreach (MapUserMarkerIcon activeUserMarker in m_activeUserMarkers)
		{
			UnityEngine.Object.Destroy(activeUserMarker.gameObject);
			m_activeIcons.Remove(activeUserMarker);
		}
		m_activeUserMarkers.Clear();
	}

	private void PopulateUserMarkers()
	{
		ClearExistingUserMarkers();
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		int uniqueID = item.GetUniqueID();
		int activeFloorIndex = item.ActiveFloorIndex;
		foreach (MapUserMarkerData marker in m_userMapData.Markers)
		{
			if (marker.MapID == uniqueID && marker.FloorIndex == activeFloorIndex)
			{
				GenerateUserMarkerIcon(marker);
			}
		}
	}

	private MapUserMarkerIcon GenerateUserMarkerIcon(MapUserMarkerData markerData)
	{
		MapUserMarkerIcon mapUserMarkerIcon = UnityEngine.Object.Instantiate(m_mapUserMarkerPrefab, m_iconsParent);
		mapUserMarkerIcon.SetMapUserMarkerData(markerData);
		mapUserMarkerIcon.MapWorldPosition = markerData.MapWorldPosition;
		m_activeIcons.Add(mapUserMarkerIcon);
		m_activeUserMarkers.Add(mapUserMarkerIcon);
		mapUserMarkerIcon.OnStartedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapUserMarkerIcon.OnStartedHovered, new UnityAction<Map3DIcon>(OnMapIconStartHovered));
		mapUserMarkerIcon.OnStoppedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapUserMarkerIcon.OnStoppedHovered, new UnityAction<Map3DIcon>(OnMapIconStopHovered));
		mapUserMarkerIcon.OnClicked = (UnityAction<Map3DIcon>)Delegate.Combine(mapUserMarkerIcon.OnClicked, new UnityAction<Map3DIcon>(OnMapMarkerClicked));
		return mapUserMarkerIcon;
	}

	private void ClearExistingGeneralIcons()
	{
		foreach (MapGeneralIcon activeGeneralIcon in m_activeGeneralIcons)
		{
			UnityEngine.Object.Destroy(activeGeneralIcon.gameObject);
			m_activeIcons.Remove(activeGeneralIcon);
		}
		m_activeGeneralIcons.Clear();
	}

	private void PopulateWorldMapRegions()
	{
		ClearExistingRegionIcons();
		if (m_mapRegionAreaPrefab == null)
		{
			return;
		}
		MapArea3D[] mapAreas = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.MapAreas;
		foreach (MapArea3D mapArea3D in mapAreas)
		{
			if (mapArea3D.MapAreaMetadata is RegionMapAreaMetadata regionData && mapArea3D.HasPlayerVisitedArea())
			{
				MapRegionAreaIcon mapRegionAreaIcon = UnityEngine.Object.Instantiate(m_mapRegionAreaPrefab, m_iconsParent);
				mapRegionAreaIcon.SetRegionData(regionData);
				mapRegionAreaIcon.MapWorldPosition = mapArea3D.GetRegionMapIconPosition();
				m_activeIcons.Add(mapRegionAreaIcon);
				m_activeMapRegionAreaIcons.Add(mapRegionAreaIcon);
				mapRegionAreaIcon.OnStartedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapRegionAreaIcon.OnStartedHovered, new UnityAction<Map3DIcon>(OnMapIconStartHovered));
				mapRegionAreaIcon.OnStoppedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapRegionAreaIcon.OnStoppedHovered, new UnityAction<Map3DIcon>(OnMapIconStopHovered));
				mapRegionAreaIcon.OnClicked = (UnityAction<Map3DIcon>)Delegate.Combine(mapRegionAreaIcon.OnClicked, new UnityAction<Map3DIcon>(OnRegionClicked));
			}
		}
	}

	private void OnRegionClicked(Map3DIcon icon)
	{
		MapRegionAreaIcon mapRegionAreaIcon = icon as MapRegionAreaIcon;
		GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Raise(mapRegionAreaIcon.MapViewMetadata);
	}

	private void ClearExistingRegionIcons()
	{
		foreach (MapRegionAreaIcon activeMapRegionAreaIcon in m_activeMapRegionAreaIcons)
		{
			UnityEngine.Object.Destroy(activeMapRegionAreaIcon.gameObject);
			m_activeIcons.Remove(activeMapRegionAreaIcon);
		}
		m_activeMapRegionAreaIcons.Clear();
	}

	private void PopulateGeneralIcons()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		ClearExistingGeneralIcons();
		MapArea3D[] mapAreas = item.MapAreas;
		foreach (MapArea3D mapArea3D in mapAreas)
		{
			BaseMapAreaMetadata mapAreaMetadata = mapArea3D.MapAreaMetadata;
			if (mapAreaMetadata == null)
			{
				Debug.LogWarning(mapArea3D.name + " is missing a map metadata");
				continue;
			}
			LevelMapAOMMetadata mapAOMMetadata = mapAreaMetadata.MapAOMMetadata;
			if (mapAOMMetadata == null || !mapArea3D.HasPlayerVisitedArea())
			{
				continue;
			}
			foreach (LevelMapAOMMetadata.AppearsOnMapInstance appearsOnMapInstance in mapAOMMetadata.AppearsOnMapInstances)
			{
				if (!m_mapIconSettings.IsSupportedIconType(appearsOnMapInstance.Settings.IconType))
				{
					continue;
				}
				MapIconState mapIconStateBitMask = GlobalReferences.Instance.MapDynamicData.GetMapIconStateBitMask(appearsOnMapInstance.Settings.MapIconGUID);
				bool flag = false;
				flag = ((appearsOnMapInstance.Settings.IconType != MapIconType.LoreEntry) ? (mapIconStateBitMask.HasFlag(MapIconState.Discovered) || GameDebugCommands.CHEAT_MAP_SHOW_EVERYTHING) : GlobalReferences.Instance.LoreInventory.HasLoreEntry(appearsOnMapInstance.LoreEntry));
				if (((appearsOnMapInstance.Settings.IconType == MapIconType.ItemPickup || appearsOnMapInstance.Settings.IconType == MapIconType.LootContainer) && (appearsOnMapInstance.ItemDefinition == null || (!appearsOnMapInstance.ItemDefinition.IsImportantItem && !appearsOnMapInstance.ItemDefinition.IsKey))) || !flag || mapIconStateBitMask.HasFlag(MapIconState.Cleared) || appearsOnMapInstance.Settings.IconType == MapIconType.LoreEntry)
				{
					continue;
				}
				MapGeneralIcon mapGeneralIcon;
				if (appearsOnMapInstance.Settings.IconType == MapIconType.PointOfInterest)
				{
					mapGeneralIcon = UnityEngine.Object.Instantiate(m_mapPointOfInterestPrefab, m_iconsParent);
				}
				else if (appearsOnMapInstance.Settings.IconType == MapIconType.ItemPickup || appearsOnMapInstance.Settings.IconType == MapIconType.LootContainer)
				{
					mapGeneralIcon = UnityEngine.Object.Instantiate(m_mapItemIconPrefab, m_iconsParent);
				}
				else
				{
					if (appearsOnMapInstance.Settings.IconType == MapIconType.BoatHarbor && !GlobalReferences.Instance.Variables.Progression.PV_BoatRepaired.Value)
					{
						continue;
					}
					mapGeneralIcon = UnityEngine.Object.Instantiate(m_mapGeneralIconPrefab, m_iconsParent);
					if (appearsOnMapInstance.Settings.IconType == MapIconType.BoatHarbor)
					{
						MapGeneralIcon mapGeneralIcon2 = mapGeneralIcon;
						mapGeneralIcon2.OnClicked = (UnityAction<Map3DIcon>)Delegate.Combine(mapGeneralIcon2.OnClicked, new UnityAction<Map3DIcon>(OnBoatMapIconClicked));
					}
				}
				mapGeneralIcon.Setup(appearsOnMapInstance, mapArea3D.MapAreaType);
				mapGeneralIcon.MapWorldPosition = item.Get3DMapWorldPositionFromLevelPosition(mapAreaMetadata, appearsOnMapInstance.WorldPosition);
				MapGeneralIcon mapGeneralIcon3 = mapGeneralIcon;
				mapGeneralIcon3.OnStartedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapGeneralIcon3.OnStartedHovered, new UnityAction<Map3DIcon>(OnMapIconStartHovered));
				MapGeneralIcon mapGeneralIcon4 = mapGeneralIcon;
				mapGeneralIcon4.OnStoppedHovered = (UnityAction<Map3DIcon>)Delegate.Combine(mapGeneralIcon4.OnStoppedHovered, new UnityAction<Map3DIcon>(OnMapIconStopHovered));
				m_activeIcons.Add(mapGeneralIcon);
				m_activeGeneralIcons.Add(mapGeneralIcon);
				if (m_focusOnLoreAfterFloorChanged != null && appearsOnMapInstance.LoreEntry == m_focusOnLoreAfterFloorChanged)
				{
					item.FocusCameraOnMapPosition(mapGeneralIcon.MapWorldPosition, ignoreClamping: false);
					m_focusOnLoreAfterFloorChanged = null;
				}
			}
		}
	}

	private void OnBoatMapIconClicked(Map3DIcon boatMapIcon)
	{
		MapGeneralIcon mapGeneralIcon = boatMapIcon as MapGeneralIcon;
		if (mapGeneralIcon != null)
		{
			GlobalReferences.Instance.EventChannels.LevelTransition.BoatLevelSelected.Raise(mapGeneralIcon.ParentLevel);
		}
	}

	private void OnLoreMarkerClicked(Map3DIcon icon)
	{
		OnMapIconClicked?.Invoke(icon);
	}

	private void OnPhotoClicked(Map3DIcon icon)
	{
		if (!m_markerOptionsPanel.IsActive && !(m_iconActionsPanel.ActiveIcon == icon))
		{
			m_markerOptionsPanel.Close();
			MapPhoto mapPhoto = icon as MapPhoto;
			m_photoViewer.ShowPhoto(mapPhoto.PhotoData);
			m_highlightInfoPanel.ShowButtonPrompts(show: false);
			OnMapIconClicked?.Invoke(icon);
		}
	}

	private void OnMapMarkerClicked(Map3DIcon icon)
	{
		if (!(Time.realtimeSinceStartup < m_blockSelectMapMarkerTime) && !m_markerOptionsPanel.IsActive && !(m_iconActionsPanel.ActiveIcon == icon))
		{
			m_photoViewer.Close();
			m_highlightInfoPanel.ShowButtonPrompts(show: false);
			m_iconActionsPanel.Show(icon);
			OnMapIconClicked?.Invoke(icon);
		}
	}

	private void ShowMapMarkerEditPanel(MapUserMarkerIcon icon)
	{
		m_iconActionsPanel.Hide();
		m_markerOptionsPanel.ShowMarkerOptions(icon);
	}

	private void SetHighlightInfoPanelIcon(Map3DIcon mapIcon)
	{
		m_highlightInfoPanel.SetMap3DIcon(mapIcon);
		UpdateHighlightInfoPanel();
	}

	private void UpdateHighlightInfoPanel()
	{
		Map3DIcon map3DIcon = m_hoveredMapIcon;
		if (m_iconActionsPanel != null && m_iconActionsPanel.ActiveIcon != null)
		{
			map3DIcon = m_iconActionsPanel.ActiveIcon;
		}
		if (map3DIcon != null)
		{
			m_highlightInfoPanel.SetMap3DIcon(map3DIcon);
			if (m_iconActionsPanel.isActiveAndEnabled)
			{
				m_highlightInfoPanel.PositionAroundMapIcon(map3DIcon);
			}
			else
			{
				m_highlightInfoPanel.PositionInNormalUI();
			}
			return;
		}
		IMapRaycastTooltipElement raycastTooltipElement = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.GetRaycastTooltipElement();
		if (raycastTooltipElement != null)
		{
			m_highlightInfoPanel.SetMapRaycastTooltipElement(raycastTooltipElement);
			m_highlightInfoPanel.PositionInNormalUI();
		}
		else
		{
			m_highlightInfoPanel.Clear();
		}
	}

	private void OnMapIconStartHovered(Map3DIcon icon)
	{
		if (!GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.IsAnimating && !m_markerOptionsPanel.IsActive && !m_iconActionsPanel.ActiveIcon)
		{
			m_hoveredMapIcon = icon;
			SetHighlightInfoPanelIcon(m_hoveredMapIcon);
			OnMapIconHovered?.Invoke(icon);
			GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
		}
	}

	private void OnMapIconStopHovered(Map3DIcon icon)
	{
		if (m_hoveredMapIcon == icon)
		{
			m_hoveredMapIcon = null;
			OnMapIconHovered?.Invoke(null);
			GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
		}
	}

	private bool IsSubPanelOpen()
	{
		if (m_photoViewer.IsActive || m_markerOptionsPanel.IsActive)
		{
			return true;
		}
		return false;
	}

	public bool CanAddMarker()
	{
		return false;
	}

	public bool CanSelect()
	{
		if (!IsSubPanelOpen())
		{
			if (m_hoveredMapIcon != null)
			{
				if (!(m_hoveredMapIcon is MapUserMarkerIcon))
				{
					return m_hoveredMapIcon is MapPhoto;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public MapLoreEntryIcon GetMapLoreEntryIconForLoreEntry(LoreEntry loreEntry)
	{
		foreach (MapGeneralIcon activeGeneralIcon in m_activeGeneralIcons)
		{
			MapLoreEntryIcon mapLoreEntryIcon = activeGeneralIcon as MapLoreEntryIcon;
			if (mapLoreEntryIcon != null && mapLoreEntryIcon.LoreEntry == loreEntry)
			{
				return mapLoreEntryIcon;
			}
		}
		return null;
	}

	public MapPhoto GetMapPhotoIconForPhotoData(PhotoData photo)
	{
		foreach (MapPhoto activePhoto in m_activePhotos)
		{
			if (activePhoto != null && activePhoto.PhotoData == photo)
			{
				return activePhoto;
			}
		}
		return null;
	}

	private void FocusOnLoreIcon(LoreEntry loreEntry, bool searchOtherMapViews = true)
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		MapLoreEntryIcon mapLoreEntryIconForLoreEntry = GetMapLoreEntryIconForLoreEntry(loreEntry);
		if (mapLoreEntryIconForLoreEntry == null && item.Floors.Count > 0 && item.SwitchToFloorForLore(loreEntry))
		{
			mapLoreEntryIconForLoreEntry = GetMapLoreEntryIconForLoreEntry(loreEntry);
			m_focusOnLoreAfterFloorChanged = loreEntry;
		}
		if (mapLoreEntryIconForLoreEntry != null)
		{
			StartCoroutine(item.FocusCameraOnMapPositionAnimated(mapLoreEntryIconForLoreEntry.MapWorldPosition, 500f));
		}
		else
		{
			if (!searchOtherMapViews)
			{
				return;
			}
			if (loreEntry.AssociatedRegion != null)
			{
				MapViewMetadata[] mapviewMetadatas = GlobalReferences.Instance.MapviewMetadatas;
				foreach (MapViewMetadata mapViewMetadata in mapviewMetadatas)
				{
					if (mapViewMetadata.EncompassesRegion(loreEntry.AssociatedRegion))
					{
						StartCoroutine(SwitchToMapViewAndFocusOnLoreIcon(mapViewMetadata, loreEntry));
						break;
					}
				}
			}
			else
			{
				StartCoroutine(SwitchToMapViewAndFocusOnLoreIcon(null, loreEntry));
			}
		}
	}

	private void FocusOnPhoto(PhotoData photo)
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		MapPhoto mapPhotoIconForPhotoData = GetMapPhotoIconForPhotoData(photo);
		if (mapPhotoIconForPhotoData == null && item.Floors.Count > 0 && item.SwitchToFloorForMapArea(photo.MapAreaMetadata))
		{
			mapPhotoIconForPhotoData = GetMapPhotoIconForPhotoData(photo);
			m_focusOnPhotoAfterFloorChanged = photo;
		}
		if (mapPhotoIconForPhotoData != null)
		{
			Vector3 mapWorldPosition = mapPhotoIconForPhotoData.MapWorldPosition;
			StartCoroutine(item.FocusCameraOnMapPositionAnimated(mapWorldPosition, 200f));
		}
	}

	private void DeletePhoto(PhotoData photo)
	{
		MapPhoto mapPhotoIconForPhotoData = GetMapPhotoIconForPhotoData(photo);
		m_userMapData.DeletePhoto(photo);
		m_activeIcons.Remove(mapPhotoIconForPhotoData);
		m_activePhotos.Remove(mapPhotoIconForPhotoData);
		OnMapIconStopHovered(m_iconActionsPanel.ActiveIcon);
		UnityEngine.Object.Destroy(mapPhotoIconForPhotoData.gameObject);
	}

	private void OnMapChangeComplete()
	{
		m_waitingForMapChange = false;
	}

	private IEnumerator SwitchToMapViewAndFocusOnLoreIcon(MapViewMetadata mapViewMetadata, LoreEntry loreEntry)
	{
		m_waitingForMapChange = true;
		GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Raise(mapViewMetadata);
		GlobalReferences.Instance.EventChannels.Map.MapSceneLoaded.Register(OnMapChangeComplete);
		yield return new WaitUntil(() => m_waitingForMapChange);
		GlobalReferences.Instance.EventChannels.Map.MapSceneLoaded.Unregister(OnMapChangeComplete);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitUntil(() => GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item != null);
		FocusOnLoreIcon(loreEntry, searchOtherMapViews: false);
	}

	private void GenerateLoreLines()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		item.MapLinesManager.ClearLines();
		Dictionary<LoreEntry, MapLoreEntryIcon> dictionary = new Dictionary<LoreEntry, MapLoreEntryIcon>();
		foreach (MapGeneralIcon activeGeneralIcon in m_activeGeneralIcons)
		{
			MapLoreEntryIcon mapLoreEntryIcon = activeGeneralIcon as MapLoreEntryIcon;
			if (mapLoreEntryIcon != null)
			{
				dictionary.Add(mapLoreEntryIcon.LoreEntry, mapLoreEntryIcon);
			}
		}
		foreach (MapGeneralIcon activeGeneralIcon2 in m_activeGeneralIcons)
		{
			MapLoreEntryIcon mapLoreEntryIcon2 = activeGeneralIcon2 as MapLoreEntryIcon;
			if (!(mapLoreEntryIcon2 != null) || mapLoreEntryIcon2.LoreEntry.ConnectedLore == null)
			{
				continue;
			}
			LoreEntry[] connectedLore = mapLoreEntryIcon2.LoreEntry.ConnectedLore;
			foreach (LoreEntry loreEntry in connectedLore)
			{
				if (!(loreEntry == null) && dictionary.ContainsKey(loreEntry))
				{
					item.MapLinesManager.AddLine(mapLoreEntryIcon2, dictionary[loreEntry]);
				}
			}
		}
	}

	public bool IsOnTopLevelMap()
	{
		if (!(GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item == null))
		{
			return GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item.Metadata == null;
		}
		return true;
	}

	private void CloseIconActionsPanel()
	{
		if (m_iconActionsPanel.ActiveIcon != null)
		{
			m_iconActionsPanel.ActiveIcon.BlockClick();
		}
		m_iconActionsPanel.Hide();
		m_highlightInfoPanel.ShowButtonPrompts(show: true);
	}

	private void EditIconClicked()
	{
		if (m_iconActionsPanel.ActiveIcon is MapUserMarkerIcon)
		{
			ShowMapMarkerEditPanel(m_iconActionsPanel.ActiveIcon as MapUserMarkerIcon);
		}
	}

	private void ActionPanelClosedCallback()
	{
		m_highlightInfoPanel.ShowButtonPrompts(show: true);
	}

	private void DeleteIconClicked()
	{
		if (m_iconActionsPanel.ActiveIcon is MapUserMarkerIcon mapUserMarkerIcon)
		{
			if (mapUserMarkerIcon.ParentGroup != null)
			{
				mapUserMarkerIcon.ParentGroup.RemoveIcon(mapUserMarkerIcon);
			}
			m_userMapData.DeleteUserMarker(mapUserMarkerIcon.MarkerData);
			m_activeIcons.Remove(mapUserMarkerIcon);
			m_activeUserMarkers.Remove(mapUserMarkerIcon);
			OnMapIconStopHovered(m_iconActionsPanel.ActiveIcon);
			UnityEngine.Object.Destroy(mapUserMarkerIcon.gameObject);
			CloseIconActionsPanel();
		}
		else if (m_iconActionsPanel.ActiveIcon is MapPhoto mapPhoto)
		{
			m_userMapData.DeletePhoto(mapPhoto.PhotoData);
			m_activeIcons.Remove(mapPhoto);
			m_activePhotos.Remove(mapPhoto);
			OnMapIconStopHovered(m_iconActionsPanel.ActiveIcon);
			UnityEngine.Object.Destroy(mapPhoto.gameObject);
			CloseIconActionsPanel();
		}
	}

	private void ViewIconClicked()
	{
		if (m_iconActionsPanel.ActiveIcon is MapPhoto)
		{
			CloseIconActionsPanel();
			m_photoViewer.Show();
		}
	}

	public bool OnBackInput()
	{
		if (m_mapViewActive)
		{
			Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
			if (item != null && item.IsAnimating)
			{
				return true;
			}
		}
		if (m_iconActionsPanel != null && (bool)m_iconActionsPanel.ActiveIcon)
		{
			CloseIconActionsPanel();
			return true;
		}
		if (!IsOnTopLevelMap())
		{
			GlobalReferences.Instance.EventChannels.Map.SetActiveMapView.Raise(null);
			return true;
		}
		return false;
	}

	private void OnMapCycleIcon(InputAction.CallbackContext callback)
	{
		if (m_hoveredIcons != null && m_hoveredIcons.Count > 1)
		{
			m_hoverCycleIndex++;
			if (m_hoverCycleIndex >= m_hoveredIcons.Count)
			{
				m_hoverCycleIndex = 0;
			}
			m_hoveredIcons[m_hoverCycleIndex].transform.SetAsLastSibling();
			GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
			m_highlightInfoPanel.SetGroupInfo(m_hoveredIcons.Count, m_hoverCycleIndex);
		}
	}

	public bool HasPhotos()
	{
		return m_userMapData.Photos.Count > 0;
	}

	public bool CanDeletePhoto()
	{
		return m_photoViewer.IsActive;
	}

	private void OnViewPhotos(InputAction.CallbackContext callback)
	{
		if (HasPhotos())
		{
			m_photoViewer.Toggle();
		}
	}

	public Vector2 GetGamepadCursorPosition()
	{
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (item != null)
		{
			return item.GetGamepadCursorScreenPosition();
		}
		return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
	}
}
