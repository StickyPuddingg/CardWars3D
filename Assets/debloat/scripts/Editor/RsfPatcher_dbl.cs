using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// ponytail: Unity's N3DS build pipeline auto-generates Temp/StagingArea/Application.rsf
/// fresh on every build, and its default SystemControlInfo section omits SystemModeExt -
/// meaning the game never requests New 3DS extended memory (~172MB) and stays capped at the
/// Old 3DS 64MB ceiling, which is what's been causing the OOM crashes on real N3DS/Azahar.
///
/// PlayerSettings.N3DS.targetPlatform is the field that should control this, but it's
/// read-only via the public scripting API in this Unity version - it must be set once via
/// Edit > Project Settings > Player > Other Settings > Nintendo 3DS > Target Platform =
/// "New Nintendo 3DS". That's a one-time change, persisted in ProjectSettings.asset, not
/// something that needs redoing per build.
///
/// This PostProcessBuild patch is a text-level safety net for the RSF itself, but measured
/// timing showed it fires ~10ms AFTER the native makerom/3dstool packaging step already
/// consumed the RSF - so it does NOT reliably affect the built .cia. Left in place in case
/// it helps on a differently-timed build, but the real fix is the Player Settings change above.
public static class RsfPatcher_dbl
{
	private const string RsfRelativePath = "Temp/StagingArea/Application.rsf";
	private const string ExtMemLine = "  SystemModeExt: 124MB";

	[PostProcessBuild(1)]
	public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
	{
		PatchRsf();
	}

	[MenuItem("Debloat/Patch Application.rsf Now")]
	public static void PatchRsfManual()
	{
		PatchRsf();
	}

	private static void PatchRsf()
	{
		string projectRoot = Directory.GetParent(Application.dataPath).FullName;
		string rsfPath = Path.Combine(projectRoot, RsfRelativePath);

		if (!File.Exists(rsfPath))
		{
			Debug.LogWarning("[RsfPatcher_dbl] Application.rsf not found at " + rsfPath + " - nothing to patch");
			return;
		}

		string content = File.ReadAllText(rsfPath);
		if (content.Contains("SystemModeExt"))
		{
			Debug.Log("[RsfPatcher_dbl] SystemModeExt already present, skipping");
			return;
		}

		string marker = "SystemControlInfo:";
		int idx = content.IndexOf(marker);
		if (idx < 0)
		{
			Debug.LogWarning("[RsfPatcher_dbl] SystemControlInfo: section not found in Application.rsf, cannot patch");
			return;
		}

		int insertAt = idx + marker.Length;
		string patched = content.Substring(0, insertAt) + "\n" + ExtMemLine + content.Substring(insertAt);
		File.WriteAllText(rsfPath, patched);
		Debug.Log("[RsfPatcher_dbl] Patched Application.rsf with New 3DS extended memory (SystemModeExt: 124MB)");
	}
}
