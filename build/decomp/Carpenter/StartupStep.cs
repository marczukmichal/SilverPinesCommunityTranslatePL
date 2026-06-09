using System.Collections;
using UnityEngine;

public abstract class StartupStep : MonoBehaviour
{
	public abstract IEnumerator Execute(StartupLoadingScreen loadingScreen);
}
