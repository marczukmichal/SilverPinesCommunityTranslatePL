using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Shuriken")]
[Tooltip("Plays a particleSystem")]
public class ParticleSystemPlay : FsmStateAction
{
	[RequiredField]
	[Tooltip("The GameObject with the particleSystem to play")]
	[CheckForComponent(typeof(ParticleSystem))]
	public FsmOwnerDefault gameObject;

	[Tooltip("Stop playing again when state exits")]
	public FsmBool stopOnExit;

	private ParticleSystem _ps;

	public override void Reset()
	{
		gameObject = null;
		stopOnExit = null;
	}

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (!(ownerDefaultTarget == null))
		{
			_ps = ownerDefaultTarget.GetComponent<ParticleSystem>();
			if (!(_ps == null))
			{
				_ps.Play();
				Finish();
			}
		}
	}

	public override void OnExit()
	{
		if (_ps != null && stopOnExit.Value)
		{
			_ps.Stop();
		}
	}
}
