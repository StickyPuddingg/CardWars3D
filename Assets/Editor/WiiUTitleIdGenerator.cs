using UnityEngine;
using UnityEditor;

public class WiiUTitleIdGeneratorWindow : EditorWindow
{
    public enum WiiURegion
    {
        USA = 0x00,
        EUR = 0x01,
        JPN = 0x02
    }

    private bool isBaseGame = true;
    private WiiURegion region = WiiURegion.USA;
    private string generatedTitleId = "";

    // Añade una opción en el menú superior de Unity: Window > Wii U Title ID Generator
    [MenuItem("Window/Wii U Title ID Generator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(WiiUTitleIdGeneratorWindow), false, "Wii U ID Gen");
    }

    // Dibuja la interfaz de la ventana flotante
    private void OnGUI()
    {
        GUILayout.Label("Configuración de Title ID (Patrón Retail)", EditorStyles.boldLabel);

        // Opciones de configuración
        isBaseGame = EditorGUILayout.Toggle("¿Es Juego Base?", isBaseGame);
        region = (WiiURegion)EditorGUILayout.EnumPopup("Región:", region);

        GUILayout.Space(10);

        // Botón para ejecutar la lógica
        if (GUILayout.Button("Generar Title ID"))
        {
            generatedTitleId = GenerateRandomTitleID(isBaseGame, region);
        }

        GUILayout.Space(15);

        // Muestra el resultado si ya se generó uno
        if (!string.IsNullOrEmpty(generatedTitleId))
        {
            EditorGUILayout.TextField("Title ID Generado:", generatedTitleId);

            if (GUILayout.Button("Copiar al Portapapeles"))
            {
                EditorGUIUtility.systemCopyBuffer = generatedTitleId;
                Debug.Log(string.Format("[Wii U] Copiado al portapapeles: {0}", generatedTitleId));
            }
        }
    }

    private static string GenerateRandomTitleID(bool isBase, WiiURegion reg)
    {
        // 1. Primer Bloque (High Title ID)
        string highTitleId = isBase ? "00050000" : "0005000c";

        // 2. Bloque Intermedio Fijo (Patrón Comercial/Retail)
        string segmentoretail = "104";

        // 3. ID Único del Juego (3 dígitos hexadecimales aleatorios: 000 a FFF)
        // Rango en decimal: 0 a 4095 (0xFFF). En Random.Range el máximo es exclusivo, por eso 4096.
        int randomProductNum = Random.Range(0, 4096);
        string idUnicoJuego = randomProductNum.ToString("X3");

        // 4. Código de Región (Últimos 2 dígitos)
        string regionId = ((int)reg).ToString("X2");

        // Combinamos todo respetando la estructura real que observaste
        return (highTitleId + segmentoretail + idUnicoJuego + regionId).ToLower();
    }
}