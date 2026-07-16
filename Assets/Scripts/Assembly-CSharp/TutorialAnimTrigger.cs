using UnityEngine;

public class TutorialAnimTrigger : MonoBehaviour
{
	public TutorialTrigger Trigger;

	public bool triggerOnStart;

	private void Start()
	{
		if (triggerOnStart)
		{
			TriggerTutorial();
		}
	}

    private void TriggerTutorial()
    {
        // Ensure the GameObject/Component is active
        if (!base.gameObject.activeInHierarchy || !base.enabled)
        {
            return;
        }

        // Verify all singletons are actually instantiated before calling methods on them
        if (TutorialManager.Instance == null || TutorialMonitor.Instance == null)
        {
            // Managers aren't ready this frame. Queue it for the next frame instead.
            StartCoroutine(RetryTriggerNextFrame());
            return;
        }

        // Safe to trigger now
        TutorialMonitor.Instance.TriggerTutorial(Trigger);
    }

    private System.Collections.IEnumerator RetryTriggerNextFrame()
    {
        // Wait until the end of the frame or next frame for Awake/Start cycles to finish
        yield return new WaitForEndOfFrame();
        TriggerTutorial();
    }
    public bool WillTriggerTutorial()
	{
		if (base.gameObject.activeInHierarchy && base.enabled && TutorialManager.Instance != null)
		{
			return TutorialMonitor.Instance.ShouldTriggerTutorial(Trigger);
		}
		return false;
	}
}
