using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class BattleQuest
{
    public BattleQuest(BattleQuestController battleQuestController = null)
    {
        this.battleQuestController = battleQuestController;
    }
    
    /// <summary> 레벨이 증가할 때</summary>
    public SimpleDelegate onChangedLevel;
    
    /// <summary> 레벨이 0에서 1이상이 될 때 발생 </summary>
    public SimpleDelegate onUnlocked;

    /// <summary> 자동 실행 개방될 때 </summary>
    public SimpleDelegate onUnlockedAuto;

    /// <summary> 퀘스트 완료 후 리셋 될 때 실행 (progress 0 될 때) </summary>
    public SimpleDelegate onFinished;

    BattleQuestController battleQuestController = null;

    BattleQuestBaseData _baseData;
    public BattleQuestBaseData baseData
    {
        get { return _baseData; }
        set
        {
            _baseData = value;

            if (value.tier == 1 && level < 1)
                level = 1;

            TerritoryManager.onAddPlace += UpdatePlaceModify;
            TerritoryManager.onChangedPlaceData += UpdatePlaceModify;

            UpdatePlaceModify();
            UpdateBaseIncome();
        }
    }

    /// <summary> 수익 증가, 업그레이드 비용 감소 등 가격에 보정되는 값이 변경된 경우 </summary>
    public SimpleDelegate onChangedModifyValue;

    ObscuredDouble _placeModifyIncreaseIncome = 0d;
    ObscuredDouble placeModifyIncreaseIncome
    {
        get { return _placeModifyIncreaseIncome; }
        set
        {
            bool isChanged = _placeModifyIncreaseIncome != value;

            _placeModifyIncreaseIncome = value;

            if (isChanged && onChangedModifyValue != null)
                onChangedModifyValue();
        }
    }

    ObscuredDouble _placeModifyReduceUpgradeCost = 0f;
    ObscuredDouble placeModifyReduceUpgradeCost
    {
        get { return _placeModifyReduceUpgradeCost; }
        set
        {
            bool isChanged = _placeModifyReduceUpgradeCost != value;

            _placeModifyReduceUpgradeCost = value;

            if (isChanged && onChangedModifyValue != null)
                onChangedModifyValue();
        }
    }

    void UpdatePlaceModify()
    {
        //영지 효과 퀘스트 보상 증가
        placeModifyIncreaseIncome = 0f;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
            if (placeData.placeBaseData.type == "Battle_IncreaseQuestExpRatio")
            {
                float a = 0f;
                float.TryParse(placeData.placeBaseData.formula, out a);
                placeModifyIncreaseIncome += a * 0.01f * placeData.placeLevel;
            }
        }

        //영지 효과 - 퀘스트 레벨업 비용 감소
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
            if (placeData.placeBaseData.type == "Battle_IncreaseNeedQuestExpRatio")
            {
                float a = 0f;
                float.TryParse(placeData.placeBaseData.formula, out a);
                placeModifyReduceUpgradeCost = a * 0.01f * placeData.placeLevel;
            }
        }


        //Debug.Log(placeModifyReduceUpgradeCost + ", " + income);
    }

    public BattleQuestSaveDataQuest saveData = null;

    /// <summary> 현재 퀘스트 진행 시간 </summary>
    public ObscuredFloat startTime = 0f;

    ObscuredInt _level = 0;
    /// <summary> 현재 레벨. 0은 아직 습득하지 않은 것을 의미 </summary>
    public ObscuredInt level
    {
        get { return _level; }
        set
        {
            bool isChanged = _level != value;
            bool isUnlock = _level == 0 && value > 0;
                        
            _level = value;

            //해금 여부에 따라 콜백
            if (isUnlock && onUnlocked != null)
                onUnlocked();

            //레벨 변동 콜백
            if (isChanged && onChangedLevel != null)
                onChangedLevel();
        }
    }

    ObscuredBool _isAutoRepeat = false;
    /// <summary> 자동 실행 여부 </summary>
    public ObscuredBool isAutoRepeat
    {
        get { return _isAutoRepeat; }
        set
        {
            bool isChanged = _isAutoRepeat != value;

            _isAutoRepeat = value;

            if (isChanged && value && onUnlockedAuto != null)
            {
                onUnlockedAuto();

                battleQuestController.StartQuest(this);
            }
        }
    }

    public ObscuredDouble baseIncomePrevTier = 0d;

    ObscuredDouble _baseIncome = 5d;
    public ObscuredDouble baseIncome
    {
        get { return _baseIncome; }
        set { _baseIncome = value; }
    }

    /// <summary> 현재 수익 (실제 보상증가량 * 레벨) </summary>
    public ObscuredDouble income
    {
        get
        {
            if (baseData == null)
                return 0d;

            //해금하기 전이면 1레벨 기준으로 계산해줌
            if (level == 0)
                return incomeGrouth;

            return incomeGrouth * level;
        }
    }

    /// <summary> 수익 증가량 (기준 수익 * 수행 시간) </summary>
    public ObscuredDouble incomeGrouth
    {
        get
        {
            if (baseData == null)
                return 0d;

            return baseIncome * (1 + placeModifyIncreaseIncome) * baseData.time;
        }
    }

    /// <summary> 해금 비용 </summary>
    public ObscuredDouble unlockCost
    {
        get
        {
            if (baseData == null)
                return 0d;

            //해금 비용 = 이전 티어의 기준 보상 * 기준 시간 (기준 시간은 1티어 기준 5분, 이 시간은 티어마다 30%씩 증가)
            return 300 * Math.Pow(1.5d, baseData.tier - 1) * baseIncomePrevTier;

            //return baseIncome * baseData.time * 50;
        }
    }

    ObscuredDouble upgradeCostModify = 1d;

    /// <summary> 해금 비용 </summary>
    public ObscuredDouble autoQuestCost
    {
        get
        {
            if (baseData == null)
                return 0d;

            return baseIncome * baseData.time * 300;
        }
    }

    /// <summary> 해금 비용 루비 </summary>
    public ObscuredInt autoQuestCostRuby
    {
        get
        {
            if (baseData == null)
                return 1000;
                        
            return 1 + (baseData.tier - 1) * 2;
        }
    }

    public ObscuredDouble GetUpgradeCost(int upgradeAmount = 1)
    {
        if (baseData == null)
            return 0d;


        //Debug.Log(placeModify);

        //강화비용은 퀘스트 레벨마다 기본가 대비 복리로 20%씩 증가. 기본가는 티어마다 일정 비율(40%씩?) 증가

        double curLevelCost = baseData.time * baseIncome / (1 + placeModifyReduceUpgradeCost) * upgradeCostModify * Math.Pow(1.3, level - 1);        
        double destLevelCost = baseData.time * baseIncome / (1 + placeModifyReduceUpgradeCost) * upgradeCostModify * Math.Pow(1.3, level + upgradeAmount - 1);

        return destLevelCost - curLevelCost;
    }

    public ObscuredDouble upgradeCost
    {
        get
        {
            return GetUpgradeCost();
        }
    }

    void UpdateBaseIncome()
    {
        double tierModify = Math.Pow(baseData.tier, 1.3d);
        double tierModifyPrev = Math.Pow(baseData.tier - 1, 1.3d);

        baseIncome = 0.2 * Math.Pow(7, tierModify/* - 1*/);

        baseIncomePrevTier = 0.2 * Math.Pow(7, tierModifyPrev);

        upgradeCostModify = Math.Pow(baseData.tier, 1.4);

        //Todo: 특성 등에 의한 수익량 증가 적용
    }

    ObscuredFloat _progress = 0f;
    /// <summary> 진척도 </summary>
    public ObscuredFloat progress
    {
        get { return _progress; }
        set
        {
            bool isChanged = _progress != value;

            _progress = value;

            //완료 여부 콜백
            if (isChanged && value == 0f && onFinished != null)
                onFinished();
        }
    }

    public float lastSaveTime { get; set; }

    /// <summary> 퀘스트 진행 중인지 여부 </summary>
    public ObscuredBool isDoingQuest
    {
        get { return coroutineDoQuest != null; }
    }

    //퀘스트 진행 중 자동 세이브 주기
    float autoSaveInterval = 10f;

    public Coroutine coroutineDoQuest = null;
    /// <summary> 퀘스트 진행 </summary>
    public IEnumerator DoQuest(float startTime)
    {
        this.startTime = startTime;
        //startTime = Time.time;

        //progress = 0.01f;

        lastSaveTime = Time.time;

        while (true)
        {
            //진행상황 갱신
            progress = (Time.time - startTime) / baseData.time;

            //시간 꽉 찰 때 처리
            if (progress >= 1f)
            {
                //수익 증가
                battleQuestController.AddIncome(income);

                //진행상황 0으로 초기화
                progress = 0f;

                //수익이 난 경우 무조건 자동 세이브
                if (baseData.time > autoSaveInterval)
                    battleQuestController.SaveQuestData(this);

                //자동 반복 중이면
                if (isAutoRepeat)
                {
                    this.startTime = Time.time;
                    startTime = Time.time;
                }
                //아니면 중단
                else
                {
                    
                    break;
                }   
            }
            else
            {
                //퀘스트 진행 중에는 가장 마지막으로 세이브한 시점으로 부터 n초 이상 경과했으면 자동 세이브
                if (baseData.time > autoSaveInterval && Time.time - lastSaveTime > autoSaveInterval)
                    battleQuestController.SaveQuestData(this);
            }
            

            yield return null;
        }

        progress = 0f;

        coroutineDoQuest = null;
    }
    
}