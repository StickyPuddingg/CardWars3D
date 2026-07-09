using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager
{
    public delegate void OnFinishedDelegate();

    private const float PROCESSTIME_MIN = 0.015f;
    private static LoadingManager instance;
    private static float lastYieldTime;
    private static float lastProcessTime = -1f;

    private List<ILoadable> LoadableList;

    public static LoadingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LoadingManager();
            }
            return instance;
        }
    }

    private LoadingManager()
    {

        LoadableList = new List<ILoadable>();
        LoadableList.Add(ParametersManager.Instance);
        LoadableList.Add(FactionManager.Instance);
        LoadableList.Add(CharacterDataManager.Instance);
        LoadableList.Add(CardDataManager.Instance);
        LoadableList.Add(CardBoxManager.Instance);
        LoadableList.Add(AIDeckManager.Instance);
        LoadableList.Add(QuestManager.Instance);
        LoadableList.Add(SideQuestManager.Instance);
        LoadableList.Add(XPManager.Instance);
        LoadableList.Add(RankManager.Instance);
        LoadableList.Add(FusionManager.Instance);
        LoadableList.Add(GachaManager.Instance);
        LoadableList.Add(LeaderManager.Instance);
        LoadableList.Add(QuestConditionManager.Instance);
        LoadableList.Add(RPSMatrix.Instance);
        LoadableList.Add(TutorialManager.Instance);
        LoadableList.Add(VOManager.Instance);
        LoadableList.Add(TipManager.Instance);
        LoadableList.Add(Singleton<TournamentManager>.Instance);
        LoadableList.Add(Singleton<CodeRedemptionManager>.Instance);
        LoadableList.Add(DungeonDataManager.Instance);
        LoadableList.Add(DailyGiftDataManager.Instance);
        LoadableList.Add(CalendarGiftDataManager.Instance);
        LoadableList.Add(ScheduleDataManager.Instance);
        LoadableList.Add(KeyRingDataManager.Instance);
        LoadableList.Add(VirtualGoodsDataManager.Instance);
        LoadableList.Add(DropProfileDataManager.Instance);
        LoadableList.Add(ElFistoDataManager.Instance);
    }

    public static bool ShouldYield()
    {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        if (lastProcessTime < 0f)
        {
            lastProcessTime = realtimeSinceStartup;
            return false;
        }
        float num = realtimeSinceStartup - lastProcessTime;
        float num2 = 0.01f;
        bool flag = num >= 0.015f && realtimeSinceStartup - lastYieldTime > num2;
        if (flag)
        {
            lastYieldTime = realtimeSinceStartup;
            lastProcessTime = -1f;
        }
        return flag;
    }

    public void Add(ILoadable item)
    {
        LoadableList.Add(item);
    }

    public IEnumerator LoadAll(OnFinishedDelegate callback)
    {
        Logger.Error("[LoadingManager] LoadAll sequence started. Total managers to load: " + LoadableList.Count);

        for (int i = 0; i < LoadableList.Count; i++)
        {
            ILoadable ldr = LoadableList[i];
            if (ldr != null)
            {
                string managerName = ldr.GetType().ToString();
                Logger.Error(string.Format("[LoadingManager] [{0}/{1}] STEP START: Loading {2}...", (i + 1), LoadableList.Count, managerName));

                IEnumerator current = null;
                try
                {
                    current = ldr.Load();
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("[LoadingManager] CRITICAL EXCEPTION getting IEnumerator from {0}: {1}\n{2}", managerName, ex.Message, ex.StackTrace));
                    continue; // Skip to next manager if gathering the iterator crashes
                }

                if (current != null)
                {
                    bool hasNext = true;
                    int iterations = 0;

                    while (hasNext)
                    {
                        try
                        {
                            hasNext = current.MoveNext();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(string.Format("[LoadingManager] CRITICAL EXCEPTION inside {0}.Load() MoveNext loop: {1}\n{2}", managerName, ex.Message, ex.StackTrace));
                            break; // Break the while loop so initialization doesn't lock up entirely
                        }

                        if (hasNext)
                        {
                            iterations++;
                            if (iterations % 500 == 0) // Alert if a single manager runs an extreme amount of iterations
                            {
                                Logger.Error(string.Format("[LoadingManager] WARNING: {0}.Load() is taking an abnormally long time... Loop count: {1}", managerName, iterations));
                            }
                            yield return current.Current;
                        }
                    }
                }

                Logger.Error(string.Format("[LoadingManager] STEP COMPLETE: Finished loading {0}", managerName));
            }
            else
            {
                Logger.Error(string.Format("[LoadingManager] [{0}/{1}] STEP WARNING: Element in LoadableList is NULL! Skipping.", (i + 1), LoadableList.Count));
            }
        }

        Logger.Error("[LoadingManager] LoadAll sequence successfully completed all steps.");

        if (callback != null)
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("[LoadingManager] EXCEPTION inside OnFinishedDelegate callback: {0}\n{1}", ex.Message, ex.StackTrace));
            }
        }
    }

    public void Clear()
    {
        foreach (ILoadable loadable in LoadableList)
        {
            if (loadable != null)
            {
                loadable.Destroy();
            }
        }
        instance = null;
    }
}