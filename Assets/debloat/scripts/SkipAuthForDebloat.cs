using UnityEngine;

/// ponytail: bypass auth screen for debloat - sets auth as already started
public class SkipAuthForDebloat : MonoBehaviour
{
	private void Awake()
	{
		AuthScreenController.AuthStarted = true;
	}
}
