using UnityEngine;
using System.Reflection;

/// ponytail: bootstrap script that replaces tutorial singletons with inert versions
public class DisableTutorialSystem_dbl : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeTutorialStubs()
	{
		// Force TutorialManager_dbl as singleton BEFORE any scene tries to use TutorialManager
		TutorialManager_dbl.Instance.triggers.Clear();
		TutorialManager_dbl.Instance.tutorials.Clear();

		// Log that tutorial system is disabled
		Debug.Log("[DisableTutorialSystem_dbl] Tutorial system disabled - using inert stubs");
	}
}
