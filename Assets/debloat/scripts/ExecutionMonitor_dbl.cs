using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// ponytail: monitors execution flow - tracks what's running, detects what stopped
public class ExecutionMonitor_dbl : MonoBehaviour
{
	private static ExecutionMonitor_dbl instance;
	private Dictionary<string, ExecutionSpan> activeSpans = new Dictionary<string, ExecutionSpan>();

	private class ExecutionSpan
	{
		public string Name;
		public float StartTime;
		public float LastUpdateTime;
		public int ExecutionCount;
		public float MaxDuration;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		// Check for stalled execution spans (nothing updated for 5+ seconds)
		float currentTime = Time.realtimeSinceStartup;
		List<string> stalledSpans = new List<string>();

		foreach (KeyValuePair<string, ExecutionSpan> kvp in activeSpans)
		{
			if (currentTime - kvp.Value.LastUpdateTime > 5f)
			{
				stalledSpans.Add(kvp.Key);
				CrashLogger_dbl logger = FindObjectOfType<CrashLogger_dbl>();
				if (logger != null)
				{
					logger.LogEvent("STALLED_EXECUTION", kvp.Value.Name + " - no update for " + (currentTime - kvp.Value.LastUpdateTime).ToString("F1") + " seconds");
				}
			}
		}

		// Clean up old spans
		foreach (string stalledName in stalledSpans)
		{
			activeSpans.Remove(stalledName);
		}
	}

	public void BeginSpan(string spanName)
	{
		if (!activeSpans.ContainsKey(spanName))
		{
			ExecutionSpan span = new ExecutionSpan();
			span.Name = spanName;
			span.StartTime = Time.realtimeSinceStartup;
			span.LastUpdateTime = span.StartTime;
			span.ExecutionCount = 0;
			span.MaxDuration = 0;
			activeSpans[spanName] = span;

			CrashLogger_dbl logger = FindObjectOfType<CrashLogger_dbl>();
			if (logger != null)
			{
				logger.LogEvent("EXECUTION_BEGIN", spanName);
			}
		}
	}

	public void EndSpan(string spanName)
	{
		if (activeSpans.ContainsKey(spanName))
		{
			ExecutionSpan span = activeSpans[spanName];
			float duration = Time.realtimeSinceStartup - span.StartTime;
			span.ExecutionCount++;
			if (duration > span.MaxDuration)
			{
				span.MaxDuration = duration;
			}

			CrashLogger_dbl logger = FindObjectOfType<CrashLogger_dbl>();
			if (logger != null)
			{
				logger.LogEvent("EXECUTION_END", spanName + " (Duration: " + duration.ToString("F3") + "s, Count: " + span.ExecutionCount + ")");
			}

			activeSpans.Remove(spanName);
		}
	}

	public void UpdateSpan(string spanName)
	{
		if (activeSpans.ContainsKey(spanName))
		{
			activeSpans[spanName].LastUpdateTime = Time.realtimeSinceStartup;
		}
	}

	public void LogAsset(string assetName, string assetType)
	{
		CrashLogger_dbl logger = FindObjectOfType<CrashLogger_dbl>();
		if (logger != null)
		{
			logger.LogEvent("ASSET_LOAD", assetType + ": " + assetName);
		}
	}

	public void LogSceneTransition(string fromScene, string toScene)
	{
		CrashLogger_dbl logger = FindObjectOfType<CrashLogger_dbl>();
		if (logger != null)
		{
			logger.LogEvent("SCENE_CHANGE", "From: " + fromScene + " → To: " + toScene);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void AutoInitialize()
	{
		if (instance == null)
		{
			GameObject monitorGO = new GameObject("[ExecutionMonitor]");
			monitorGO.AddComponent<ExecutionMonitor_dbl>();
		}
	}

	public static ExecutionMonitor_dbl GetInstance()
	{
		if (instance == null)
		{
			GameObject monitorGO = new GameObject("[ExecutionMonitor]");
			instance = monitorGO.AddComponent<ExecutionMonitor_dbl>();
		}
		return instance;
	}
}
