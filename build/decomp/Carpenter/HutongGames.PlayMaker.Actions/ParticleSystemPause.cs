using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Shuriken")]
[Tooltip("Pauses a particleSystem")]
public class ParticleSystemPause : FsmStateAction
{
	[RequiredField]
	[Tooltip("The GameObject with the particleSystem to pause")]
	[CheckForComponent(typeof(ParticleSystem))]
	public FsmOwnerDefault gameObject;

	[Tooltip("Plays again when state exits")]
	public FsmBool playOnExit;

	private ParticleSystem _ps;

	public override void Reset()
	{
		gameObject = null;
		playOnExit = null;
	}

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (!(ownerDefaultTarget == null))
		{
			_ps = ownerDefaultTarget.GetComponent<ParticleSystem>();
			if (!(_ps == null))
			{
				_ps.Pause();
				Finish();
			}
		}
	}

	public override void OnExit()
	{
		if (_ps != null && playOnExit.Value)
		{
			_ps.Play();
		}
	}
}
