using UnityEngine;

/// ponytail: disabled login - local save only
public class SLOTLogin_dbl : MonoBehaviour
{
	public NextScreenScript nextScript;
	public GameObject name_label;
	public GameObject continue_button;
	public GameObject back_button;
	public GameObject status_text;
	public GameObject newPlayerDestination;
	public GameObject oldPlayerDestination;
	public UILabel StatusLabel;

	private void OnClick()
	{
		// DISABLED - no online login in debloat mode
		Debug.Log("[SLOTLogin_dbl] Login disabled - local save only");
	}

	private void OnReady()
	{
	}
}
