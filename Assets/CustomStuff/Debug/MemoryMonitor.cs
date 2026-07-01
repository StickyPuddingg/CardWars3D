using UnityEngine;
using System;

public class MemoryMonitor : MonoBehaviour
{
    long lastSystem;

    void Update()
    {
        long current = UnityEngine.N3DS.Debug.GetSystemFree();

        if (current < lastSystem)
        {
            long delta = lastSystem - current;

            if (delta > 1024 * 1024)
            {
                Debug.Log(
                    "[MEM SPIKE] -" +
                    (delta / 1024f / 1024f).ToString("F2") +
                    " MB");
            }
        }

        lastSystem = current;
    }

    void OnGUI()
    {
        GUILayout.Label(
            "System Free: " +
            (UnityEngine.N3DS.Debug.GetSystemFree() / 1024f / 1024f).ToString("F2") +
            " MB");

        GUILayout.Label(
            "VRAM A Free: " +
            (UnityEngine.N3DS.Debug.GetVRAMAFree() / 1024f / 1024f).ToString("F2") +
            " MB");

        GUILayout.Label(
            "VRAM B Free: " +
            (UnityEngine.N3DS.Debug.GetVRAMBFree() / 1024f / 1024f).ToString("F2") +
            " MB");

        GUILayout.Label(
            ".NET Heap: " +
            (GC.GetTotalMemory(false) / 1024f / 1024f).ToString("F2") +
            " MB");
    }
}