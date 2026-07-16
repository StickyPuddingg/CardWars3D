using UnityEngine;

/// ponytail: tutorial disabled
public class TutorialAnimTrigger_dbl : MonoBehaviour
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
        // DISABLED
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