using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GlobalReferences : ScriptableObject
{
	[Serializable]
	public class AnchorsReferences
	{
		[Serializable]
		public class AnchorsAchievements
		{
			public AchievementManagerAnchor AchievementManagerAnchor;
		}

		[Serializable]
		public class AnchorsGeneric
		{
			public LevelMetadataAnchor ActiveLevelMetadata;

			public GameObjectAnchor AudioListenerAnchor;

			public ExaminableAnchor ExaminableAnchor;

			public GameInputManagerAnchor GameInputManagerAnchor;

			public InteractableAnchor NearbyInteractableAnchor;

			public PathNodeGraphAnchor PathNodeGraphAnchor;
		}

		[Serializable]
		public class AnchorsCamera
		{
			public CameraBoundsAnchor ActiveCameraBoundsAnchor;

			public CinemachineCameraBlendUtilsAnchor CinemachineCameraBlendAnchor;

			public CameraAnchor GameUICameraAnchor;

			public CameraAnchor InGameMenuCameraAnchor;
		}

		[Serializable]
		public class AnchorsDLC
		{
			public DLCManagerAnchor DLCManagerAnchor;
		}

		[Serializable]
		public class AnchorsFieldOfView
		{
			public FieldOfViewAnchor FieldOfViewAnchor;

			public FieldOfViewLevelSettingsAnchor FieldOfViewLevelSettingsAnchor;
		}

		[Serializable]
		public class AnchorsGameplay
		{
			public GameObjectAnchor CatAnchor;

			public CircuitManagerAnchor CircuitManagerAnchor;

			public GameDifficultyManagerAnchor GameDifficultyManagerAnchor;

			public GameObjectAnchor PlayerAnchor;
		}

		[Serializable]
		public class AnchorsHints
		{
			public HintInfoAnchor ActiveAreaHintInfoAnchor;
		}

		[Serializable]
		public class AnchorsInput
		{
			public ICursorOverrideAnchor CursorOverridesAnchor;
		}

		[Serializable]
		public class AnchorsInventory
		{
			public ApplyItemAnchor ApplyItemInteractableAnchor;

			public InteractableAnchor ItemBoxInteractableAnchor;

			public ItemPickupAnchor ItemPickupInteractAnchor;

			public ItemInstanceAnchor QueuedUseItemInstanceAnchor;
		}

		[Serializable]
		public class AnchorsLore
		{
			public AudioLogPlayerAnchor AudioLogPlayerAnchor;
		}

		[Serializable]
		public class AnchorsMap
		{
			public Map3DAnchor ActiveMap3DView;

			public UIMap3DPanelAnchor UIMap3DPanelAnchor;
		}

		[Serializable]
		public class AnchorsMigration
		{
			public EnemyMigrationManagerAnchor EnemyMigrationManager;
		}

		[Serializable]
		public class AnchorsMinigame
		{
			public GameObjectAnchor ActiveMinigameInteractableAnchor;

			public MinigameMetadataAnchor ActiveMinigameMetadataAnchor;
		}

		[Serializable]
		public class AnchorsSaveData
		{
			public SaveDataManagerAnchor SaveDataManagerAnchor;
		}

		public AnchorsAchievements Achievements;

		public AnchorsGeneric Generic;

		public AnchorsCamera Camera;

		public AnchorsDLC DLC;

		public AnchorsFieldOfView FieldOfView;

		public AnchorsGameplay Gameplay;

		public AnchorsHints Hints;

		public AnchorsInput Input;

		public AnchorsInventory Inventory;

		public AnchorsLore Lore;

		public AnchorsMap Map;

		public AnchorsMigration Migration;

		public AnchorsMinigame Minigame;

		public AnchorsSaveData SaveData;
	}

	[Serializable]
	public class ArtifactEffectsReferences
	{
		[Serializable]
		public class ArtifactEffectsGeneric
		{
			public ArtifactEffectDefinition BetterHeadshots;

			public ArtifactEffectDefinition BetterWeaponHandling;

			public ArtifactEffectDefinition DamageReduction;

			public ArtifactEffectDefinition DamageTakenIncrease;

			public ArtifactEffectDefinition EasierReload;

			public ArtifactEffectDefinition EnableDodgeRoll;

			public ArtifactEffectDefinition FasterHealItemUseAnimation;

			public ArtifactEffectDefinition FasterHerbHeal;

			public ArtifactEffectDefinition FasterMelee;

			public ArtifactEffectDefinition FasterMeleeLowHP;

			public ArtifactEffectDefinition FasterReload;

			public ArtifactEffectDefinition FasterSprint;

			public ArtifactEffectDefinition FasterStaminaRecovery;

			public ArtifactEffectDefinition ImprovedMeleeComboFinalHit;

			public ArtifactEffectDefinition ImprovedRunningMelee;

			public ArtifactEffectDefinition IncreasedDamage;

			public ArtifactEffectDefinition IncreasedMeleeDamage;

			public ArtifactEffectDefinition IncreasedMeleeDamageChargeAttacks;

			public ArtifactEffectDefinition IncreasedMeleeDamageLowHP;

			public ArtifactEffectDefinition IncreasedMeleeDurabilityLossOnAttacks;

			public ArtifactEffectDefinition IncreasedRangedWeaponDamage;

			public ArtifactEffectDefinition IncreasedRecoil;

			public ArtifactEffectDefinition IncreasedSlashingWeaponDamage;

			public ArtifactEffectDefinition IncreasedStaggerFromMelee;

			public ArtifactEffectDefinition IncreasedStompDamage;

			public ArtifactEffectDefinition IncreaseMaxHealth;

			public ArtifactEffectDefinition IncreaseMaxStamina;

			public ArtifactEffectDefinition InfiniteAmmo;

			public ArtifactEffectDefinition LargerParryWindow;

			public ArtifactEffectDefinition LastShotMagazineBonusDamage;

			public ArtifactEffectDefinition NearbyEnemyWarning;

			public ArtifactEffectDefinition RateOfFire;

			public ArtifactEffectDefinition ReduceBlockingMeleeDurabilityLoss;

			public ArtifactEffectDefinition ReducedBlockStaminaCost;

			public ArtifactEffectDefinition ReducedDodgeStaminaCost;

			public ArtifactEffectDefinition ReducedMeleeChargeTime;

			public ArtifactEffectDefinition ReducedMeleeStaminaCost;

			public ArtifactEffectDefinition ReducedRecoil;

			public ArtifactEffectDefinition ReducedSprintStaminaCost;

			public ArtifactEffectDefinition ReducedSwimmingStaminaCost;

			public ArtifactEffectDefinition ReducedWeaponSway;

			public ArtifactEffectDefinition ReduceMeleeDurabilityLossOnAttacks;

			public ArtifactEffectDefinition ShowPlayerHealthBar;

			public ArtifactEffectDefinition StaminaRecoveryDelay;

			public ArtifactEffectDefinition WeaponPenetrationChance;
		}

		public ArtifactEffectsGeneric Generic;
	}

	[Serializable]
	public class MaterialsReferences
	{
		public Material SpriteDefault;
	}

	[Serializable]
	public class EventChannelsReferences
	{
		[Serializable]
		public class EventChannelsGeneric
		{
			public BoolGameEventChannel AllowInteractionPrompt;

			public BossIntroductionGameEventChannel BossIntroduction;

			public CameraShakeEventChannel CameraShake;

			public VoidGameEventChannel ColdStartupSetupScene;

			public ControllerRumbleEventChannel ControllerRumble;

			public VoidGameEventChannel EndOfDemo;

			public BoolGameEventChannel FadeScreen;

			public TimeSlowEventChannel GameSleep;

			public BoolGameEventChannel Hiding;

			public BoolGameEventChannel InteractionEnabled;

			public MeleeWeaponDurabilityChangedEventChannel MeleeWeaponDurabilityChanged;

			public MeleeWeaponDurabilityChangedEventChannel MeleeWeaponRefresh;

			public FloatGameEventChannel NearbyEnemyPresenceChanged;

			public PlayerInputOverrideEventChannel PlayerInputOverride;

			public FadeTypeEventChannel ScreenFadeOfType;

			public BoolGameEventChannel SetCutsceneIsPlaying;

			public FloatGameEventChannel SetDebugTimeScaleChannel;

			public BoolGameEventChannel SetGamepadCursorAllowed;

			public PlayAudioVoicedEventGameEventChannel ShowSubtitleVoiceLine;

			public VoidGameEventChannel TimeOfDaySceneLightingChanged;

			public TryApplyItemEventChannel TryApplyItem;

			public BoolGameEventChannel TryApplyItemSuccessFail;

			public ActiveUseStateGameEventChannel UseItemActiveInfo;

			public WeaponInfoGameEventChannel WeaponInfo;

			public ActiveUseStateGameEventChannel WeaponReloadInfo;
		}

		[Serializable]
		public class EventChannelsAnimatedButtonSequence
		{
			public InteractableABSEventChannel ActiveAnimatedButtonSequenceChanged;

			public VoidGameEventChannel AnimatedButtonSequenceSuccess;
		}

		[Serializable]
		public class EventChannelsAudio
		{
			public VoidGameEventChannel OnAudioVoicedEventFinished;

			public BoolGameEventChannel PauseGameAudioBusses;

			public BoolGameEventChannel PauseMusic;

			public PlayAudioEventGameEventChannel PlayAudio;

			public PlayAudioVoicedEventGameEventChannel PlayAudioVoiced;

			public MusicSettingsGameEventChannel PlayMusicOverride;

			public SubtitlesEventChannel ShowVoiceSubtitles;

			public VoidGameEventChannel StopAllVoicedEvents;

			public VoidGameEventChannel TriggerMainMenuMusic;

			public VoidGameEventChannel TriggerMainMenuMusicTransition;

			public VoidGameEventChannel UpdateAudioVolumeLevelsFromUserPreferences;
		}

		[Serializable]
		public class EventChannelsCamera
		{
			public VoidGameEventChannel CameraFollowDataInitialised;

			public VoidGameEventChannel ForceCameraReset;

			public BoolGameEventChannel LockActiveCameraBounds;

			public CameraEventChannel NewCamera;

			public BoolGameEventChannel PauseCameraFollow;

			public VoidGameEventChannel ResetCameraLookahead;

			public BoolGameEventChannel SetMainCameraEnabled;
		}

		[Serializable]
		public class EventChannelsCollectables
		{
			public CollectableDefinitionEventChannel DeliverCollectableDefinition;

			public VoidGameEventChannel OnPickupFirstCollectable;

			public CollectableDefinitionEventChannel PickupCollectableDefinition;
		}

		[Serializable]
		public class EventChannelsComic
		{
			public ComicMetadataGameEventChannel PlayComic;

			public ComicMetadataGameEventChannel PlayComicFromMenu;

			public BoolGameEventChannel ToggleComicViewActive;
		}

		[Serializable]
		public class EventChannelsDatastore
		{
			public VoidGameEventChannel PersistentDataRefresh;
		}

		[Serializable]
		public class EventChannelsDialogue
		{
			public BoolGameEventChannel DialogueEnterExit;

			public DialogueEventChannel Dialogue;

			public VoidGameEventChannel ProgressDialogue;

			public VoidGameEventChannel SkipDialogue;

			public TriggerDialogueEventChannel TriggerDialogue;
		}

		[Serializable]
		public class EventChannelsDynamicallySpawnedObjects
		{
			public DynamicallySpawnObjectGameEventChannel Spawn;
		}

		[Serializable]
		public class EventChannelsExamine
		{
			public VoidGameEventChannel CancelExamining;

			public VoidGameEventChannel CancelMinigameExamining;

			public BoolGameEventChannel DoneExamining;

			public BoolGameEventChannel DoneMinigameExamining;

			public VoidGameEventChannel ShowExaminable;

			public VoidGameEventChannel ShowMinigameExaminable;
		}

		[Serializable]
		public class EventChannelsFire
		{
			public SpawnFireGameEventChannel SpawnFire;
		}

		[Serializable]
		public class EventChannelsGameCutscene
		{
			public BoolGameEventChannel SetGameCutsceneEnabled;
		}

		[Serializable]
		public class EventChannelsGameplay
		{
			public VoidGameEventChannel CoffeeConsumed;

			public VoidGameEventChannel InsufficientStamina;

			public BoolGameEventChannel PhoneDialogue;

			public PursuerSpawnTriggerEventChannel PursuerSpawnTrigger;

			public BoolGameEventChannel ShowAimingUI;

			public VoidGameEventChannel SuddenEnemyAppeared;
		}

		[Serializable]
		public class EventChannelsHints
		{
			public HintInfoGameEventChannel CancelHintInfo;

			public DynamicHintGameEventChannel DynamicHint;

			public HintInfoGameEventChannel ShowHintInfo;

			public MenuHintInfoGameEventChannel ShowMenuHintInfo;

			public HintInfoGameEventChannel ShowMinigameHintInfo;

			public PauseHintInfoGameEventChannel ShowPauseHintInfo;
		}

		[Serializable]
		public class EventChannelsInGameMenu
		{
			public MenuPageEventChannel InGameMenuRequestPage;

			public MenuInfoMessageEventChannel MenuInfoMessage;

			public VoidGameEventChannel ShowArtifactMenuTab;

			public VoidGameEventChannel ShowInventoryMenuTab;

			public VoidGameEventChannel ShowMapMenuTab;

			public VoidGameEventChannel ShowOptionsMenuTab;
		}

		[Serializable]
		public class EventChannelsInput
		{
			public BindableInputActionEntryGameEventChannel OnRequestRebindGamepadInputAction;

			public BindableInputActionEntryGameEventChannel OnRequestRebindKeyboardAltInputAction;

			public BindableInputActionEntryGameEventChannel OnRequestRebindKeyboardInputAction;

			public BindableInputActionEntryGameEventChannel OnRequestResetInputAction;

			public VoidGameEventChannel RefreshMouseCursorState;
		}

		[Serializable]
		public class EventChannelsInventory
		{
			public AddItemGameEventChannel AddArtifactItem;

			public AddItemGameEventChannel AddItem;

			public IntGameEventChannel AddPlayerMoney;

			public BoolGameEventChannel AllowComplexInventoryActionsChanged;

			public ItemInstanceGameEventChannel ApplyItemToInteractable;

			public ItemInstanceGameEventChannel ArtifactItemCollected;

			public ItemInstanceGameEventChannel BatteryChargeLevelNotifyItem;

			public ItemInstanceGameEventChannel BatteryDepletedItem;

			public ItemInstanceGameEventChannel BatteryLowItem;

			public ItemEquippedGameEventChannel EquippedArtifactsChanged;

			public ItemEquippedGameEventChannel EquippedMeleeItemChanged;

			public ItemEquippedGameEventChannel EquippedRangedItemChanged;

			public ItemEquippedGameEventChannel EquippedSecondaryItemChanged;

			public ItemInstanceGameEventChannel ExamineItemInstance;

			public ItemInstanceGameEventChannel ItemActivatedChanged;

			public ItemInstanceGameEventChannel ItemInstanceDefinitionChanged;

			public AddItemGameEventChannel ItemPickedUp;

			public AddItemGameEventChannel ItemPurchased;

			public ItemInstanceGameEventChannel ItemShortcutUsed;

			public ItemInstanceGameEventChannel ItemStackAmountChanged;

			public ItemInstanceGameEventChannel KeyAutoAddedToKeyRing;

			public ItemInstanceGameEventChannel MeleeWeaponItemBroken;

			public ItemInstanceGameEventChannel MeleeWeaponItemDurabilityLow;

			public ItemInstanceGameEventChannel MeleeWeaponItemRepaired;

			public ItemInstanceGameEventChannel NewItemCombined;

			public VoidGameEventChannel OnFirstArtifactPickedUp;

			public ItemInstanceGameEventChannel OnItemRemoved;

			public IntGameEventChannel OnPlayerMoneyUpdated;

			public ItemInstanceGameEventChannel OnPowerUpArtifactItem;

			public VoidGameEventChannel PerformSelectOnItemWheel;

			public ItemInstanceGameEventChannel QuickUseOnPickupItem;

			public VoidGameEventChannel RequestCharacterReloadWeapon;

			public ItemInstanceGameEventChannel RequestDropItemInstance;

			public ItemInstanceGameEventChannel RequestEquipItem;

			public TransferItemInstanceGameEventChannel RequestItemInstanceTranfer;

			public RemoveItemInstanceGameEventChannel RequestRemoveItem;

			public ItemInstanceGameEventChannel SelectedFromItemWheel;

			public ItemDefinitionGameEventChannel SelectedItemFromQuickItemPanel;

			public IntGameEventChannel SetMaxNumberOfActiveArtifacts;

			public BoolGameEventChannel ShowApplyItemInteract;

			public BoolGameEventChannel ShowItemWheel;

			public ItemInstanceGameEventChannel ShowKeyring;

			public BoolGameEventChannel ShowQuickItemPanel;

			public VoidGameEventChannel UnlockNewArtifactSlot;

			public ItemDefinitionGameEventChannel UpgradeInventorySize;

			public ItemInstanceGameEventChannel UseItem;

			public VoidGameEventChannel WeaponCycled;
		}

		[Serializable]
		public class EventChannelsLevelTransition
		{
			public LevelMetadataGameEventChannel BoatLevelSelected;

			public VoidGameEventChannel EnableLevelTransitionTriggers;

			public VoidGameEventChannel LevelTransitionCompleted;

			public LevelTransitionGameEventChannel LevelTransition;

			public VoidGameEventChannel LevelTransitionStarted;

			public VoidGameEventChannel LevelUnloaded;

			public GameObjectGameEventChannel NearbyLevelTransitionObject;

			public LevelMetadataGameEventChannel SetupLevel;

			public BoolGameEventChannel ShowLoadingScreen;

			public BoolGameEventChannel TempDisableLevelFadeIn;

			public BoolGameEventChannel TempDisableLevelTransitionText;

			public VoidGameEventChannel TriggerStartGameInScene;
		}

		[Serializable]
		public class EventChannelsLore
		{
			public BoolGameEventChannel AudioLogPlaybackStatusChanged;

			public LoreEntryEventChannel LoreAddedToLoreInventory;

			public LoreEntryEventChannel LorePickup;

			public LoreEntryEventChannel LorePickupNoShow;

			public LoreEntryEventChannel PlayLoreEntryAsAudioLog;

			public LoreEntryEventChannel ShowLoreEntryInNotesPage;

			public LoreEntryEventChannel ShowLoreEntryOnMapPage;

			public VoidGameEventChannel StopPlayingAudioLog;
		}

		[Serializable]
		public class EventChannelsMainMenu
		{
			public NewGameStartConfigurationEventChannel ChapterSelect;

			public MenuPageEventChannel MainMenuRequestPage;
		}

		[Serializable]
		public class EventChannelsMap
		{
			public Map3DFloorEventChannel ActiveMapFloorChanged;

			public AppearsOnMapGameEventChannel DiscoveredAppearsOnMap;

			public Map3DFloorEventChannel InitialFloorMapApplied;

			public StringGameEventChannel MapCompletionItemDone;

			public VoidGameEventChannel MapSceneLoaded;

			public VoidGameEventChannel OnMapCameraRendered;

			public MapViewMetadataEventChannel SetActiveMapView;

			public BoolGameEventChannel SetMapActive;

			public BoolGameEventChannel ShowQuickMap;
		}

		[Serializable]
		public class EventChannelsMigration
		{
			public MigrationRequestEventChannel MigrateEnemyRequest;
		}

		[Serializable]
		public class EventChannelsMinigames
		{
			public GameObjectGameEventChannel MinigameHighlightInteract;
		}

		[Serializable]
		public class EventChannelsPhone
		{
			public PhoneNumberGameEventChannel PhoneNumberAdded;
		}

		[Serializable]
		public class EventChannelsPhotos
		{
			public PhotoDataGameEventChannel NewPhotoAdded;

			public RenderTextureGameEventChannel PhotoRenderTextureUpdated;

			public BoolGameEventChannel PrepareForPhoto;

			public VoidGameEventChannel TakePhoto;

			public VoidGameEventChannel TooManyPhotosMemory;
		}

		[Serializable]
		public class EventChannelsPlayer
		{
			public VoidGameEventChannel PlayerDead;

			public GameObjectGameEventChannel PlayerSpawned;

			public VoidGameEventChannel PlayerTakeDamage;

			public BoolGameEventChannel TogglePlayerInput;
		}

		[Serializable]
		public class EventChannelsSaveLoad
		{
			public IntGameEventChannel DeleteProfile;

			public IntGameEventChannel GameLoad;

			public VoidGameEventChannel GameSave;

			public IntGameEventChannel GameSaveInSlot;

			public VoidGameEventChannel LoadMostRecentSave;

			public VoidGameEventChannel OnGameReloadedEvent;

			public PersistentDataEventChannel PersistentDataOnLoaded;

			public PersistentDataEventChannel PersistentDataPopulateForSave;

			public VoidGameEventChannel PrepareSave;

			public VoidGameEventChannel SaveCompletedFailed;

			public VoidGameEventChannel SaveCompletedSuccesfully;

			public VoidGameEventChannel ShowSaveGamePanel;
		}

		[Serializable]
		public class EventChannelsStamina
		{
			public MeleeChargeStateGameEventChannel MeleeChargeState;

			public VoidGameEventChannel MeleeChargeStateUpgrade;

			public StaminaInfoGameEventChannel StaminaInfo;
		}

		[Serializable]
		public class EventChannelsStatusEffects
		{
			public StatusEffectInstanceGameEventChannel PlayerStatusEffectUpdated;
		}

		[Serializable]
		public class EventChannelsUserPreferences
		{
			public VoidGameEventChannel GraphicsQualityChanged;

			public VoidGameEventChannel SaveUserPreferences;

			public FloatGameEventChannel SetTemporaryGamma;

			public VoidGameEventChannel UIScaleChanged;

			public VoidGameEventChannel UpdateLocalization;
		}

		[Serializable]
		public class EventChannelsVideoCutscenes
		{
			public BoolGameEventChannel PlayingVideoCutsceneChanged;

			public VideoMetadataGameEventChannel PlayVideoCutscene;
		}

		public EventChannelsGeneric Generic;

		public EventChannelsAnimatedButtonSequence AnimatedButtonSequence;

		public EventChannelsAudio Audio;

		public EventChannelsCamera Camera;

		public EventChannelsCollectables Collectables;

		public EventChannelsComic Comic;

		public EventChannelsDatastore Datastore;

		public EventChannelsDialogue Dialogue;

		public EventChannelsDynamicallySpawnedObjects DynamicallySpawnedObjects;

		public EventChannelsExamine Examine;

		public EventChannelsFire Fire;

		public EventChannelsGameCutscene GameCutscene;

		public EventChannelsGameplay Gameplay;

		public EventChannelsHints Hints;

		public EventChannelsInGameMenu InGameMenu;

		public EventChannelsInput Input;

		public EventChannelsInventory Inventory;

		public EventChannelsLevelTransition LevelTransition;

		public EventChannelsLore Lore;

		public EventChannelsMainMenu MainMenu;

		public EventChannelsMap Map;

		public EventChannelsMigration Migration;

		public EventChannelsMinigames Minigames;

		public EventChannelsPhone Phone;

		public EventChannelsPhotos Photos;

		public EventChannelsPlayer Player;

		public EventChannelsSaveLoad SaveLoad;

		public EventChannelsStamina Stamina;

		public EventChannelsStatusEffects StatusEffects;

		public EventChannelsUserPreferences UserPreferences;

		public EventChannelsVideoCutscenes VideoCutscenes;
	}

	[Serializable]
	public class SetsReferences
	{
		[Serializable]
		public class SetsGeneric
		{
			public AISensesSet ActiveAISensesSet;

			public AIHearingSet AIHearingSet;

			public AimTargetSet AimTargetsSet;

			public AttachPointSet AttachPointsSet;

			public CharacterBirdMovementSet BirdMovementSet;

			public DigSpotSet DigSpotsSet;

			public DropPlatformSet DropPlatformSet;

			public EnemyPresenceSet EnemyPresenceSet;

			public FootstepEffectAreaSet FootstepEffectAreaSet;

			public GameplayFogBoundsSet GameplayFogSet;

			public GameplayWaterBoundsSet GameplayWaterSet;

			public LedgeGrabLevelTransitionSet LedgeGrabLevelTransitionSet;

			public StairsSet StairsSet;

			public VisibilityLightSourceSet VisibilityLightSourcesSet;
		}

		[Serializable]
		public class SetsCamera
		{
			public ActiveCameraBoundsExtenderSet ActiveCameraBoundsExtenderSet;

			public ActiveCameraBoundsSet ActiveCameraBoundsSet;
		}

		[Serializable]
		public class SetsFieldOfView
		{
			public FieldOfViewEffectorAreaSet FieldOfViewAreaSet;

			public ObjectVisibilitySet ObjectVisibilitySet;
		}

		[Serializable]
		public class SetsInput
		{
			public CursorInteractHighlightSet CursorInteractHighlightSet;

			public RestrictPlayerActionsSet RestrictPlayerActionsSet;
		}

		public SetsGeneric Generic;

		public SetsCamera Camera;

		public SetsFieldOfView FieldOfView;

		public SetsInput Input;
	}

	[Serializable]
	public class StatusEffectsReferences
	{
		[Serializable]
		public class StatusEffectsGeneric
		{
			public StatusEffectDefinition Bleeding;

			public StatusEffectDefinition Blind;

			public StatusEffectDefinition Burning;

			public StatusEffectDefinition DeepWater;

			public StatusEffectDefinition Entangled;

			public StatusEffectDefinition Frenzy;

			public StatusEffectDefinition GreenHerbRegeneration;

			public StatusEffectDefinition Knockdown;

			public StatusEffectDefinition Poison;

			public StatusEffectDefinition StaminaBoost;
		}

		public StatusEffectsGeneric Generic;
	}

	[Serializable]
	public class VariablesReferences
	{
		[Serializable]
		public class VariablesLevelTransitionDoors
		{
			public ProgressionVariable PV_Door_Market_Lower_Sport_Store_To_Market_Department_Store_Exterior;

			public ProgressionVariable PV_Door_Market_Sport_Store;

			public ProgressionVariable PV_Door_Sheriff_Lobby_To_Sheriff_Reception;
		}

		[Serializable]
		public class VariablesMechanics
		{
			public ProgressionVariable PV_InventoryDisabled;

			public ProgressionVariable PV_MapDisabled;

			public ProgressionVariable PV_PlayerHasSaved;

			public ProgressionVariable PV_PursuitSystemActive;

			public ProgressionVariable PV_WorldMapMainMapRevealed;
		}

		[Serializable]
		public class VariablesNPCs
		{
			public ProgressionVariableInt PV_NPC_Boots;

			public ProgressionVariableInt PV_NPC_Cowboy;

			public ProgressionVariableInt PV_NPC_Dwarf;

			public ProgressionVariableInt PV_NPC_GalleryMan;

			public ProgressionVariableInt PV_NPC_JaneDoe;

			public ProgressionVariableInt PV_NPC_Roberts;
		}

		[Serializable]
		public class VariablesProgression
		{
			public ProgressionVariable PV_Apartment_Button_Pickup;

			public ProgressionVariable PV_Apartment_ElevatorOnBottomFloor;

			public ProgressionVariable PV_Apartment_EncounteredBasementDespair;

			public ProgressionVariable PV_Apartment_Intercom;

			public ProgressionVariable PV_ApartmentBossDefeated;

			public ProgressionVariable PV_ApartmentBuzzer4B_Enabled;

			public ProgressionVariable PV_ApartmentLaundryAmbush_Enable;

			public ProgressionVariable PV_ApartmentsSpokenToBoots;

			public ProgressionVariable PV_BoatRepaired;

			public ProgressionVariable PV_Boxcutter_PickedUp;

			public ProgressionVariable PV_Cable_Station_Dwarf_Present;

			public ProgressionVariable PV_CableCar_Called_To_Downtown;

			public ProgressionVariable PV_Church_Cemetary_Monsters_Enable;

			public ProgressionVariableInt PV_Church_Confession;

			public ProgressionVariable PV_Church_CryptShortcut_Unlocked;

			public ProgressionVariable PV_Church_Hill_Crematorium_Key_Found;

			public ProgressionVariable PV_Church_Parking_Lot_Gate_Unlocked;

			public ProgressionVariable PV_Church_Tower_Door_Unlocked;

			public ProgressionVariableInt PV_CollectedBellsCount;

			public ProgressionVariable PV_Completed_Apartment;

			public ProgressionVariable PV_Completed_CableCar_Bells;

			public ProgressionVariable PV_Completed_ChurchHill;

			public ProgressionVariable PV_Completed_Sewer;

			public ProgressionVariable PV_Completed_Sheriff;

			public ProgressionVariable PV_Completed_TownHall;

			public ProgressionVariable PV_ComputerLogin;

			public ProgressionVariable PV_Crawfish;

			public ProgressionVariable PV_Diner_Phone_Ringing;

			public ProgressionVariable PV_Diner_Phone_Save_Enabled;

			public ProgressionVariable PV_Diner_Waitress;

			public ProgressionVariable PV_DinerKey;

			public ProgressionVariable PV_DinerTakenKitchenKey;

			public ProgressionVariable PV_DoneDinerPhoneCall;

			public ProgressionVariable PV_Downtown_Community_Center_AA_Door;

			public ProgressionVariable PV_Downtown_Community_Center_Printer_Done;

			public ProgressionVariable PV_Downtown_Community_Center_Printer_Fixed;

			public ProgressionVariable PV_Downtown_Gazette_Unlocked;

			public ProgressionVariable PV_Downtown_Market_St_Alley_Door;

			public ProgressionVariable PV_Downtown_Rabbithole_Curtain_opened;

			public ProgressionVariable PV_EddieSpared;

			public ProgressionVariable PV_Ferry_BrokenDown;

			public ProgressionVariable PV_Ferry_Called_To_Outskirts;

			public ProgressionVariable PV_Flashlight;

			public ProgressionVariable PV_GasStationPower;

			public ProgressionVariable PV_GasstationUnlocked;

			public ProgressionVariable PV_HandlerCall_ApartmentDone;

			public ProgressionVariable PV_HandlerCall_ApproachStudio;

			public ProgressionVariable PV_HandlerCall_BootsDeadSewers;

			public ProgressionVariable PV_HandlerCall_CableStation_Bell;

			public ProgressionVariable PV_HandlerCall_MotelExit;

			public ProgressionVariable PV_HandlerCall_SheriffStationDone;

			public ProgressionVariable PV_HasDrowned;

			public ProgressionVariableInt PV_IntTest;

			public ProgressionVariable PV_InvestigatedCar;

			public ProgressionVariable PV_InvestigatedPoliceStation;

			public ProgressionVariable PV_Lady_01;

			public ProgressionVariable PV_LeftDiner;

			public ProgressionVariable PV_Loomis_Alley_Saveroom_Door;

			public ProgressionVariable PV_Loomis_Laundromat_Shortcut_Enabled;

			public ProgressionVariable PV_Lumberyard_Changing_Room;

			public ProgressionVariable PV_Motel_CalledWoman;

			public ProgressionVariable PV_Motel_Door_1_Unlocked;

			public ProgressionVariable PV_Motel_Door_Bungalo_2;

			public ProgressionVariable PV_Motel_Door_Save_Backdoor;

			public ProgressionVariable PV_Motel_Nightmare_Complete;

			public ProgressionVariable PV_Motel_Power_Alley;

			public ProgressionVariable PV_Motel_Power_Checkin;

			public ProgressionVariable PV_Motel_Power_Game;

			public ProgressionVariable PV_Motel_Power_Rooms;

			public ProgressionVariable PV_Motel_Power_Store;

			public ProgressionVariable PV_Motel_Tunnel_Exit_Unlocked;

			public ProgressionVariable PV_MotelCheckinDone;

			public ProgressionVariable PV_MotelGate;

			public ProgressionVariable PV_MotelGilTape;

			public ProgressionVariable PV_MotelPadlockCub;

			public ProgressionVariable PV_MotelPadlockHolloway;

			public ProgressionVariable PV_MotelPadlockSalmon;

			public ProgressionVariable PV_MotelSSBCall;

			public ProgressionVariable PV_NightTimeSwitch;

			public ProgressionVariable PV_NPC_Cowboy_Done_MessHall;

			public ProgressionVariable PV_OPCO_DoorCleared;

			public ProgressionVariableInt PV_OPCO_ElevatorFloor;

			public ProgressionVariable PV_OPCO_Power;

			public ProgressionVariable PV_Outskirts_Checkin_Key_Collected;

			public ProgressionVariable PV_Outskirts_Ferry_Gate_Unlocked;

			public ProgressionVariable PV_Outskirts_Ferry_Key_Collected;

			public ProgressionVariable PV_Outskirts_Ferry_Shortcut_Open;

			public ProgressionVariable PV_Outskirts_Gallery_Man_Left;

			public ProgressionVariable PV_Outskirts_Pharmacy_key_Collected;

			public ProgressionVariable PV_Outskirts_Shore_Beach_Furnace_Activated;

			public ProgressionVariable PV_Outskirts_Store_Monster_Triggered;

			public ProgressionVariable PV_Outskirts_Trunk_Opened;

			public ProgressionVariable PV_Outskirts_Tunnel_Boss_Active;

			public ProgressionVariable PV_Outskirts_Tunnel_Boss_Defeated;

			public ProgressionVariable PV_OutskirtsBeach_House_Unlocked;

			public ProgressionVariable PV_OutskirtsBeachToDinerGateOpen;

			public ProgressionVariable PV_OutskirtsWest_Ferry_Ticket_Office_Unlocked;

			public ProgressionVariable PV_Park_Clock;

			public ProgressionVariable PV_Park_Placed_Mask1;

			public ProgressionVariable PV_Park_Placed_Mask2;

			public ProgressionVariable PV_Park_Placed_Mask4;

			public ProgressionVariable PV_Park_Pursuer_Active;

			public ProgressionVariable PV_Park_Sewer_Mask;

			public ProgressionVariable PV_Peak_Cable_Station_Lights;

			public ProgressionVariable PV_Peak_Tunnel_Power;

			public ProgressionVariable PV_Phonecall_Dwarf_FindPeace;

			public ProgressionVariable PV_PickedUpCamera;

			public ProgressionVariable PV_Pier_Fishery_Door_Unlocked;

			public ProgressionVariable PV_Pier_Power;

			public ProgressionVariable PV_Pier_Steam;

			public ProgressionVariable PV_Pier_Warehouse_Shortcut_Unlocked;

			public ProgressionVariable PV_Police_Shortcut_Enabled;

			public ProgressionVariable PV_PondArtPuzzle_CraneTile;

			public ProgressionVariable PV_PondArtPuzzle_DeerShadowTile;

			public ProgressionVariable PV_PondArtPuzzle_DragonTile;

			public ProgressionVariable PV_PondArtPuzzle_HouseTile;

			public ProgressionVariable PV_PondArtPuzzle_TigerTile;

			public ProgressionVariable PV_Pool_Minigame_State;

			public ProgressionVariable PV_PrinterInkInserted;

			public ProgressionVariable PV_PursuerTVShowActive;

			public ProgressionVariable PV_RabbitHole_BirdCage_PickedUpKey;

			public ProgressionVariable PV_RabbitHoleVisited;

			public ProgressionVariableInt PV_RadioBroadcastState;

			public ProgressionVariable PV_RepairedJukebox;

			public ProgressionVariable PV_Rifle_PickedUp;

			public ProgressionVariable PV_Riverside_Gasstation_Garage_Ambush_Active;

			public ProgressionVariable PV_Riverside_Gasstation_Garage_Gate_Broken;

			public ProgressionVariable PV_Riverside_Gasstation_Shortcut;

			public ProgressionVariable PV_Riverside_PickedUpFlashlight;

			public ProgressionVariable PV_Riverside_Police_Boss_Enabled;

			public ProgressionVariable PV_Riverside_Police_Office_Lights_On;

			public ProgressionVariable PV_Riverside_Police_Prisoners_Freed;

			public ProgressionVariable PV_Riverside_Repair_Key_Collected;

			public ProgressionVariable PV_Riverside_Sheriff_Open;

			public ProgressionVariable PV_SeenBellPuzzle;

			public ProgressionVariable PV_Sewer_Flow_A_Open;

			public ProgressionVariable PV_Sewer_Flow_B_Open;

			public ProgressionVariable PV_Sewer_Flow_C_Open;

			public ProgressionVariable PV_Sewer_Sluice_Open;

			public ProgressionVariable PV_Sheriff_Boss;

			public ProgressionVariable PV_Sheriff_Briefing_Key_Collected;

			public ProgressionVariable PV_Sheriff_Crowbar;

			public ProgressionVariable PV_Sheriff_Dogs_Out;

			public ProgressionVariable PV_Sheriff_Lock_Backdoor;

			public ProgressionVariable PV_Sheriff_Lockup_Lights;

			public ProgressionVariable PV_Sheriff_Safe_Completed;

			public ProgressionVariable PV_Sheriff_SpokenToBoots;

			public ProgressionVariable Pv_Sheriff_Sprinklers_On;

			public ProgressionVariable PV_Storage_Pause_Hint;

			public ProgressionVariable PV_Studio_Bleachers_Shortcut;

			public ProgressionVariable PV_Studio_Crying_Alice_Seen;

			public ProgressionVariable PV_Studio_Fireescape_Door_Unlocked;

			public ProgressionVariableInt PV_Studio_Light_MaskInt;

			public ProgressionVariable PV_Studio_Light_State;

			public ProgressionVariable PV_Studio_Nightmare_Key_Half_Taken;

			public ProgressionVariable PV_Studio_Nightmare_Key_Rust_Taken;

			public ProgressionVariable PV_Studio_Nightmare_Water_Lowered;

			public ProgressionVariable PV_Studio_Office_Shortcut;

			public ProgressionVariable PV_TalkedToPrisoner;

			public ProgressionVariable PV_Townhall_Basement_Door;

			public ProgressionVariable PV_Trailer;

			public ProgressionVariable PV_Tutorial_Save;

			public ProgressionVariable PV_Ventilation_Lumberyard;

			public ProgressionVariable PV_Ventilation_Transition_Generic;

			public ProgressionVariable PV_WretchingManDisappeared;

			public ProgressionVariable PV_WretchingManSeen;
		}

		[Serializable]
		public class VariablesGeneric
		{
			public DangerIntensityVariable DangerIntensity;

			public BoolVariable IsPlayerInside;

			public StringVariable MapScriptTrigger;

			public IntVariable MaxPlayerHealth;

			public IntVariable MaxStaminaUpgradeCount;

			public FloatVariable NearbyEnemyPresence;

			public BoolVariable PlayerFlashlightActive;

			public IntVariable PlayerHealth;

			public FloatVariable PlayerHealthPercentage;

			public FloatVariable PlayerStaminaReserve;

			public IntVariable PursuerHealth;

			public StatusEffectsVariable StatusEffects;
		}

		public VariablesLevelTransitionDoors LevelTransitionDoors;

		public VariablesMechanics Mechanics;

		public VariablesNPCs NPCs;

		public VariablesProgression Progression;

		public VariablesGeneric Generic;
	}

	public AnchorsReferences Anchors;

	public ArtifactEffectsReferences ArtifactEffects;

	private static GlobalReferences m_instance;

	private static bool s_isLoading;

	public SurfaceSettings DefaultSurfaceSettings;

	public PlayerMainInventory MainInventory;

	public Inventory StashInventory;

	public LoreInventory LoreInventory;

	public CollectableInventory CollectableInventory;

	public MaterialsReferences Materials;

	public InputState InputState;

	public GameMenuState GameMenuState;

	public GameState GameState;

	public ActiveObjective ActiveObjective;

	public TimeOfDay TimeOfDay;

	public CameraTargetData CameraTargetData;

	public MapDynamicData MapDynamicData;

	public UserMapData UserMapData;

	public GameCollectableSettings GameCollectableSettings;

	public GameDifficultySettings GameDifficultySettings;

	public AISharedSettings AISharedSettings;

	public Weather Weather;

	public UserPreferences UserPreferences;

	public PersistentDataStore DataStore;

	[NonSerialized]
	[HideInInspector]
	public ButtonPromptIconSettings PlaystationButtonPrompts;

	[NonSerialized]
	[HideInInspector]
	public ButtonPromptIconSettings XboxButtonPrompts;

	[NonSerialized]
	[HideInInspector]
	public ButtonPromptIconSettings SwitchButtonPrompts;

	[NonSerialized]
	[HideInInspector]
	public ButtonPromptIconSettings KeyboardMouseButtonPrompts;

	public AssetReferenceButtonPromptIconSettings PlaystationButtonsRef;

	public AssetReferenceButtonPromptIconSettings XboxButtonsRef;

	public AssetReferenceButtonPromptIconSettings SwitchButtonsRef;

	public AssetReferenceButtonPromptIconSettings KeyboardMouseButtonsRef;

	public DoorInteractGlobalSettings DoorInteractSettings;

	public AssetReference TestImpactVFX;

	public GraphicsQualityGlobalSettings GraphicsSettings;

	public ScriptableRendererFeature ScreenSpaceAmbientOcclusionRenderFeature;

	public Achievements Achievements;

	public DLCs DLC;

	[SerializeField]
	public MapViewMetadata[] MapviewMetadatas;

	public EventChannelsReferences EventChannels;

	public SetsReferences Sets;

	public StatusEffectsReferences StatusEffects;

	public VariablesReferences Variables;

	public static GlobalReferences Instance
	{
		get
		{
			if (m_instance == null)
			{
				Debug.Log("Loading GlobalReferences synchronously");
				if (s_isLoading)
				{
					Debug.LogError("Tried to double load the GlobalReferences instance! This will result in a crash in a build! Don't try to access GlobalReferences.Instance in OnEnable for ScriptableObject instances, as they will double up when loading the GlobalReferences package!");
					return null;
				}
				s_isLoading = true;
				m_instance = Addressables.LoadAssetAsync<GlobalReferences>("GlobalReferences").WaitForCompletion();
				m_instance.PlaystationButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.PlaystationButtonsRef);
				m_instance.XboxButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.XboxButtonsRef);
				m_instance.SwitchButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.SwitchButtonsRef);
				m_instance.KeyboardMouseButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.KeyboardMouseButtonsRef);
				s_isLoading = false;
			}
			return m_instance;
		}
	}

	public static IEnumerator LoadSync()
	{
		s_isLoading = true;
		AsyncOperationHandle<GlobalReferences> asyncOperationHandle = Addressables.LoadAssetAsync<GlobalReferences>("GlobalReferences");
		asyncOperationHandle.WaitForCompletion();
		m_instance = asyncOperationHandle.Result;
		m_instance.PlaystationButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.PlaystationButtonsRef);
		m_instance.XboxButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.XboxButtonsRef);
		m_instance.SwitchButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.SwitchButtonsRef);
		m_instance.KeyboardMouseButtonPrompts = AddressableReferenceLoader.LoadValidSync<ButtonPromptIconSettings, AssetReferenceButtonPromptIconSettings>(m_instance.KeyboardMouseButtonsRef);
		s_isLoading = false;
		yield break;
	}

	public static IEnumerator LoadAsync()
	{
		yield return LoadSync();
	}
}
