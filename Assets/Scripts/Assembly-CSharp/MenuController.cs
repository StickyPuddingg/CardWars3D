using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : ReloadHandler
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true; // Toggle this in the Unity Inspector to enable/disable logs

    private enum MenuStartupStates
    {
        Logo_Panel_Visible,
        Returning_To_Menu,
        Logging_In,
        Startup_Checklist,
        Startup_Complete
    }

    public enum MenuStates
    {
        None,
        Wait,
        Start,
        MainMenu,
        Options,
        Market,
        Gacha,
        Battle,
        Dungeon,
        Deck,
        Messages
    }

    private static MenuController g_menuController;

    public MenuStates MenuState;

    public GameObject AsyncLoaders;

    public GameObject MainLogo;

    public GameObject PlayerStats;

    public LogoPanelScript LogoPanel;

    public Transform BattleSelectCameraSnap;

    public Transform BattleSelectCameraTargetSnap;

    public Transform DungeonSelectCameraSnap;

    public Transform DungeonSelectCameraTargetSnap;

    public GameObject MainMenuCamera;

    public GameObject StartHide;

    public GameObject StartShow;

    public GameObject MainMenuHide;

    public GameObject MainMenuShow;

    public GameObject OptionsHide;

    public GameObject OptionsShow;

    public GameObject MarketHide;

    public GameObject MarketShow;

    public GameObject GachaHide;

    public GameObject GachaShow;

    public GameObject BattleHide;

    public GameObject BattleShow;

    public GameObject DungeonHide;

    public GameObject DungeonShow;

    public GameObject DeckHide;

    public GameObject DeckShow;

    public GameObject MessagesHide;

    public GameObject MessagesShow;

    public UIButtonTween CalendarShow;

    public UIButtonTween TooManyCardsShow;

    public UIButtonTween DungeonExtrasShow;

    public GameObject YouGotThis;

    public UIButtonTween ElFistoVictoryShow;

    public UIButtonTween ElFistoCompleteShow;

    public GameObject FCMapButton;

    private MenuStartupStates MenuStartupState;

    private bool mainMenuFlowActive;

    private bool gachaFlowActive;

    public bool hasAwardStuff;

    // Helper method to respect the top-level debug configuration
    private void LogDebugError(string message)
    {
            Logger.Error(message);
       
    }

    private void Awake()
    {


        // --- Place inside Awake() or Start() ---

        if (LogoPanel == null)
        {
            Logger.Error("[MenuController][Checklist] CRITICAL: 'LogoPanel' field is null in the Inspector!");
        }

        if (AsyncLoaders == null)
        {
            Logger.Error("[MenuController][Checklist] WARNING: 'AsyncLoaders' GameObject is null. Async steps will bypass immediately.");
        }
        else if (AsyncLoaders.GetComponents<AsyncLoader>().Length == 0)
        {
            Logger.Error("[MenuController][Checklist] WARNING: 'AsyncLoaders' is assigned but contains 0 AsyncLoader components.");
        }

        if (StartShow == null)
        {
            Logger.Error("[MenuController][Checklist] CRITICAL: 'StartShow' UI target is missing. Menu state progression will break.");
        }


        if (g_menuController == null)
        {
            g_menuController = this;
        }
        hasAwardStuff = false;
    }

    public static MenuController GetInstance()
    {
        return g_menuController;
    }

    private void Start()
    {
        MenuStartupState = ((GlobalFlags.Instance.ReturnToMainMenu || GlobalFlags.Instance.ReturnToBuildDeck) ? MenuStartupStates.Returning_To_Menu : MenuStartupStates.Logo_Panel_Visible);

        // El Fisto isn't in use -> Destroy the script component's immediate parent object
        if (ElFistoVictoryShow.transform.parent != null)
        {
            Object.Destroy(ElFistoVictoryShow.transform.parent.gameObject);
        }
    }

    private void Update()
    {
        if (MenuStartupState == MenuStartupStates.Startup_Complete)
        {
            return;
        }
        if (MenuStartupState == MenuStartupStates.Returning_To_Menu)
        {
            if (IsAsyncLoadComplete())
            {
                BattleResult.Menu returnMenu = BattleResult.Menu.MapMain;
                if (GlobalFlags.Instance.BattleResult != null)
                {
                    returnMenu = GlobalFlags.Instance.BattleResult.returnMenu;
                }
                else if (GlobalFlags.Instance.ReturnToBuildDeck)
                {
                    returnMenu = BattleResult.Menu.BuildDeck;
                }
                StartCoroutine(CoroutineMenuReturn(returnMenu));
                MenuStartupState = MenuStartupStates.Startup_Complete;
            }
            return;
        }
        if (GlobalFlags.Instance.ReturnToMainMenu)
        {
            if (GlobalFlags.Instance.BattleResult == null)
            {
                SwitchToBattle();
            }
            else
            {
                SwitchToMainMenu();
            }
            MenuStartupState = MenuStartupStates.Startup_Complete;
            return;
        }
        if (GlobalFlags.Instance.ReturnToBuildDeck)
        {
            SwitchToMainMenu();
            MenuStartupState = MenuStartupStates.Startup_Complete;
            return;
        }
        if (MenuStartupState == MenuStartupStates.Logo_Panel_Visible)
        {
            if (LogoPanel != null && !LogoPanel.IsComplete)
            {
                return;
            }
            LogDebugError("[MenuController] Moving to state: Logging_In");
            MenuStartupState = MenuStartupStates.Logging_In;
        }
        if (MenuStartupState == MenuStartupStates.Logging_In)
        {
            SessionManager instance2 = SessionManager.GetInstance();
            bool sessionReady = instance2.IsReady();
            bool asyncDone2 = IsAsyncLoadComplete();

            if (!sessionReady || !asyncDone2)
            {
                LogDebugError(string.Format("[MenuController] Waiting for Session. Session IsReady: {0}, AsyncLoad Done: {1}", sessionReady, asyncDone2));
                return;
            }

            LogDebugError("[MenuController] Moving to state: Startup_Checklist");
            MenuStartupState = MenuStartupStates.Startup_Checklist;
        }
        if (MenuStartupState == MenuStartupStates.Startup_Checklist)
        {
            SessionManager instance3 = SessionManager.GetInstance();

            LogDebugError(string.Format("[MenuController] Checklist evaluation -> ForcedUpdate: {0}, HasMessages: {1}", instance3.NeedsForcedUpdate, instance3.HasNewMessagesReady));

            if (instance3.HasNewMessagesReady)
            {
                StartCoroutine(SwitchToMessages(0.5f));
                MenuStartupState = MenuStartupStates.Startup_Complete;
            }
            else
            {
                LogDebugError("[MenuController] All checklist checks passed successfully! Invoking SwitchToStart(0.5f)");
                SwitchToStart(0.5f);
                MenuStartupState = MenuStartupStates.Startup_Complete;
            }
        }
    }

    public void SwitchToStart(float waitSecs = 0f)
    {
        LogDebugError(string.Format("[MenuController] SwitchToStart requested. Current MenuState: {0}", MenuState));
        if (MenuState != MenuStates.Start)
        {
            PanelManager panelMgr = PanelManager.GetInstance();
            if (panelMgr != null && panelMgr.gameObject != null && panelMgr.gameObject.activeInHierarchy)
            {
                panelMgr.StartCoroutine(SwitchToStartCoroutine(waitSecs));
            }
            else if (this.gameObject != null && this.gameObject.activeInHierarchy)
            {
                StartCoroutine(SwitchToStartCoroutine(waitSecs));
            }
            else
            {
                LogDebugError("[MenuController] Unable to start SwitchToStartCoroutine because MenuController is inactive and no active PanelManager found.");
            }
        }
    }

    private IEnumerator SwitchToStartCoroutine(float waitSecs)
    {
        LogDebugError(string.Format("[MenuController] SwitchToStartCoroutine began. Waiting for {0} seconds...", waitSecs));
        if (waitSecs > 0f)
        {
            yield return new WaitForSeconds(waitSecs);
        }

        if (enableDebugLogs) TFUtils.DebugLog("SwitchToStartCoroutine -- Getting product data", "iap");

        try
        {
            LogDebugError("[MenuController] Requesting PurchaseManager Product Data...");
            Singleton<PurchaseManager>.Instance.GetProductData(null);
            LogDebugError("[MenuController] PurchaseManager completed without throwing.");
        }
        catch (System.Exception ex)
        {
            LogDebugError("[MenuController] CRASH PREVENTED: PurchaseManager threw an exception: " + ex.Message);
        }

        LogDebugError(string.Format("[MenuController] Launching TransitionState towards StartShow target. Object present: {0}", StartShow != null));
        TransitionState(StartShow, MenuStates.Start);
    }

    private void TransitionState(GameObject target, MenuStates state)
    {
        lock (this)
        {
            string targetName = (target != null) ? target.name : "NULL";
            LogDebugError(string.Format("[MenuController] TransitionState executing: Hiding Current, Tweens Target: {0}, New State: {1}", targetName, state));

            HideCurrent();
            DoTweens(target);
            MenuState = state;
        }
    }

    private IEnumerator CoroutineMenuReturn(BattleResult.Menu returnMenu)
    {
        switch (returnMenu)
        {
            case BattleResult.Menu.BuildDeck:
                yield return StartCoroutine(CoroutineMenuReturnBuildDeck());
                break;
            case BattleResult.Menu.BattleModeSelect:
                yield return StartCoroutine(CoroutineMenuReturnBattleModeSelect());
                break;
            case BattleResult.Menu.DungeonSelect:
                yield return StartCoroutine(CoroutineMenuReturnDungeonSelect());
                break;
            default:
                yield return StartCoroutine(CoroutineMenuReturnMapMain());
                break;
        }
        if (LogoPanel != null)
        {
            LogoPanel.Complete();
        }
        if (MainLogo != null)
        {
            Destroy(MainLogo);
        }
        if (PlayerStats != null)
        {
            PlayerStats.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator CoroutineMenuReturnBuildDeck()
    {
        yield return null;
        CWDeckManagerAdditiveLoad deckManager = CWDeckManagerAdditiveLoad.GetInstance();
        if (deckManager != null)
        {
            deckManager.NavigateToDeckManager();
        }
    }

    private IEnumerator CoroutineMenuReturnBattleModeSelect()
    {
        yield return null;
        if (BattleSelectCameraSnap != null && BattleSelectCameraTargetSnap != null)
        {
            SnapWorldCamera(BattleSelectCameraSnap.position, BattleSelectCameraTargetSnap.position);
        }
        SwitchToBattle();
        yield return null;
        GlobalFlags.Instance.ReturnToMainMenu = false;
        GlobalFlags.Instance.BattleResult = null;
    }

    private IEnumerator CoroutineMenuReturnDungeonSelect()
    {
        yield return null;
        if (DungeonSelectCameraSnap != null && DungeonSelectCameraTargetSnap != null)
        {
            SnapWorldCamera(DungeonSelectCameraSnap.position, DungeonSelectCameraTargetSnap.position);
        }
        SwitchDungeon();
        yield return new WaitForSeconds(0.5f);
        SwitchToDungeonExtras();
        yield return null;
        GlobalFlags.Instance.ReturnToMainMenu = false;
        GlobalFlags.Instance.BattleResult = null;
    }

    private IEnumerator CoroutineMenuReturnMapMain()
    {
        yield return null;
        if (BattleSelectCameraSnap != null && BattleSelectCameraTargetSnap != null)
        {
            SnapWorldCamera(BattleSelectCameraSnap.position, BattleSelectCameraTargetSnap.position);
        }
        SwitchToBattle();
        CWQuestMapAdditiveLoad questMapManager = ((!(AsyncLoaders != null)) ? null : AsyncLoaders.GetComponent<CWQuestMapAdditiveLoad>());
        if (questMapManager != null)
        {
            questMapManager.NavigateToMapScene();
        }
    }

    private void SnapWorldCamera(Vector3 position, Vector3 targetPosition)
    {
        PanelManager instance = PanelManager.GetInstance();
        instance.newCamera.transform.position = position;
        instance.newCameraTarget.transform.position = targetPosition;
        CWMenuCameraTarget component = instance.newCameraTarget.GetComponent<CWMenuCameraTarget>();
        component.followFlag = true;
    }

    private bool IsAsyncLoadComplete()
    {
        if (AsyncLoaders == null)
        {
            return true;
        }
        AsyncLoader[] components = AsyncLoaders.GetComponents<AsyncLoader>();
        AsyncLoader[] array = components;
        foreach (AsyncLoader asyncLoader in array)
        {
            if (!asyncLoader.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    public void SwitchToMainMenu()
    {
        Destroy(MainLogo);
        TransitionState(MainMenuShow, MenuStates.MainMenu);
        StartCoroutine(MainMenuItems());
    }

    private IEnumerator MainMenuItems()
    {
        if (mainMenuFlowActive)
        {
            yield break;
        }
        mainMenuFlowActive = true;
        SessionManager sessionMan = SessionManager.GetInstance();
        if (null != sessionMan && sessionMan.LocalRemoteSaveGameConflict)
        {
            if (null != AuthDialogController.GetInstance())
            {
                AuthDialogController.GetInstance().DisplayAuthDialog();
            }
        }
        else
        {
            if (MenuState == MenuStates.MainMenu && ActivateCalendar())
            {
                yield break;
            }
            yield return StartCoroutine(ShowPlacement("main_menu", 1.5f));
        }
        mainMenuFlowActive = false;
    }

    public bool ActivateCalendar()
    {
        if (enableDebugLogs) TFUtils.DebugLog("CalendarCheck", "calendar");
        PlayerInfoScript instance = PlayerInfoScript.GetInstance();
        if (null != CalendarShow && null != instance && instance.HasUnclaimedCalendarGift())
        {
            if (enableDebugLogs) TFUtils.DebugLog("Will show calendar", "calendar");
            StartCoroutine(ShowCalendar());
            return true;
        }
        return false;
    }

    private IEnumerator ShowCalendar()
    {
        UICamera.useInputEnabler = true;
        yield return new WaitForSeconds(1f);
        UICamera.useInputEnabler = false;
        CalendarShow.Play(true);
        yield return null;
    }

    public void SwitchToOptions()
    {
        TransitionState(OptionsShow, MenuStates.Options);
    }

    public void SwitchToMarket()
    {
        TransitionState(MarketShow, MenuStates.Market);
    }

    public void SwitchToGacha()
    {
        PlayerInfoScript instance = PlayerInfoScript.GetInstance();
        if (TooManyCardsShow != null && instance.DeckManager.CardCount() >= instance.MaxInventory)
        {
            TooManyCardsShow.Play(true);
            return;
        }
        TransitionState(GachaShow, MenuStates.Gacha);
        StartCoroutine(GachaItems());
    }

    private IEnumerator GachaItems()
    {
        if (!gachaFlowActive)
        {
            gachaFlowActive = true;
            yield return StartCoroutine(ShowPlacement("game_gacha", 1f));
            gachaFlowActive = false;
        }
    }

    private IEnumerator ShowPlacement(string placement, float waitSecs = 0f)
    {
        PlayerInfoScript pinfo = PlayerInfoScript.GetInstance();
        if (!(null == pinfo) && pinfo.IsCalendarUnlocked())
        {
            UICamera.useInputEnabler = true;
            yield return KFFRequestorController.GetInstance().ShowContentCoroutine(placement, waitSecs);
            UICamera.useInputEnabler = false;
            KFFUpsightVGController upsightController = KFFUpsightVGController.GetInstance();
            while (upsightController != null && upsightController.IsPlacementInProgress)
            {
                yield return null;
            }
        }
    }

    public void SwitchToBattle()
    {
        TransitionState(BattleShow, MenuStates.Battle);
        StartCoroutine(AwardStuff());
    }

    private IEnumerator AwardStuff()
    {
        hasAwardStuff = true;
        yield return StartCoroutine(AwardFCDemoCards());
        yield return StartCoroutine(AwardElFistoCards());
    }

    private IEnumerator AwardFCDemoCards()
    {
        PlayerInfoScript pinfo = PlayerInfoScript.GetInstance();
        if (null == pinfo)
        {
            yield break;
        }
        if (pinfo.HasCompletedFCDemo())
        {
            yield return new WaitForSeconds(1f);
            if (!pinfo.HasReceivedFCCards())
            {
                if (null == YouGotThis)
                {
                    yield break;
                }
                YouGotThisController youGotThisController = YouGotThis.GetComponent<YouGotThisController>();
                if (null == youGotThisController)
                {
                    yield break;
                }
                yield return StartCoroutine(youGotThisController.AwardLeader("Leader_Fionna"));
                yield return StartCoroutine(youGotThisController.AwardLeader("Leader_Cake"));
                PlayerInfoScript.GetInstance().SetHasReceivedFCCards();
                Singleton<AnalyticsManager>.Instance.LogFCHeroesAwarded();
                yield return new WaitForSeconds(2.5f);
            }
            if (!pinfo.HasSeenFCUpsellScreen() && null != FCMapButton)
            {
                FCMapButton.SendMessage("OnClick");
                pinfo.SetHasSeenFCUpsellScreen();
            }
        }
        yield return null;
    }

    public IEnumerator AwardSideQuestCards(SideQuestData sqd)
    {
        YouGotThisController youGotThisController = YouGotThis.GetComponent<YouGotThisController>();
        if (!(null == youGotThisController))
        {
            yield return StartCoroutine(youGotThisController.AwardCard(sqd.RewardID));
        }
    }

    public IEnumerator AwardCard(string cardID)
    {
        YouGotThisController youGotThisController = YouGotThis.GetComponent<YouGotThisController>();
        if (!(null == youGotThisController))
        {
            if (cardID.StartsWith("Leader"))
            {
                yield return StartCoroutine(youGotThisController.AwardLeader(cardID));
            }
            else
            {
                yield return StartCoroutine(youGotThisController.AwardCard(cardID));
            }
        }
    }

    private IEnumerator AwardElFistoCards()
    {
        PlayerInfoScript pinfo = PlayerInfoScript.GetInstance();
        if (!(null == pinfo))
        {
            ElFistoController efc2 = base.gameObject.AddComponent<ElFistoController>();
            if (efc2.ShouldAward())
            {
                UICamera.useInputEnabler = true;
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(efc2.DisplayAward(YouGotThis));
                yield return new WaitForSeconds(2.5f);
                yield return StartCoroutine(efc2.DisplayElFisto(ElFistoVictoryShow, ElFistoCompleteShow));
                UICamera.useInputEnabler = false;
            }
            Object.Destroy(efc2);
            efc2 = null;
            yield return null;
            hasAwardStuff = false;
        }
    }

    public void SwitchDungeon()
    {
        TransitionState(DungeonShow, MenuStates.Dungeon);
    }

    public void SwitchToDungeonExtras()
    {
        if (null != DungeonExtrasShow && MenuState == MenuStates.Dungeon)
        {
            DungeonExtrasShow.Play(true);
        }
    }

    public void SwitchToDeck()
    {
        TransitionState(DeckShow, MenuStates.Deck);
    }

    public override void SwitchToReload()
    {
        // Redirecting safety fallback to Main Menu state because explicit reload layout was requested out.
        TransitionState(MainMenuShow, MenuStates.MainMenu);
    }

    public void SwitchToDeckBuild()
    {
        TransitionState(MenuStates.Deck);
    }

    public void SwitchToQuit()
    {
        TransitionState(MenuStates.None);
    }

    private IEnumerator SwitchToMessages(float wait = 0f)
    {
        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }
        TransitionState(MessagesShow, MenuStates.Messages);
    }

    private void TransitionState(MenuStates state)
    {
        TransitionState(null, state);
    }

    private void HideCurrent()
    {
        switch (MenuState)
        {
            case MenuStates.Start:
                DoTweens(StartHide);
                break;
            case MenuStates.MainMenu:
                DoTweens(MainMenuHide);
                break;
            case MenuStates.Options:
                DoTweens(OptionsHide);
                break;
            case MenuStates.Market:
                DoTweens(MarketHide);
                break;
            case MenuStates.Gacha:
                DoTweens(GachaHide);
                break;
            case MenuStates.Battle:
                DoTweens(BattleHide);
                break;
            case MenuStates.Dungeon:
                DoTweens(DungeonHide);
                break;
            case MenuStates.Deck:
                DoTweens(DeckHide);
                break;
            case MenuStates.Messages:
                DoTweens(MessagesHide);
                break;
        }

        List<GameObject> list = new List<GameObject>();
        if (MenuState != MenuStates.Start && StartHide != null) list.Add(StartHide);
        if (MenuState != MenuStates.MainMenu && MainMenuHide != null) list.Add(MainMenuHide);
        if (MenuState != MenuStates.Options && OptionsHide != null) list.Add(OptionsHide);
        if (MenuState != MenuStates.Market && MarketHide != null) list.Add(MarketHide);
        if (MenuState != MenuStates.Gacha && GachaHide != null) list.Add(GachaHide);
        if (MenuState != MenuStates.Battle && BattleHide != null) list.Add(BattleHide);
        if (MenuState != MenuStates.Dungeon && DungeonHide != null) list.Add(DungeonHide);
        if (MenuState != MenuStates.Deck && DeckHide != null) list.Add(DeckHide);
        if (MenuState != MenuStates.Messages && MessagesHide != null) list.Add(MessagesHide);

        foreach (GameObject item in list)
        {
            UIButtonTween[] components = item.GetComponents<UIButtonTween>();

            foreach (UIButtonTween uIButtonTween in components)
            {
                if (uIButtonTween.tweenTarget == null)
                {
                    uIButtonTween.tweenTarget = uIButtonTween.gameObject;
                }

                string tgtName = (uIButtonTween.tweenTarget != null) ? uIButtonTween.tweenTarget.name : "NULL";
                LogDebugError(string.Format("[MenuController] Hiding tweenTarget: {0} (source: {1}) from MenuState: {2}", tgtName, item != null ? item.name : "NULL", MenuState));

                bool isOwner = false;

                if (isOwner)
                {
                    LogDebugError(string.Format("[MenuController] warning deactivation of {0} because it contains MenuController.", tgtName));
                    continue;
                }

                if (tgtName == "Menu UI Panel")
                {
                    LogDebugError("[MenuController] Detected deactivation of 'Menu UI Panel' here.");
                }

                NGUITools.SetActive(uIButtonTween.tweenTarget, false);
            }
        }
    }

    private void DoTweens(GameObject obj)
    {
        if (obj != null)
        {
            UIButtonTween[] componentsInChildren = obj.GetComponentsInChildren<UIButtonTween>(true);
            UIButtonTween[] array = componentsInChildren;
            foreach (UIButtonTween uIButtonTween in array)
            {
                uIButtonTween.Play(true);
            }
        }
    }

    private void SpawnEnvironmentPrefab(string scheduleCategory)
    {
        List<ScheduleData> itemsAvailableAndUnlocked = ScheduleDataManager.Instance.GetItemsAvailableAndUnlocked(scheduleCategory, TFUtils.ServerTime.Ticks);
        foreach (ScheduleData item in itemsAvailableAndUnlocked)
        {
            if (TryLoadEnvironmentPrefab(item.ID))
            {
                break;
            }
        }
    }

    private bool TryLoadEnvironmentPrefab(string prefab)
    {
        string path = "Environment/" + prefab;
        Object @object = SLOTGameSingleton<SLOTResourceManager>.GetInstance().LoadResource(path);
        if (@object != null)
        {
            Object.Instantiate(@object);
            return true;
        }
        return false;
    }
}