using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using LitJson;

public class UIHeroTraining : MonoBehaviour {

    public static UIHeroTraining Instance;

    public RectTransform rectTransformScreen;
    

    //영웅 선택창 표현부 레퍼런스
    //#################################################################################
    [Header("영웅 선택창 표현부 레퍼런스")]
    [SerializeField]
    GameObject noTrainingHeroPanel;

    [SerializeField]
    Image heroBG;

    [SerializeField]
    List<Color> colorList;

    [SerializeField]
    Image heroImage;

    [SerializeField]
    Text heroName;

    [SerializeField]
    List<Text> textParamNameList;

    [SerializeField]
    List<Text> textCurrentTrainingValueList;

    [SerializeField]
    List<Text> textMaxTrainingValueList;


    [SerializeField]
    List<GameObject> gradeList;

    [SerializeField]
    Text coinAmountText;

    //###############################################################################################
    //영웅선택창 영웅슬롯 표현부 레퍼런스
    [Header("영웅선택창 영웅슬롯 표현부 레퍼런스")]
    [SerializeField]
    public List<UIHeroTrainingSlot> heroTrainingSlotList = new List<UIHeroTrainingSlot>();
    

    //스탯 리스트
    public List<UITrainingStatSlot> statSlotList = new List<UITrainingStatSlot>();
    
  
    [SerializeField]
    Text noHeroTrainingText;

    [SerializeField]
    Text textTrainingPoint;

    //#####################################################################################
    //훈련소 영웅 선택화면 레퍼런스
    [Header("훈련소 레퍼런스")]
    [SerializeField]
    GameObject selectHeroPanel;

    [SerializeField]
    Text trainingTimeText;

    public int slotNumber;

    public int trainingStat = 100;

    float trainingTime = 0f;

    double trainingNeedCoin = 0;

    [SerializeField]
    GameObject heroSelectButton;

    [SerializeField]
    ScrollRect heroTrainingSlotScrollRect;

    [SerializeField]
    GridLayoutGroup heroTrainingScrollViewContent;
    

