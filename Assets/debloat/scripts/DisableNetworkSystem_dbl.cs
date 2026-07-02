using UnityEngine;

/// ponytail: bootstrap that disables network + analytics singletons
public class DisableNetworkSystem_dbl : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeNetworkStubs()
	{
		Debug.Log("[DisableNetworkSystem_dbl] Network and analytics disabled");
		KFFNetwork2_dbl.GetInstance();
		AnalyticsManager_dbl mgr = AnalyticsManager_dbl.Instance;
	}
}
