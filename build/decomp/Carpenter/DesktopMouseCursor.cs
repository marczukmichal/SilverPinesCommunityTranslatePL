using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DesktopMouseCursor : MonoBehaviour, ICursorOverrides
{
	[SerializeField]
	private Camera m_camera;

	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private RectTransform m_desktopCursorBounds;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private ComputerDesktop m_desktop;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Sprite m_normalSprite;

	[SerializeField]
	private Sprite m_busySprite;

	private bool m_isBusy;

	private bool m_isShowingCursor;

	public ICursorOverrides.CursorOverrideOption ShouldShowCursor
	{
		get
		{
			if (!m_isShowingCursor)
			{
				return ICursorOverrides.CursorOverrideOption.Unchanged;
			}
			return ICursorOverrides.CursorOverrideOption.ForceOn;
		}
	}

	private bool IsCusorInBounds()
	{
		Vector2 screenPoint = Mouse.current.position.ReadValue();
		return RectTransformUtility.RectangleContainsScreenPoint(m_desktopCursorBounds, screenPoint);
	}

	private void Update()
	{
		bool flag = IsCusorInBounds();
		if (flag != m_isShowingCursor)
		{
			m_isShowingCursor = flag;
			if (flag)
			{
				m_image.enabled = true;
				GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(this);
			}
			else
			{
				m_image.enabled = false;
				GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(null);
			}
		}
		if (m_isShowingCursor)
		{
			Vector2 vector = Mouse.current.position.ReadValue();
			vector.x = (int)vector.x;
			vector.y = (int)vector.y;
			vector /= m_canvas.scaleFactor;
			m_rectTransform.anchoredPosition3D = vector;
			UpdateCursorType();
		}
	}

	private void OnDisable()
	{
		if (GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Item == this)
		{
			GlobalReferences.Instance.Anchors.Input.CursorOverridesAnchor.Set(null);
		}
	}

	private IEnumerator LoadingLoop()
	{
		while (m_isBusy)
		{
			yield return new WaitForSecondsRealtime(1f);
			m_rectTransform.Rotate(0f, 0f, 90f);
			yield return new WaitForSecondsRealtime(0.1f);
			m_rectTransform.Rotate(0f, 0f, 90f);
			yield return new WaitForSecondsRealtime(0.1f);
			m_rectTransform.Rotate(0f, 0f, 90f);
			yield return new WaitForSecondsRealtime(0.1f);
			m_rectTransform.Rotate(0f, 0f, 90f);
			yield return new WaitForSecondsRealtime(0.1f);
			m_rectTransform.rotation = Quaternion.identity;
		}
	}

	private void UpdateCursorType()
	{
		bool isBusy = m_desktop.IsBusy;
		if (isBusy != m_isBusy)
		{
			StopAllCoroutines();
			m_isBusy = isBusy;
			m_image.sprite = (isBusy ? m_busySprite : m_normalSprite);
			if (isBusy)
			{
				m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
				StartCoroutine(LoadingLoop());
			}
			else
			{
				m_rectTransform.rotation = Quaternion.identity;
				m_rectTransform.pivot = new Vector2(0f, 1f);
			}
		}
	}
}
