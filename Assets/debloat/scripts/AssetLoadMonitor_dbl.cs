using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// ponytail: monitors all asset loads, activations, and RAM usage for optimization analysis
public class AssetLoadMonitor_dbl : MonoBehaviour
{
	public static AssetLoadMonitor_dbl Instance;

	private List<string> loadedAssets = new List<string>();
	private List<string> activatedObjects = new List<string>();
	private int gameObjectCount = 0;
	private long initialMemory = 0;
	private StreamWriter logFile;
	private string logPath;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			initialMemory = System.GC.GetTotalMemory(false);

			// Criar arquivo de log
			logPath = Path.Combine(Application.persistentDataPath, "AssetLoadMonitor_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
			logFile = new StreamWriter(logPath, false);
			logFile.AutoFlush = true;

			LogMessage("[AssetLoadMonitor] INICIADO");
			LogMessage("Data: " + System.DateTime.Now);
			LogMessage("Caminho: " + logPath);
			LogMessage("RAM inicial: " + FormatBytes(initialMemory));
			LogMessage("");
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		LogSceneState();
	}

	private void Update()
	{
		// Log a cada 5 segundos
		if (Time.frameCount % 300 == 0)
		{
			LogMemoryUsage();
		}
	}

	private void LogSceneState()
	{
		LogMessage("");
		LogMessage("========== SCENE LOAD STATE ==========");

		// GameObjects
		GameObject[] allGOs = FindObjectsOfType<GameObject>();
		gameObjectCount = allGOs.Length;
		LogMessage("GameObjects na cena: " + gameObjectCount);

		// Transforms
		Transform[] allTransforms = FindObjectsOfType<Transform>();
		LogMessage("Transforms: " + allTransforms.Length);

		// Renderers
		Renderer[] allRenderers = FindObjectsOfType<Renderer>();
		int totalTriangles = 0;
		foreach (Renderer r in allRenderers)
		{
			Mesh mesh = null;
			if (r is MeshRenderer)
			{
				MeshFilter mf = r.GetComponent<MeshFilter>();
				if (mf != null) mesh = mf.sharedMesh;
			}
			else if (r is SkinnedMeshRenderer)
			{
				mesh = ((SkinnedMeshRenderer)r).sharedMesh;
			}
			if (mesh != null)
			{
				try
				{
					totalTriangles += mesh.triangles.Length / 3;
				}
				catch
				{
					// Mesh não tem Read/Write habilitado, ignora
				}
			}
		}
		LogMessage("Renderers: " + allRenderers.Length + " | Triangles: " + totalTriangles);

		// Audio
		AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
		LogMessage("AudioSources: " + allAudio.Length);

		// Lights
		Light[] allLights = FindObjectsOfType<Light>();
		LogMessage("Lights: " + allLights.Length);

		// Particles
		ParticleSystem[] allParticles = FindObjectsOfType<ParticleSystem>();
		LogMessage("ParticleSystems: " + allParticles.Length);

		// Animations
		Animator[] allAnimators = FindObjectsOfType<Animator>();
		LogMessage("Animators: " + allAnimators.Length);

		// UI Elements
		Canvas[] allCanvas = FindObjectsOfType<Canvas>();
		LogMessage("Canvas: " + allCanvas.Length);

		// MonoBehaviours by type
		MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
		Dictionary<string, int> scriptCount = new Dictionary<string, int>();
		foreach (MonoBehaviour script in allScripts)
		{
			string typeName = script.GetType().Name;
			if (scriptCount.ContainsKey(typeName))
				scriptCount[typeName]++;
			else
				scriptCount[typeName] = 1;
		}

		LogMessage("TOP 10 Scripts mais usados:");
		int count = 0;
		foreach (KeyValuePair<string, int> kvp in scriptCount)
		{
			if (count >= 10) break;
			LogMessage("  • " + kvp.Key + ": " + kvp.Value + "x");
			count++;
		}

		LogMemoryUsage();
		LogMessage("========== END SCENE STATE ==========");
		LogMessage("");
	}

	private void LogMemoryUsage()
	{
		long currentMemory = System.GC.GetTotalMemory(false);
		long usedMemory = currentMemory - initialMemory;

		LogMessage("RAM: " + FormatBytes(currentMemory) + " (+" + FormatBytes(usedMemory) + " desde boot)");
	}

	private void LogMessage(string message)
	{
		if (logFile != null)
		{
			logFile.WriteLine("[" + System.DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message);
			Debug.Log("[FILE] " + message);
		}
		else
		{
			Debug.Log(message);
		}
	}

	private void OnDestroy()
	{
		if (logFile != null)
		{
			LogMessage("FECHADO");
			logFile.Close();
			logFile.Dispose();
		}
	}

	private string FormatBytes(long bytes)
	{
		if (bytes < 1024) return bytes + " B";
		if (bytes < 1024 * 1024) return (bytes / 1024f).ToString("F2") + " KB";
		if (bytes < 1024 * 1024 * 1024) return (bytes / (1024f * 1024f)).ToString("F2") + " MB";
		return (bytes / (1024f * 1024f * 1024f)).ToString("F2") + " GB";
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeOnSceneLoad()
	{
		// Auto-init quando cena carrega
		if (Instance == null)
		{
			GameObject monitor = new GameObject("[AssetLoadMonitor]");
			monitor.AddComponent<AssetLoadMonitor_dbl>();
		}
	}
}
