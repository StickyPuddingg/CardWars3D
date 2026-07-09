using System;
using System.Collections;
using UnityEngine;

public class SLOTStartup_dbl : MonoBehaviour
{
	public string startupScene;

	private bool started;

	private void Update()
	{
		if (!started)
		{
			started = true;
			Startup();
		}
	}

	private void Startup()
	{
		if (startupScene != null && startupScene.Length > 0)
		{
			try
			{
				SLOTGameSingleton<SLOTSceneManager_dbl>.GetInstance().LoadLevel(startupScene);
			}
			catch (Exception ex)
			{
				Debug.LogError("[SLOTStartup_dbl] Falha ao carregar cena '" + startupScene + "': " + ex.Message);
			}
		}
	}
}
