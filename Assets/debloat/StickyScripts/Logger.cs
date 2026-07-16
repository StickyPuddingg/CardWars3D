using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance;

    // Bridge to the native 3DS OS kernel function
#if UNITY_3DS && !UNITY_EDITOR
	[DllImport("__Internal", EntryPoint = "_ZN2nn3svc17OutputDebugStringEPKci")]
	private static extern int OutputDebugString(string text, int length);
#endif

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

    /// <summary>
    /// Sends a direct string to the 3DS OS/Emulator debug console bypassing standard buffers.
    /// </summary>
    public static void SendToEmulator(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        string formattedMessage = "[StickyLogger] " + message + "\n";

#if UNITY_3DS && !UNITY_EDITOR
		try 
		{
			OutputDebugString(formattedMessage, formattedMessage.Length);
		}
		catch (Exception)
		{
			// Fallback fallback if the PInvoke fails
			UnityEngine.Debug.Log(formattedMessage);
		}
#else
        // Fallback for Editor or other platforms
        UnityEngine.Debug.Log(formattedMessage);
#endif
    }

    public void LogInstance(string message)
    {
        UnityEngine.Debug.Log("[StickyLogger] " + message);
        SendToEmulator(message);
    }

    public void LogFormatInstance(string format, params object[] args)
    {
        string message = string.Format(format, args);
        UnityEngine.Debug.Log("[StickyLogger] " + message);
        SendToEmulator(message);
    }

    public void WarnInstance(string message)
    {
        UnityEngine.Debug.LogWarning("[StickyLogger] " + message);
        SendToEmulator("WARN: " + message);
    }

    public void ErrorInstance(string message)
    {
        UnityEngine.Debug.LogError("[StickyLogger] " + message);
        SendToEmulator("ERROR: " + message);
    }

    public void LogWithContextInstance(UnityEngine.Object context, string message)
    {
        UnityEngine.Debug.Log("[StickyLogger] " + message, context);
        SendToEmulator(message);
    }

    public static void Log(string message)
    {
        if (Instance != null) Instance.LogInstance(message);
        else
        {
            UnityEngine.Debug.Log("[StickyLogger] " + message);
            SendToEmulator(message);
        }
    }

    public static void LogFormat(string format, params object[] args)
    {
        if (Instance != null) Instance.LogFormatInstance(format, args);
        else
        {
            string message = string.Format(format, args);
            UnityEngine.Debug.Log("[StickyLogger] " + message);
            SendToEmulator(message);
        }
    }

    public static void Warn(string message)
    {
        if (Instance != null) Instance.WarnInstance(message);
        else
        {
            UnityEngine.Debug.LogWarning("[StickyLogger] " + message);
            SendToEmulator("WARN: " + message);
        }
    }

    public static void Error(string message)
    {
        if (Instance != null) Instance.ErrorInstance(message);
        else
        {
            UnityEngine.Debug.LogError("[StickyLogger] " + message);
            SendToEmulator("ERROR: " + message);
        }
    }

    public static void LogWithContext(UnityEngine.Object context, string message)
    {
        if (Instance != null) Instance.LogWithContextInstance(context, message);
        else
        {
            UnityEngine.Debug.Log("[StickyLogger] " + message, context);
            SendToEmulator(message);
        }
    }
}