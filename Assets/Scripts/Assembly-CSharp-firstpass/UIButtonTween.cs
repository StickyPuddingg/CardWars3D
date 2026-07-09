using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public string label;

	public GameObject tweenTarget;

	public int tweenGroup;

	public Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public DisableCondition deleteWhenFinished;

    public bool includeChildren;

	public GameObject eventReceiver;

	public string callWhenFinished;

	public UITweener.OnFinished onFinished;

	private UITweener[] mTweens;

	private bool mStarted;

	private bool mHighlighted;

	private bool mLastPlayWasForward = true;

	private void Start()
	{
		mStarted = true;

    }

    private void OnEnable()
	{
		if (mStarted && mHighlighted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled)
		{
			if (trigger == Trigger.OnHover || (trigger == Trigger.OnHoverTrue && isOver) || (trigger == Trigger.OnHoverFalse && !isOver))
			{
				Play(isOver);
			}
			mHighlighted = isOver;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && (trigger == Trigger.OnPress || (trigger == Trigger.OnPressTrue && isPressed) || (trigger == Trigger.OnPressFalse && !isPressed)))
		{
			Play(isPressed);
		}
	}

	private void OnClick()
	{
		if (base.enabled && trigger == Trigger.OnClick)
		{
			Play(true);
		}
	}

	private void OnDoubleClick()
	{
		if (base.enabled && trigger == Trigger.OnDoubleClick)
		{
			Play(true);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (base.enabled && (trigger == Trigger.OnSelect || (trigger == Trigger.OnSelectTrue && isSelected) || (trigger == Trigger.OnSelectFalse && !isSelected)))
		{
			Play(true);
		}
	}

	private void OnActivate(bool isActive)
	{
		if (base.enabled && (trigger == Trigger.OnActivate || (trigger == Trigger.OnActivateTrue && isActive) || (trigger == Trigger.OnActivateFalse && !isActive)))
		{
			Play(isActive);
		}
	}

	private void Update()
	{
		if (disableWhenFinished == DisableCondition.DoNotDisable && deleteWhenFinished == DisableCondition.DoNotDisable || mTweens == null)
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (uITweener.enabled)
				{
					flag = false;
					break;
				}
				if (uITweener.tweenGroup == tweenGroup && uITweener.direction != (Direction)disableWhenFinished)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			if (flag2)
			{
				NGUITools.SetActive(tweenTarget, false);
			}
			if (ShouldDeleteTarget())
			{
				// -s Destroy the tween target once the configured finish direction is reached.
				GameObject targetToDelete = ((!(tweenTarget == null)) ? tweenTarget : base.gameObject);
				if (targetToDelete != null)
				{
					Destroy(targetToDelete);
				}
				// -s If the tween component still exists, destroy its owning GameObject too.
				if (this != null && gameObject != null)
				{
					Destroy(gameObject);
				}
			}
			mTweens = null;
		}
	}

	private bool ShouldDeleteTarget()
	{
		if (deleteWhenFinished == DisableCondition.DoNotDisable)
		{
			return false;
		}
		if (deleteWhenFinished == DisableCondition.DisableAfterForward)
		{
			return mLastPlayWasForward;
		}
		if (deleteWhenFinished == DisableCondition.DisableAfterReverse)
		{
			return !mLastPlayWasForward;
		}
		return false;
	}

	public void Play(bool forward)
	{
		GameObject gameObject = ((!(tweenTarget == null)) ? tweenTarget : base.gameObject);
		if (!NGUITools.GetActive(gameObject))
		{
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay)
			{
				return;
			}
			NGUITools.SetActive(gameObject, true);
		}
		mTweens = ((!includeChildren) ? gameObject.GetComponents<UITweener>() : gameObject.GetComponentsInChildren<UITweener>());
		if (mTweens.Length == 0)
		{
			if (disableWhenFinished != 0)
			{
				NGUITools.SetActive(tweenTarget, false);
			}
			return;
		}
		bool flag = false;
		mLastPlayWasForward = forward;
		if (playDirection == Direction.Reverse)
		{
			mLastPlayWasForward = !forward;
			forward = !forward;
		}
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (!flag && !NGUITools.GetActive(gameObject))
				{
					flag = true;
					NGUITools.SetActive(gameObject, true);
				}
				if (playDirection == Direction.Toggle)
				{
					uITweener.Toggle();
				}
				else
				{
					uITweener.Play(forward);
				}
				if (resetOnPlay)
				{
					uITweener.Reset();
				}
				uITweener.onFinished = onFinished;
				if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
				{
					uITweener.eventReceiver = eventReceiver;
					uITweener.callWhenFinished = callWhenFinished;
				}
				uITweener.label = label;
			}
		}
	}
}
