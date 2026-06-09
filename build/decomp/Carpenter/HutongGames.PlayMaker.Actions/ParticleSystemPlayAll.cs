using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Shuriken")]
[Tooltip("Plays All particles from a GameObject. Can include All children as well")]
public class ParticleSystemPlayAll : FsmStateAction
{
	[RequiredField]
	[Tooltip("The GameObject with particles")]
	public FsmOwnerDefault gameObject;

	[Tooltip("Play All particles even on children")]
	public FsmBool includeChilden;

	public override void Reset()
	{
		gameObject = null;
		includeChilden = true;
	}

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (ownerDefaultTarget != null)
		{
			ParticleSystem[] components = ownerDefaultTarget.GetComponents<ParticleSystem>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].Play();
			}
			if (includeChilden.Value)
			{
				components = ownerDefaultTarget.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].Play();
				}
			}
		}
		Finish();
	}
}
