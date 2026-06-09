using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

public class DemoCallToActionScreen : MonoBehaviour
{
	[SerializeField]
	private FullscreenMenuRedFlash m_redFlash;

	[SerializeField]
	private MenuFade m_menuFade;

	[SerializeField]
	private GameObject m_button;

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private float m_waitTime = 10f;

	private bool m_tryExit;

	private void CancelClicked(InputAction.CallbackContext obj)
	{
		m_tryExit = true;
	}

	private IEnumerator Start()
	{
		m_container.SetActive(value: false);
		AsyncOperationHandle<IList<IResourceLocation>> loc = Addressables.LoadResourceLocationsAsync("UtilityScenes/PersistentManagers");
		yield return loc.WaitForCompletion();
		if (!SceneManager.GetSceneByPath(loc.Result[0].InternalId).isLoaded)
		{
			yield return ((AsyncOperationHandle)Addressables.LoadSceneAsync("UtilityScenes/PersistentManagers", LoadSceneMode.Additive)).WaitForCompletion();
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.1f);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Enable();
			GameInputManager.GameInputActions.UI.Cancel.performed += CancelClicked;
		}
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.InGameMenu);
		m_container.SetActive(value: true);
		EventSystem.current.SetSelectedGameObject(m_button);
		m_redFlash.Flash();
		float timer = 0f;
		while (timer < m_waitTime && !m_tryExit)
		{
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		yield return m_menuFade.FadeCoroutine();
		yield return new WaitForSeconds(1f);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Cancel.performed -= CancelClicked;
		}
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}
}
