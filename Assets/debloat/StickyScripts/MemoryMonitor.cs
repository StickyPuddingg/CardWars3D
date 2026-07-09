using UnityEngine;
using System;

public class MemoryMonitor : MonoBehaviour
{
    public static MemoryMonitor Instance;

    // =========================
    // CONFIG
    // =========================
    public float updateInterval = 0.25f;
    public bool hideMonitor = false;

    // =========================
    // MEMORY CACHE
    // =========================
    private long lastHeap;
    private long lowestSystem;
    private long lowestDevice;
    private long lowestVRAMA;
    private long lowestVRAMB;

    private float nextUpdate;

    // =========================
    // INIT
    // =========================
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize lowest watermarks
        lowestSystem = GetSystemFree();
        lowestDevice = GetDeviceFree();
        lowestVRAMA = GetVRAMA();
        lowestVRAMB = GetVRAMB();
        lastHeap = GC.GetTotalMemory(false);

        nextUpdate = Time.realtimeSinceStartup;
    }

    // =========================
    // UPDATE LOOP
    // =========================
    void Update()
    {
        float t = Time.realtimeSinceStartup;

        if (t >= nextUpdate)
        {
            nextUpdate = t + updateInterval;

            long system = GetSystemFree();
            long device = GetDeviceFree();
            long vramA = GetVRAMA();
            long vramB = GetVRAMB();

            if (system < lowestSystem) lowestSystem = system;
            if (device < lowestDevice) lowestDevice = device;
            if (vramA < lowestVRAMA) lowestVRAMA = vramA;
            if (vramB < lowestVRAMB) lowestVRAMB = vramB;

            lastHeap = GC.GetTotalMemory(false);
        }
    }

    // =========================
    // MEMORY ACCESS (3DS API)
    // =========================
    long GetSystemFree() { return UnityEngine.N3DS.Debug.GetSystemFree(); }
    long GetDeviceFree() { return UnityEngine.N3DS.Debug.GetDeviceFree(); }
    long GetVRAMA() { return UnityEngine.N3DS.Debug.GetVRAMAFree(); }
    long GetVRAMB() { return UnityEngine.N3DS.Debug.GetVRAMBFree(); }

    // =========================
    // ON GUI (REFERENCE LAYOUT)
    // =========================

    [GUITarget(1)]
    void OnGUI()
    {
        hideMonitor = GUI.Toggle(new Rect(10, 10, 80, 20), hideMonitor, "Minimize");
        if (hideMonitor) return;

        // Base rectangle to begin our stack (Matching your working reference layout)
        Rect rect = new Rect(10, 35, 250, 20);

        long system = GetSystemFree();
        long device = GetDeviceFree();
        long vramA = GetVRAMA();
        long vramB = GetVRAMB();

        // Standard metrics
        GUI.Label(rect, "SYS  : " + (system / 1048576f).ToString("F2") + " MB");
        rect.y += 18;
        GUI.Label(rect, "DEV  : " + (device / 1048576f).ToString("F2") + " MB");
        rect.y += 18;
        GUI.Label(rect, "HEAP : " + (lastHeap / 1048576f).ToString("F2") + " MB");
        rect.y += 18;
        GUI.Label(rect, "VRA  : " + (vramA / 1024f).ToString("F0") + " KB");
        rect.y += 18;
        GUI.Label(rect, "VRB  : " + (vramB / 1024f).ToString("F0") + " KB");

        // Spacer before lowest values
        rect.y += 24;
        GUI.Label(rect, "--- HISTORICAL LOWS ---");

        rect.y += 18;
        GUI.Label(rect, "L-SYS: " + (lowestSystem / 1048576f).ToString("F2") + " MB");
        rect.y += 18;
        GUI.Label(rect, "L-DEV: " + (lowestDevice / 1048576f).ToString("F2") + " MB");
        rect.y += 18;
        GUI.Label(rect, "L-VRA: " + (lowestVRAMA / 1024f).ToString("F0") + " KB");
        rect.y += 18;
        GUI.Label(rect, "L-VRB: " + (lowestVRAMB / 1024f).ToString("F0") + " KB");
    }
}