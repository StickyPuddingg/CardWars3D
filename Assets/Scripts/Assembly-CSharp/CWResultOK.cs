using System.Collections;
using UnityEngine;

public class CWResultOK : MonoBehaviour
{
	public bool GoToBuildDeck;

	private void Start()
	{
		if (GoToBuildDeck)
		{
			GlobalFlags.Instance.ReturnToBuildDeck = true;
		}
		else
		{
			GlobalFlags.Instance.ReturnToMainMenu = true;
		}

		PlayerInfoScript.GetInstance().Save();
		StartCoroutine(GoToMainMenu());
	}

	private IEnumerator GoToMainMenu()
	{
		Debug.Log("SCREEEEEEEAM");

		UICamera.useInputEnabler = true;
		float savedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		yield return Resources.UnloadUnusedAssets();
		Time.timeScale = savedTimeScale;
		UICamera.useInputEnabler = false;
		SLOTGameSingleton<SLOTSceneManager>.GetInstance().LoadLevel("AdventureTime");
	}
}
