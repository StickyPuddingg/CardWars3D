using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

/// ponytail: execution tracker + crash logger - logs all activity, detects what stopped working
public class CrashLogger_dbl : MonoBehaviour
{
	private static CrashLogger_dbl instance;
	private StreamWriter logFile;
	private string logPath;
	private Queue<string> eventBuffer = new Queue<string>();
	private const int MAX_BUFFER_SIZE = 1000;
	private long lastFrameMemory;
	private int lastFrameCount;
	private float lastTimeScale;
	private bool hasWarningsThisFrame;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
			InitializeLogging();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void InitializeLogging()
	{
		// Save to 3DS game data directory (inside emulator) - distinct name from Azahar's own native log
		logPath = Path.Combine(Application.persistentDataPath, "cardwars_debloat_log.txt");

		try
		{
			logFile = new StreamWriter(logPath, true);
			logFile.AutoFlush = true;
		}
		catch (System.Exception ex)
		{
			Debug.LogError("[CrashLogger] Failed to create log: " + ex.Message);
			logFile = null;
			return;
		}

		LogEvent("CRASH_LOGGER_STARTED", "CrashLogger initialized");
		LogEvent("DEVICE_INFO", "Device: " + SystemInfo.deviceModel + " | OS: " + SystemInfo.operatingSystem);
		LogEvent("MEMORY_INFO", "Total RAM: " + (SystemInfo.systemMemorySize) + " MB");

		Application.logMessageReceived += OnLogMessageReceived;
		lastFrameMemory = System.GC.GetTotalMemory(false);
		lastTimeScale = Time.timeScale;
	}

	private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception)
		{
			LogEvent("CRITICAL_" + type.ToString().ToUpper(), condition);
			if (!string.IsNullOrEmpty(stackTrace))
			{
				string[] lines = stackTrace.Split('\n');
				foreach (string line in lines)
				{
					if (!string.IsNullOrEmpty(line.Trim()))
					{
						LogEvent("STACKTRACE", line.Trim());
					}
				}
			}
		}
		else if (type == LogType.Warning)
		{
			hasWarningsThisFrame = true;
		}
	}

	private void Update()
	{
		// Detect freezes/pauses
		if (Time.timeScale != lastTimeScale)
		{
			LogEvent("TIME_SCALE_CHANGED", "From " + lastTimeScale + " to " + Time.timeScale);
			lastTimeScale = Time.timeScale;
		}

		// Detect memory spikes
		long currentMemory = System.GC.GetTotalMemory(false);
		long delta = currentMemory - lastFrameMemory;
		if (Math.Abs(delta) > 1024 * 1024 * 5) // 5MB spike
		{
			LogEvent("MEMORY_SPIKE", "Delta: " + (delta / 1024f / 1024f).ToString("F2") + " MB | Total: " + (currentMemory / 1024f / 1024f).ToString("F2") + " MB");
		}
		lastFrameMemory = currentMemory;

		// Log every 10 frames (~6x per second at 60 FPS) for tighter crash timing resolution
		if (Time.frameCount % 10 == 0)
		{
			int fps = (int)(1f / Time.deltaTime);
			LogEvent("ALIVE", "Frame: " + Time.frameCount + " | FPS: " + fps + " | GC: " + (currentMemory / 1024f / 1024f).ToString("F1") + " MB");
			hasWarningsThisFrame = false;
		}

		// GameObject count less frequently - it's an expensive scan
		if (Time.frameCount % 120 == 0)
		{
			GameObject[] allGOs = FindObjectsOfType<GameObject>();
			LogEvent("GOs", "Count: " + allGOs.Length);
		}
	}

	private void OnDisable()
	{
		LogEvent("CRASH_LOGGER_DISABLED", "CrashLogger is being disabled - check crash!");
	}

	private void OnDestroy()
	{
		LogEvent("CRASH_LOGGER_DESTROYED", "CrashLogger destroyed - application ending or scene change");
		if (logFile != null)
		{
			logFile.WriteLine("\n========== FINAL BUFFER DUMP ==========");
			foreach (string evt in eventBuffer)
			{
				logFile.WriteLine(evt);
			}
			logFile.WriteLine("========== END BUFFER ==========\n");
			logFile.Close();
			logFile.Dispose();
		}
	}

	public void LogEvent(string category, string message)
	{
		string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
		string logEntry = "[" + timestamp + "] [" + category + "] " + message;

		if (logFile != null)
		{
			try
			{
				logFile.WriteLine(logEntry);
			}
			catch
			{
				// Silently fail if file write fails
			}
		}

		// Keep circular buffer
		if (eventBuffer.Count >= MAX_BUFFER_SIZE)
		{
			eventBuffer.Dequeue();
		}
		eventBuffer.Enqueue(logEntry);

		Debug.Log("[CrashLog] " + logEntry);
	}

	public void LogException(System.Exception ex)
	{
		LogEvent("EXCEPTION_CAUGHT", ex.GetType().Name + ": " + ex.Message);
		LogEvent("EXCEPTION_SOURCE", ex.Source);
		string[] stackLines = ex.StackTrace.Split('\n');
		foreach (string line in stackLines)
		{
			if (!string.IsNullOrEmpty(line.Trim()))
			{
				LogEvent("EXCEPTION_STACK", line.Trim());
			}
		}
	}

	public string GetLogPath()
	{
		return logPath;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void AutoInitialize()
	{
		if (instance == null)
		{
			GameObject loggerGO = new GameObject("[CrashLogger]");
			loggerGO.AddComponent<CrashLogger_dbl>();
		}
	}
}
