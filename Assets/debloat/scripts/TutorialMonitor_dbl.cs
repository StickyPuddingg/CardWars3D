using UnityEngine;

/// ponytail: inert tutorial monitor - triggers return false, no popups shown
public class TutorialMonitor_dbl : MonoBehaviour
{
	public GameObject landscapeCardTutorialText;
	public GameObject playersFirstTurnLabel;
	public TutorialBattleRing attackTutorialRing;
	public TutorialBattleRing defendTutorialRing;
	public UITweener MainMenuTweener;
	public CWCardTutorial cardTutorial;

	private static TutorialMonitor_dbl instance;

	public bool PopupActive { get; set; }

	public static TutorialMonitor_dbl Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public bool TriggerTutorial(TutorialTrigger trigger)
	{
		return false;
	}

	public bool TriggerTutorial(TutorialInfo info)
	{
		return false;
	}

	public bool ShouldTriggerTutorial(TutorialTrigger trigger)
	{
		return false;
	}

	public bool ShouldTriggerTutorial(TutorialTrigger trigger, bool checkCompleted)
	{
		return false;
	}

	public void QueueTutorial(TutorialTrigger trigger)
	{
	}

	public void ToDeckManager()
	{
	}

	public void DisableBattleRings()
	{
	}

	public void OnStartTutorial(TutorialInfo info)
	{
	}

	public void StartTutorial(TutorialInfo info)
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void AutoInitialize()
	{
		if (instance == null)
		{
			GameObject go = new GameObject("[TutorialMonitor_dbl]");
			instance = go.AddComponent<TutorialMonitor_dbl>();
			Object.DontDestroyOnLoad(go);
		}
	}
}
