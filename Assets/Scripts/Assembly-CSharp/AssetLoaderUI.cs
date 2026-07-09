using UnityEngine;

public class AssetLoaderUI : MonoBehaviour
{
	public enum State
	{
		Init,
		LoadScene,
		LoadingScene
	}

	public string startupSceneName = "AdventureTime";

	public UITexture barTexture;

	public UITexture barBG;

	public GameObject retryButton;

	public UILabel messageLabel;

	public BusyIconController busyIconController;

	private float origWidth;

	private State state;

	private void Start()
	{
		if (barTexture != null)
		{
			origWidth = barTexture.transform.localScale.x;
		}
		if (busyIconController == null)
		{
			busyIconController = SLOTGame.GetInstance();
		}
		// -s Assets ship with the 3DS build, so do not check remote downloads or show an asset-progress flow.
		ShowRetryButton(false);
		ShowProgressBar(false);
		HideMessage();
		if (busyIconController != null)
		{
			busyIconController.ShowBusyIcon(false);
		}
		state = State.LoadScene;
	}

	public void SetProgress(float progress)
	{
		if (barTexture != null)
		{
			if (progress < 0f)
			{
				progress = 0f;
			}
			else if (progress > 1f)
			{
				progress = 1f;
			}
			Vector3 localScale = barTexture.transform.localScale;
			localScale.x = origWidth * progress;
			barTexture.transform.localScale = localScale;
		}
	}

	private void Update()
	{
		UpdateState();
	}

	private void UpdateState()
	{
		switch (state)
		{
		case State.Init:
			state = State.LoadScene;
			break;
		case State.LoadScene:
			// -s Skip the download-check path and load the startup scene directly.
			if (busyIconController != null)
			{
				busyIconController.ShowBusyIcon(true);
			}
			SLOTGameSingleton<SLOTSceneManager>.GetInstance().LoadLevelAsync(startupSceneName, LoadLevelDoneCallback);
			state = State.LoadingScene;
			break;
		}
	}

	private void LoadLevelDoneCallback()
	{
		if (busyIconController != null)
		{
			busyIconController.ShowBusyIcon(false);
		}
	}

	private void ShowMessage(string message)
	{
		if (messageLabel != null)
		{
			NGUITools.SetActive(messageLabel.gameObject, true);
			messageLabel.text = message;
		}
	}

	private void HideMessage()
	{
		if (messageLabel != null)
		{
			NGUITools.SetActive(messageLabel.gameObject, false);
		}
	}

	private void ShowRetryButton(bool b)
	{
		if (retryButton != null)
		{
			NGUITools.SetActive(retryButton.gameObject, b);
		}
	}

	private void ShowProgressBar(bool b)
	{
		if (barTexture != null)
		{
			NGUITools.SetActive(barTexture.gameObject, b);
		}
		if (barBG != null)
		{
			NGUITools.SetActive(barBG.gameObject, b);
		}
	}

	private void RetryClicked()
	{
		ShowRetryButton(false);
		HideMessage();
		state = State.LoadScene;
	}
}
