using System;
using UnityEngine;

public class Logger : MonoBehaviour
{
	public static Logger Instance;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	public void LogInstance(string message)
	{
		UnityEngine.Debug.Log("[StickyLogger] " + message);
	}

	public void LogFormatInstance(string format, params object[] args)
	{
		UnityEngine.Debug.Log("[StickyLogger] " + string.Format(format, args));
	}

	public void WarnInstance(string message)
	{
		UnityEngine.Debug.LogWarning("[StickyLogger] " + message);
	}

	public void ErrorInstance(string message)
	{
		UnityEngine.Debug.LogError("[StickyLogger] " + message);
	}

	public void LogWithContextInstance(UnityEngine.Object context, string message)
	{
		UnityEngine.Debug.Log("[StickyLogger] " + message, context);
	}

	public static void Log(string message)
	{
		if (Instance != null) Instance.LogInstance(message);
		else UnityEngine.Debug.Log("[StickyLogger] " + message);
	}

	public static void LogFormat(string format, params object[] args)
	{
		if (Instance != null) Instance.LogFormatInstance(format, args);
		else UnityEngine.Debug.Log("[StickyLogger] " + string.Format(format, args));
	}

	public static void Warn(string message)
	{
		if (Instance != null) Instance.WarnInstance(message);
		else UnityEngine.Debug.LogWarning("[StickyLogger] " + message);
	}

	public static void Error(string message)
	{
		if (Instance != null) Instance.ErrorInstance(message);
		else UnityEngine.Debug.LogError("[StickyLogger] " + message);
	}

	public static void LogWithContext(UnityEngine.Object context, string message)
	{
		if (Instance != null) Instance.LogWithContextInstance(context, message);
		else UnityEngine.Debug.Log("[StickyLogger] " + message, context);
	}
}
