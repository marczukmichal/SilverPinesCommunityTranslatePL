using UnityEngine;

public static class GameLayers
{
	public static string DefaultLayerName = "Default";

	public static int DefaultLayer = LayerMask.NameToLayer(DefaultLayerName);

	public static string GameplayElementsLayerName = "GameplayElements";

	public static int GameplayElementsLayer = LayerMask.NameToLayer(GameplayElementsLayerName);

	public static string CharacterNoCollideLayerName = "CharactersNoWorldCollision";

	public static int CharacterNoCollideLayer = LayerMask.NameToLayer(CharacterNoCollideLayerName);

	public static string CollidesWithCharactersOnlyName = "CollidesWithCharactersOnly";

	public static int CollidesWithCharactersOnlyLayer = LayerMask.NameToLayer(CollidesWithCharactersOnlyName);

	public static string EnvironmentLayerName = "Environment";

	public static int EnvironmentLayer = LayerMask.NameToLayer(EnvironmentLayerName);

	public static string EnvironmentNoFogOfWarLayerName = "EnvironmentNoFogOfWar";

	public static int EnvironmentNoFogOfWarLayer = LayerMask.NameToLayer(EnvironmentNoFogOfWarLayerName);

	public static string SpecialOnlyEnvironmentLayerName = "SpecialOnlyEnvironment";

	public static int SpecialOnlyEnvironmentLayer = LayerMask.NameToLayer(SpecialOnlyEnvironmentLayerName);

	public static string TraversalLayerName = "Traversal";

	public static int TraversalLayer = LayerMask.NameToLayer(TraversalLayerName);

	public static string InteractableLayerName = "Interactable";

	public static int InteractableLayer = LayerMask.NameToLayer(InteractableLayerName);

	public static string WaterLayerName = "Water";

	public static int WaterLayer = LayerMask.NameToLayer(WaterLayerName);

	public static string MovementLayerName = "Movement";

	public static int MovementLayer = LayerMask.NameToLayer(MovementLayerName);

	public static string DamageableLayerName = "Damageable";

	public static int DamageableLayer = LayerMask.NameToLayer(DamageableLayerName);

	public static string DamageableBarrierLayerName = "DamageableBarrier";

	public static int DamageableBarrierLayer = LayerMask.NameToLayer(DamageableBarrierLayerName);

	public static string UILayerName = "UI";

	public static int UILayer = LayerMask.NameToLayer(UILayerName);

	public static string MinigameLayerName = "Minigame";

	public static int MinigameLayer = LayerMask.NameToLayer(MinigameLayerName);

	public static string AIToolsLayerName = "AITools";

	public static int AIToolsLayer = LayerMask.NameToLayer(AIToolsLayerName);

	public static string PushableLayerName = "Pushable";

	public static int PushableLayer = LayerMask.NameToLayer(PushableLayerName);

	public static string ProjectileLayerName = "Projectiles";

	public static int ProjectileLayer = LayerMask.NameToLayer(ProjectileLayerName);

	public static string Map3DName = "3DMap";

	public static int Map3DLayer = LayerMask.NameToLayer(Map3DName);

	public static string StairsName = "Stairs";

	public static int StairsLayer = LayerMask.NameToLayer(StairsName);

	public static int DamageablesMask = (1 << DamageableLayer) | (1 << GameplayElementsLayer) | (1 << DamageableBarrierLayer);

	public static int EnvironmentMask = (1 << DefaultLayer) | (1 << EnvironmentLayer) | (1 << EnvironmentNoFogOfWarLayer);

	public static int CharacterNavigationMask = EnvironmentMask | (1 << CollidesWithCharactersOnlyLayer) | (1 << DamageableBarrierLayer) | (1 << StairsLayer);

	public static int PushableNavigationMask = EnvironmentMask | (1 << CollidesWithCharactersOnlyLayer) | (1 << DamageableBarrierLayer);

	public static int EnemySpecialNavigationMask = EnvironmentMask | (1 << DamageableBarrierLayer) | (1 << StairsLayer) | (1 << SpecialOnlyEnvironmentLayer);

	public static int ClimbableMask = CharacterNavigationMask;

	public static int FogOfWarMask = (1 << DefaultLayer) | (1 << EnvironmentLayer);

	public static int CatEnvironmentMask = (1 << DefaultLayer) | (1 << EnvironmentLayer) | (1 << EnvironmentNoFogOfWarLayer) | (1 << SpecialOnlyEnvironmentLayer);

	public static int ProjectileMask = EnvironmentMask | (1 << GameplayElementsLayer) | (1 << CharacterNoCollideLayer) | (1 << WaterLayer) | (1 << DamageableLayer);

	public static int MeleeMask = EnvironmentMask | (1 << GameplayElementsLayer) | (1 << CharacterNoCollideLayer) | ((1 << DamageableLayer) | (1 << DamageableBarrierLayer));

	public static int UIMask = 1 << UILayer;

	public static int MinigameMask = 1 << MinigameLayer;

	public static int AIToolsMask = 1 << AIToolsLayer;

	public static int PushableMask = 1 << PushableLayer;

	public static int MapMask = 1 << Map3DLayer;

	public static int StairsMask = 1 << StairsLayer;
}
