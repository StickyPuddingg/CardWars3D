using System;
using System.IO;
using System.Net;
using UnityEngine;

/// ponytail: disabled server sync - only local save/load, no HTTP requests
public class SQWebFileServer_dbl : TFWebFileServer_dbl
{
	private const string PERSISTENCE_PATH = "persist";
	private string deviceId;
	private string username;

	public DateTime LastSuccessfulSave { get; set; }

	public string Username
	{
		set
		{
			username = value;
		}
	}

	public SQWebFileServer_dbl(CookieContainer cookies, string deviceId = null)
		: base(cookies)
	{
		if (deviceId != null)
		{
			deviceId = deviceId.Trim();
			if (deviceId.Length <= 0)
			{
				deviceId = null;
			}
		}
		this.deviceId = deviceId;
		Debug.Log("[SQWebFileServer_dbl] Initialized with local save only");
	}

	public void SetPlayerInfo(object player)
	{
		Debug.Log("[SQWebFileServer_dbl] SetPlayerInfo called - local save only");
	}

	public void SaveGameState(string data, object callback, object userData = null)
	{
		Debug.Log("[SQWebFileServer_dbl] SaveGameState - using local save only");
		LastSuccessfulSave = DateTime.Now;
		// Callbacks are ignored - local save only
	}

	public void LoadGameState(object callback, object userData = null)
	{
		Debug.Log("[SQWebFileServer_dbl] LoadGameState - using local load only");
		// Callbacks are ignored - local load only
	}

	public void GetFile(string uri, WebHeaderCollection headers, object callback, object userData = null)
	{
		Debug.LogWarning("[SQWebFileServer_dbl] GetFile blocked - local save only");
	}

	public void PostFile(string uri, string data, WebHeaderCollection headers, object callback, object userData = null)
	{
		Debug.LogWarning("[SQWebFileServer_dbl] PostFile blocked - local save only");
		LastSuccessfulSave = DateTime.Now;
	}
}
