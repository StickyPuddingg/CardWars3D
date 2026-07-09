using System;
using System.Collections.Generic;
using UnityEngine;

/// ponytail: disabled analytics - all methods are no-op
public class AnalyticsManager_dbl : Singleton<AnalyticsManager_dbl>
{
	public void LogEvent(string category, string eventName)
	{
	}

	public void LogEvent(string category, string eventName, string label)
	{
	}

	public void LogEvent(string category, string eventName, Dictionary<string, object> properties)
	{
	}

	public void LogEvent(string category, string eventName, string label, Dictionary<string, object> properties)
	{
	}

	public void LogEvent(string category, string eventName, int value)
	{
	}

	public void LogEvent(string category, string eventName, string label, int value)
	{
	}

	public void LogEvent(string category, string eventName, float value)
	{
	}

	public void LogEvent(string category, string eventName, string label, float value)
	{
	}

	public void LogEvent(string category, string eventName, string label, string param, object paramValue)
	{
	}

	public void LogEvent(string category, string eventName, Dictionary<string, object> properties, int value)
	{
	}

	public void LogEvent(string category, string eventName, string label, Dictionary<string, object> properties, int value)
	{
	}

	public void LogCrashEvent(Exception ex)
	{
	}

	public void LogCrashEvent(string crashMsg)
	{
	}

	public void SetUserProperty(string propertyName, object propertyValue)
	{
	}

	public void InitializeAnalytics()
	{
	}

	public void SetUniqueDeviceID(string deviceID)
	{
	}

	public string GetUniqueDeviceID()
	{
		return "";
	}

	public void SetUserID(string userID)
	{
	}

	public string GetUserID()
	{
		return "";
	}

	public void SetUserLevel(int level)
	{
	}

	public void OnGameStart()
	{
	}

	public void OnGameEnd()
	{
	}
}
