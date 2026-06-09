using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebug : MonoBehaviour
{
	[SerializeField]
	private FloatGameEventChannel m_debugTimeScaleEventChannel;

	private void Update()
	{
	}

	private void FixedUpdate()
	{
	}

	[DebugCommand("timescale", "Set timescale", "timescale <scale>", typeof(float), false)]
	public static void SetTimeScale(float timeScale)
	{
		Time.timeScale = timeScale;
	}

	private void TeleportToCursor()
	{
		Vector3 worldPosFromScreen = GameUtils.GetWorldPosFromScreen(Mouse.current.position.ReadValue(), base.transform.position.z);
		worldPosFromScreen.z = base.transform.position.z;
		base.transform.position = worldPosFromScreen;
		ResetPlayerStateMachine();
	}

	private void TeleportTowardsCursor()
	{
		Vector3 worldPosFromScreen = GameUtils.GetWorldPosFromScreen(Mouse.current.position.ReadValue(), base.transform.position.z);
		Vector3 position = base.transform.position;
		Vector3 vector = worldPosFromScreen - position;
		vector.z = 0f;
		vector.Normalize();
		Vector3 position2 = position + vector * Time.deltaTime * 35f;
		position2.z = base.transform.position.z;
		base.transform.position = position2;
		CharacterMovement component = GetComponent<CharacterMovement>();
		if (component != null)
		{
			component.SetPreviousVelocity(Vector2.zero);
		}
		GetComponent<CharacterCameraFollow>()?.SetInstantMove();
		ResetPlayerStateMachine();
	}

	private void ResetPlayerStateMachine()
	{
		PlayMakerFSM component = GetComponent<PlayMakerFSM>();
		if (component != null)
		{
			component.SendEvent("Reset");
		}
		CharacterInteractor component2 = GetComponent<CharacterInteractor>();
		if (component2 != null)
		{
			component2.StopInteracting();
		}
	}
}
