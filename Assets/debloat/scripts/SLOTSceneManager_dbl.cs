using UnityEngine;
using UnityEngine.SceneManagement;

/// ponytail: async scene load - blocking LoadScene() froze the 3DS while deserializing 588 GameObjects synchronously
public class SLOTSceneManager_dbl : SLOTGameSingleton<SLOTSceneManager_dbl>
{
	public bool useLocalScenes;

	public void LoadLevel(string name)
	{
		if (name != null && name.Length > 0)
		{
			SceneManager.LoadSceneAsync(name);
		}
	}

	public void LoadLevelAdditive(string name)
	{
		if (name != null && name.Length > 0)
		{
			SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		}
	}
}
