using System;
using System.Collections.Generic;
using UnityEngine;

/// ponytail: disabled network - all requests return null, no HTTP calls made
public class KFFNetwork2_dbl : MonoBehaviour
{
	private static int MAX_CONCURRENT_WWW_REQUEST_COUNT = 1;
	private List<object> wwwList = new List<object>();
	private int activeRequestCount;
	private static KFFNetwork2_dbl the_instance;

	public static KFFNetwork2_dbl GetInstance()
	{
		if (!the_instance)
		{
			the_instance = FindObjectOfType(typeof(KFFNetwork2_dbl)) as KFFNetwork2_dbl;
		}
		if (Application.isPlaying && !the_instance)
		{
			GameObject gameObject = new GameObject();
			if (gameObject)
			{
				the_instance = gameObject.AddComponent(typeof(KFFNetwork2_dbl)) as KFFNetwork2_dbl;
			}
			if (gameObject)
			{
				gameObject.transform.position = new Vector3(999999f, 999999f, 999999f);
				gameObject.name = "AutomaticallyCreatedKFFNetwork2_dbl";
				DontDestroyOnLoad(gameObject);
			}
		}
		return the_instance;
	}

	public object SendWWWRequest(string scriptNameAndParams, object callback, object callbackParam)
	{
		Debug.LogWarning("[KFFNetwork2_dbl] Network disabled - ignoring request: " + scriptNameAndParams);
		return null;
	}

	public object SendWWWRequestWithForm(object form, string scriptNameAndParams, object callback, object callbackParam)
	{
		Debug.LogWarning("[KFFNetwork2_dbl] Network disabled - ignoring request: " + scriptNameAndParams);
		return null;
	}

	public void SendWWWRequestWithTimeOut(string scriptNameAndParams, int timeOut, object callback, object callbackParam)
	{
		Debug.LogWarning("[KFFNetwork2_dbl] Network disabled - ignoring request: " + scriptNameAndParams);
	}

	public void CancelWWWRequest(object wwwInfo)
	{
	}

	public void CancelAllWWWRequest()
	{
	}

	public void SetServerAddress(string address)
	{
	}

	public string GetServerAddress()
	{
		return "";
	}
}
