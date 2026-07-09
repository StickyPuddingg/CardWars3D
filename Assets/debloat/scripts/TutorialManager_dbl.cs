using System;
using System.Collections.Generic;
using UnityEngine;

/// ponytail: inert tutorial manager - all methods return no-op or defaults
public class TutorialManager_dbl : ILoadable
{
	public Dictionary<string, TutorialInfo> tutorials = new Dictionary<string, TutorialInfo>();
	public Dictionary<TutorialTrigger, TutorialInfo> triggers = new Dictionary<TutorialTrigger, TutorialInfo>();
	public Dictionary<string, string> tweenTriggers = new Dictionary<string, string>();

	private static TutorialManager_dbl instance;

	public static TutorialManager_dbl Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TutorialManager_dbl();
			}
			return instance;
		}
	}

	public System.Collections.IEnumerator Load()
	{
		yield return null;
	}

	public void markTutorialCompleted(string tutorialID)
	{
	}

	public bool isTutorialCompleted(string tutorialID)
	{
		return true;
	}

	public bool isTutorialCompleted(TutorialTrigger trigger)
	{
		return true;
	}

	public TutorialInfo Find(string tutorialID)
	{
		return null;
	}

	public TutorialInfo Find(TutorialTrigger trigger)
	{
		return null;
	}

	public void Destroy()
	{
	}
}