    public string _selectedTrainingHeroID = string.Empty;
    public string selectedTrainingHeroID
    {
        get { return _selectedTrainingHeroID; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _selectedTrainingHeroID = value;

                CheckNeedMoneyAndTime(_selectedTrainingHeroID);
                return;
            }


            if (_selectedTrainingHeroID == value)
                return;

            _selectedTrainingHeroID = value;

            CheckNeedMoneyAndTime(_selectedTrainingHeroID);

        }
    }

    private void Awake()
    {
        Instance = this;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.HeroTraining)
            Close();
    }

    

    void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        UITrainingStatSlot.onClickStatSelectButton += ChangeTrainingStat;
        

        for (int i = 0; i < HeroTrainingManager.Instance.heroTrainingDataList.Count; i++)
        {
            int num = HeroTrainingManager.Instance.heroTrainingDataList[i].slotNumber;

            heroTrainingSlotList[num].heroTrainingData = HeroTrainingManager.Instance.heroTrainingDataList[i];
            heroTrainingSlotList[num].InitTrainingSlot();
        }

    }


    public void ShowSelectPanel()
    {
        if (!string.IsNullOrEmpty(selectedTrainingHeroID))
        {
            UIHeroSlotContainer s = UIHeroInventory.heroSlotContainerList.Find(x => x.heroInvenID == selectedTrainingHeroID);
            if (s)
            {
                s.isSelectedToHero = false;
            }
        }

        if (heroTrainingSlotList[slotNumber].heroTrainingData == null)
            selectedTrainingHeroID = string.Empty;

        //MAX 수련인 애들은 안보이게 할까?
        for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
        {
            string id = UIHeroInventory.heroSlotContainerList[i].heroInvenID;
            int num = 0;

            for (int j = 0; j < HeroManager.heroDataDic[id].trainingDataList.Count; j++)
            {
                num += HeroManager.heroDataDic[id].trainingDataList[j].training;
            }

            if (num >= HeroManager.heroDataDic[id].trainingMax)
            {
                UIHeroInventory.heroSlotContainerList[i].gameObject.SetActive(false);
            }

        }

        //정리되면 켜주고 
        UIHeroInventory.Instance.ShowHeroInvenForTraining();

        //인벤토리 사이즈 조절
        RectTransform t = UIHeroInventory.Instance.objListRoot.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(t.sizeDelta.x, -Instance.rectTransformScreen.sizeDelta.y - t.anchoredPosition.y);


        //선택 영웅에 대한 표현을 하고
        Show();

        //패널을 켜주자
        selectHeroPanel.SetActive(true);
    }


    /// <summary> 최대치 여부 확인 </summary>
    public bool isTrainingMax { get; private set; }

    public void Show()
    {
        //test.gameObject.SetActive(false);
        if (HeroTrainingManager.Instance == null)
            return;

        if (string.IsNullOrEmpty(selectedTrainingHeroID))
        {
            noTrainingHeroPanel.SetActive(true);
            heroSelectButton.SetActive(false);
        }   
        else
        {
            for (int i = 0; i < statSlotList.Count; i++)
            {
                statSlotList[i].InitSlot();
            }

            noTrainingHeroPanel.SetActive(false);
            heroSelectButton.SetActive(true);

            HeroData heroData = HeroManager.heroDataDic[selectedTrainingHeroID];

            for (int i = 0; i < gradeList.Count; i++)
            {
                if (heroData.heroGrade == (i + 1))
                {
                    gradeList[i].SetActive(true);
                    heroBG.color = colorList[i];
                }
                else
                {
                    gradeList[i].SetActive(false);
                }

            }

            heroName.text = heroData.heroName;

            AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);

            for (int i = 0; i < textParamNameList.Count; i++)
            {
                textParamNameList[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < heroData.trainingDataList.Count; i++)
            {
                textParamNameList[i].gameObject.SetActive(true);
                textParamNameList[i].text = heroData.trainingDataList[i].paramName;
                textCurrentTrainingValueList[i].text = heroData.trainingDataList[i].training + "단계";
                //textMaxTrainingValueList[i].text = heroData.trainingMax.ToString();
            }

            int count = 0;
            for (int i = 0; i < heroData.trainingDataList.Count; i++)
            {
                count += heroData.trainingDataList[i].training;
            }

            textTrainingPoint.text = (heroData.trainingMax - count).ToString();


           

            CheckNeedMoneyAndTime(selectedTrainingHeroID);
            ShowMaxPanel();
        }



        SizeControl();

    }
    

    public GameObject trainingMaxPanel;

    /// <summary> 수련치 최대 </summary>
    public void ShowMaxPanel()
    {
        int count = 0;
        for (int i = 0; i < HeroManager.heroDataDic[selectedTrainingHeroID].trainingDataList.Count; i++)
        {
            count += HeroManager.heroDataDic[selectedTrainingHeroID].trainingDataList[i].training;
        }

        if (count < HeroManager.heroDataDic[selectedTrainingHeroID].trainingMax)
            trainingMaxPanel.SetActive(false);
        else
            trainingMaxPanel.SetActive(true);
    }
    

    public void FinishTraining(int slotNum)
    {
        heroTrainingSlotList[slotNum].InitTrainingSlot();

        HeroTrainingData data = new HeroTrainingData();
        data.heroID = string.Empty;
        data.slotNumber = slotNum;
        double gold = 0f;

        if (DailyMissionManager.Instance && DailyMissionManager.Instance.heroTrainingCount < 1)
        {
            DailyMissionManager.Instance.heroTrainingCount += 1;
            StartCoroutine(DailyMissionManager.Instance.SetDailyMission(DailyMissionType.HeroTraining));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.colaTrainingCount < 1 && HeroManager.heroDataDic[heroTrainingSlotList[slotNum].heroTrainingData.heroID].heroID == "Knight_02_Hero")
        {
            UserQuestManager.Instance.colaTrainingCount += 1;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.ColaTraining));
        }

        StartCoroutine(SetHeroTrainingData(data, gold));
    }

    public void ChangeTrainingStat(int statNum)
    {
        trainingStat = statNum;
    }
    
    /// <summary> 수련 시작 </summary>
    public void OnClickStartTraining()
    {

        if (string.IsNullOrEmpty(selectedTrainingHeroID))
            return;

        for (int i = 0; i < HeroTrainingManager.Instance.heroTrainingDataList.Count; i++)
        {
            if(HeroTrainingManager.Instance.heroTrainingDataList[i].slotNumber != slotNumber && HeroTrainingManager.Instance.heroTrainingDataList[i].heroID == selectedTrainingHeroID)
            {
                UIPopupManager.ShowOKPopup("중복된 영웅 선택", "이미 훈련중인 영웅을 선택하셨습니다", null);
                return;
            }
        }

        if (trainingStat >= HeroManager.heroDataDic[selectedTrainingHeroID].trainingDataList.Count)
        {
            UIPopupManager.ShowOKPopup("훈련 시작 실패", "훈련할 능력치를 선택 후 다시 시도해주세요", null);
            return;
        }

        if(MoneyManager.GetMoney(MoneyType.gold).value < trainingNeedCoin)
        {
            UIPopupManager.ShowOKPopup("골드 부족", "골드가 부족합니다", null);
            return;
        }

        if (HeroManager.heroDataDic[selectedTrainingHeroID].trainingDataList[trainingStat].training >= HeroManager.heroDataDic[selectedTrainingHeroID].trainingMax)
        {
            UIPopupManager.ShowOKPopup("훈련 시작 실패", "이미 최대로 훈련된 능력치입니다\n한계돌파 후 다시 시도해주세요", null);
            return;
        }

        if (heroTrainingSlotList[slotNumber].heroTrainingData != null && selectedTrainingHeroID == heroTrainingSlotList[slotNumber].heroTrainingData.heroID && heroTrainingSlotList[slotNumber].heroTrainingData.isTrainingStart == true)
        {
            selectHeroPanel.SetActive(false);
            return;
        }

        UIPopupManager.ShowYesNoPopup("훈련 시작", "훈련이 시작되면 영웅과 훈련할 항목을 바꿀 수 없습니다\n계속하시겠습니까", TrainingStartResult);
        
        
    }

    void TrainingStartResult(string result)
    {
        if(result == "yes")
        {
            UIHeroInventory.Instance.OnClickCloseButton();

            HeroTrainingData data = new HeroTrainingData();
            data.heroID = selectedTrainingHeroID;
            data.trainingStat = trainingStat;
            data.slotNumber = slotNumber;
            data.trainingTime = trainingTime;

            StartCoroutine(SetHeroTrainingData(data, trainingNeedCoin));
        }
    }

    IEnumerator SetHeroTrainingData(HeroTrainingData data, double gold)
    {
        WWWForm form = new WWWForm();
        string php = "HeroTraining.php";
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", data.heroID);
        form.AddField("trainingStat", data.trainingStat);
        form.AddField("slotNumber", data.slotNumber);
        form.AddField("goldAmount", gold.ToString());
        form.AddField("type", 2);
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if(!string.IsNullOrEmpty(result))
        {
            if (result == "99")
            {
                UIPopupManager.ShowOKPopup("골드 부족", "골드가 부족합니다", null);
                yield break;
            }
            else if (result == "100")
            {
                UIPopupManager.ShowOKPopup("중복된 영웅 선택", "이미 훈련중인 영웅을 선택하셨습니다", null);
                yield break;
            }
            else
            {
                data.diffTime = float.Parse(result);


                heroTrainingSlotList[slotNumber].heroTrainingData = data;
                heroTrainingSlotList[slotNumber].InitTrainingSlot();
                data.isTrainingStart = true;
                HeroTrainingManager.Instance.heroTrainingDataList.Add(data);

                trainingStat = 100;
                selectHeroPanel.SetActive(false);
            }
        }
        
    }



   
    /// <summary> 수련할 영웅 선택 </summary>
    public void SelectTrainingHero(string heroID)
    {
        selectedTrainingHeroID = heroID;
        
        Show();
    }
    


    /// <summary> 수련영웅이 바뀌면 적용될 콜백 </summary>
    void CheckNeedMoneyAndTime(string heroID)
    {
        trainingNeedCoin = 0;
        if(string.IsNullOrEmpty(heroID) == false)
        {
            int trainingCount = 0;
            HeroData heroData = HeroManager.heroDataDic[heroID];
            for (int i = 0; i < heroData.trainingDataList.Count; i++)
            {
                trainingCount += heroData.trainingDataList[i].training;
            }
            trainingCount += 1;
            trainingNeedCoin = 10000 * Mathf.Pow(1.5f, trainingCount);

            trainingTime = CheckTrainingTime();
            trainingTimeText.text = trainingTime.ToStringTimeHMS();
        }

        
        coinAmountText.text = trainingNeedCoin.ToStringABC();
    }

    /// <summary>수련시간 반환메소드 </summary>
    /// <returns>총 수련시간</returns>
    float CheckTrainingTime()
    {
        HeroData heroData = HeroManager.heroDataDic[selectedTrainingHeroID];
        int trainingCount = 0;
        for (int i = 0; i < heroData.trainingDataList.Count; i++)
        {
            trainingCount += heroData.trainingDataList[i].training;
        }
        
        trainingCount += 1;
        //int heroGrade = heroData.heroGrade;
        float trainingTime = trainingCount * 3600f;

        return trainingTime; 
    }

  

    public void OnClickCloseButton()
    {
        
        UIHeroInventory.Instance.OnClickCloseButton();
        selectHeroPanel.SetActive(false);
    }

    public void Close()
    {
        for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
        {
            if (UIHeroInventory.heroSlotContainerList[i].gameObject.activeSelf == false)
            {
                UIHeroInventory.heroSlotContainerList[i].gameObject.SetActive(true);
            }
        }


        if (!string.IsNullOrEmpty(selectedTrainingHeroID))
        {
            UIHeroSlotContainer s = UIHeroInventory.heroSlotContainerList.Find(x => x.heroInvenID == selectedTrainingHeroID);
            if (s)
            {
                s.isSelectedToHero = false;
            }
        }

        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        UITrainingStatSlot.onClickStatSelectButton -= ChangeTrainingStat;

        SceneManager.UnloadSceneAsync("HeroTraining");
    }

   
    RectTransform battleHeroContentRect;
    RectTransform territoryHeroContentRect;
    RectTransform trainingSlotContentRect;

    ///// <summary> Scroll content size conrtrol </summary>
    void SizeControl()
    {
        if (trainingSlotContentRect == null)
            trainingSlotContentRect = heroTrainingScrollViewContent.GetComponent<RectTransform>();


        float count = heroTrainingScrollViewContent.transform.childCount;
        double quotient = Math.Ceiling((double)count);
        float sizeDeltaY = (heroTrainingScrollViewContent.cellSize.y + heroTrainingScrollViewContent.spacing.y) * ((int)quotient);

        trainingSlotContentRect.sizeDelta = new Vector2(trainingSlotContentRect.sizeDelta.x, sizeDeltaY);
    }

}
