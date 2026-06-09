using DG.Tweening;
using Shapes2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ActiveUseInteractionPanel : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve m_movementCurve;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private ActiveUseStateGameEventChannel m_activeUseStateUpdateEventChannel;

	[SerializeField]
	private float m_fadeTime = 0.2f;

	[SerializeField]
	private float m_hideDelayTime = 0.5f;

	[SerializeField]
	private RectTransform m_cursor;

	[SerializeField]
	private RectTransform m_activeReloadTimeMark;

	[SerializeField]
	private Shape m_activeArcShape;

	[SerializeField]
	private Shape m_reloadWheelShape;

	[SerializeField]
	private RectTransform m_animateTransform;

	[SerializeField]
	private RectTransform m_glowPulseTransform;

	[SerializeField]
	private AnimationCurve m_glowPulseCurve;

	[SerializeField]
	private float m_glowTime = 2f;

	[SerializeField]
	private RectTransform m_reloadPromptRotationTransform;

	[SerializeField]
	private CanvasGroup m_reloadPromptCanvasGroup;

	[SerializeField]
	private RectTransform m_reloadPromptTransform;

	[SerializeField]
	private float m_offsetAngleForNoContinue = 15f;

	[SerializeField]
	private RectTransform m_reloadActionViewParent;

	[Header("Colors")]
	[SerializeField]
	private Color m_successArcColor;

	[SerializeField]
	private Color m_failedArcColor;

	[SerializeField]
	private Color m_normalArcColor;

	[SerializeField]
	private Color m_inRangeArcColor;

	[SerializeField]
	private Color m_successMainColor;

	[SerializeField]
	private Color m_failedMainColor;

	[SerializeField]
	private Color m_normalMainColor;

	private ActionUseStateViewBase m_actionView;

	private AssetReference m_currentlyShowingInteractionView;

	private bool m_isShowing;

	private ActiveUseState m_previousActiveUseState;

	private float m_flashTimer;

	private int m_currentAmmoCount;

	private AsyncOperationHandle<GameObject> m_handle;

	private void Start()
	{
		if (!m_isShowing)
		{
			m_canvasGroup.alpha = 0f;
		}
	}

	private void OnEnable()
	{
		m_activeUseStateUpdateEventChannel.Register(ActiveUseUpdateEvent);
	}

	private void OnDisable()
	{
		m_activeUseStateUpdateEventChannel.Unregister(ActiveUseUpdateEvent);
	}

	private void LoadActionView(AssetReference assetReference)
	{
		if (m_actionView != null)
		{
			Object.Destroy(m_actionView.gameObject);
		}
		if (m_handle.IsValid())
		{
			Addressables.Release(m_handle);
		}
		if (assetReference.HasAsset())
		{
			m_handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
			m_handle.WaitForCompletion();
			GameObject gameObject = Object.Instantiate(m_handle.Result, m_reloadActionViewParent);
			m_actionView = gameObject.GetComponent<ActionUseStateViewBase>();
			m_currentlyShowingInteractionView = assetReference;
		}
		else
		{
			m_currentlyShowingInteractionView = null;
		}
	}

	private void ActiveUseUpdateEvent(ActiveUseStateInfoData eventData)
	{
		bool isActive = eventData.m_isActive;
		if (isActive != m_isShowing)
		{
			if (isActive)
			{
				if (eventData.m_actionViewAssetReference != m_currentlyShowingInteractionView)
				{
					LoadActionView(eventData.m_actionViewAssetReference);
				}
				if (m_actionView != null)
				{
					m_actionView.Setup(eventData.m_currentAmmoAmount, eventData.m_maxAmmoCapacity);
				}
			}
			Show(isActive);
		}
		if (!isActive)
		{
			return;
		}
		bool flag = eventData.m_activeUseState != ActiveUseState.None;
		if (m_previousActiveUseState != eventData.m_activeUseState)
		{
			switch (eventData.m_activeUseState)
			{
			case ActiveUseState.Success:
				AnimateSuccess();
				break;
			case ActiveUseState.Failed:
				AnimateFail();
				break;
			}
			m_activeReloadTimeMark.gameObject.SetActive(flag);
			m_previousActiveUseState = eventData.m_activeUseState;
		}
		bool inRange = false;
		if (eventData.m_activeUseState == ActiveUseState.None && eventData.m_progress >= eventData.m_activeActionTimeMin && eventData.m_progress <= eventData.m_activeActiveTimeMax)
		{
			inRange = true;
		}
		SetColorsForState(m_flashTimer > 0f, inRange, eventData.m_activeUseState);
		if (m_flashTimer > 0f)
		{
			m_flashTimer -= Time.deltaTime;
		}
		float num = 0f;
		if (eventData.m_isLastReloadAction)
		{
			num = m_offsetAngleForNoContinue;
		}
		if (eventData.m_canActiveInteract)
		{
			m_activeArcShape.settings.startAngle = Mathf.Lerp(360f - num, num, m_movementCurve.Evaluate(eventData.m_activeActiveTimeMax));
			m_activeArcShape.settings.endAngle = Mathf.Lerp(360f - num, num, m_movementCurve.Evaluate(eventData.m_activeActionTimeMin));
			m_activeArcShape.gameObject.SetActive(value: true);
			SetReloadPromptPostion((m_movementCurve.Evaluate(eventData.m_activeActiveTimeMax) + m_movementCurve.Evaluate(eventData.m_activeActionTimeMin)) / 2f, num);
			bool flag2 = false;
			if (eventData.m_activeUseState == ActiveUseState.None)
			{
				flag2 = eventData.m_progress >= eventData.m_activeActionTimeMin && eventData.m_progress <= eventData.m_activeActiveTimeMax;
			}
			m_reloadPromptCanvasGroup.gameObject.SetActive(value: true);
			m_reloadPromptCanvasGroup.alpha = (flag2 ? 1f : 0f);
			float time = Time.time % m_glowTime / m_glowTime;
			m_glowPulseTransform.transform.localScale = Vector3.one * m_glowPulseCurve.Evaluate(time);
		}
		else
		{
			m_reloadPromptCanvasGroup.gameObject.SetActive(value: false);
			m_activeArcShape.gameObject.SetActive(value: false);
		}
		if (m_currentAmmoCount != eventData.m_currentAmmoAmount)
		{
			m_currentAmmoCount = eventData.m_currentAmmoAmount;
			if (m_actionView != null)
			{
				m_actionView.UpdateAmmoCount(eventData.m_currentAmmoAmount, eventData.m_isLastReloadAction);
			}
		}
		m_reloadWheelShape.settings.startAngle = (eventData.m_isLastReloadAction ? m_offsetAngleForNoContinue : 0f);
		m_reloadWheelShape.settings.endAngle = (eventData.m_isLastReloadAction ? (360f - m_offsetAngleForNoContinue) : 0f);
		if (flag)
		{
			SetCursorPosition(m_activeReloadTimeMark, m_movementCurve.Evaluate(eventData.m_activeActionTime), num);
		}
		SetCursorPosition(m_cursor, m_movementCurve.Evaluate(eventData.m_progress), num);
	}

	private void SetColorsForState(bool doFlash, bool inRange, ActiveUseState reloadState)
	{
		switch (reloadState)
		{
		case ActiveUseState.Success:
			m_activeArcShape.settings.fillColor = m_successArcColor;
			m_reloadWheelShape.settings.fillColor = m_successMainColor;
			break;
		case ActiveUseState.Failed:
			m_activeArcShape.settings.fillColor = m_failedArcColor;
			m_reloadWheelShape.settings.fillColor = m_failedMainColor;
			break;
		case ActiveUseState.None:
			m_activeArcShape.settings.fillColor = (inRange ? m_inRangeArcColor : m_normalArcColor);
			m_reloadWheelShape.settings.fillColor = m_normalMainColor;
			break;
		}
		if (doFlash)
		{
			switch (reloadState)
			{
			case ActiveUseState.Success:
				m_activeArcShape.settings.fillColor = Color.white;
				break;
			case ActiveUseState.Failed:
				m_reloadWheelShape.settings.fillColor = Color.red;
				break;
			}
		}
	}

	private void AnimateSuccess()
	{
		DOTween.Kill(m_animateTransform, complete: true);
		m_flashTimer = 0.075f;
	}

	private void AnimateFail()
	{
		DOTween.Kill(m_animateTransform, complete: true);
		m_animateTransform.DOShakeAnchorPos(0.75f, 10f, 100);
		m_flashTimer = 0.075f;
	}

	private void SetCursorPosition(RectTransform cursor, float normalisedValue, float angleOffset)
	{
		cursor.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(180f - angleOffset, -180f + angleOffset, normalisedValue));
	}

	private void SetReloadPromptPostion(float normalisedValue, float angleOffset)
	{
		m_reloadPromptRotationTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(270f - angleOffset, -90f + angleOffset, normalisedValue));
		m_reloadPromptTransform.rotation = Quaternion.identity;
	}

	private void Show(bool shouldShow)
	{
		if (shouldShow)
		{
			m_activeReloadTimeMark.gameObject.SetActive(value: false);
			m_currentAmmoCount = -1;
			m_canvasGroup.alpha = 0f;
		}
		m_isShowing = shouldShow;
		m_canvasGroup.DOKill();
		m_canvasGroup.DOFade(shouldShow ? 1f : 0f, m_fadeTime).SetDelay(shouldShow ? 0f : m_hideDelayTime);
	}
}
