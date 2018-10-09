using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
//using UnityEditor;
/// <summary> 배틀 씬에서 표현되는 UI를 조작하고 표현한다. </summary>
public class UIBattle : MonoBehaviour {
    public static UIBattle Instance { get; private set; }

    public delegate void UIBattleDelegate();

    static public global::SimpleDelegate onBattleMenuChanged;

    /// <summary> 전투 화면이 보여질 때 </summary>
    static public UIBattleDelegate onShowBattle;


    //public Transform battleList;

    /// <summary> 현재 스테이지에 대한 텍스트 </summary>
    [SerializeField]
    Text stageText;
        
    /// <summary> 전투에 출전 시킬 영웅 리스트 </summary>
    public static List<HeroData> selectedHeroDataList { get; private set; } // new List<HeroData>();

    /// <summary> 페이드 인/아웃용 </summary>
    [SerializeField]
    Image imageFadeInOut;

    //[SerializeField]
    //Text battleHeroCountText;

    [Header("버튼모음")]
    public Button battleReturnButton;

    [Header("전투 관련")]
    public Canvas battleCanvas;

    public Canvas canvasFrame;

    /// <summary> 영웅 토글 눌렀을 때 활성화 되는 녀석 </summary>
    public GameObject objHeroList;

    /// <summary> 퀘스트 토글 눌렀을 때 활성화 되는 녀석 </summary>
    public GameObject objQuest;

    /// <summary> 유물 토글 눌렀을 때 활성화 되는 녀석 </summary>
    public GameObject objArtifactList;

    /// <summary> 정보 토글 눌렀을 때 활성화 되는 녀석 </summary>
    public GameObject objInfo;

    public Text dungeonNameText;

    public List<UIToggleBattle> uiToggleBattleList = new List<UIToggleBattle>();


    /// <summary> 전투 화면이 보여지는 곳 </summary>
    public RectTransform battleScreen;

    [Header("회군 팝업 관련")]
    public GameObject objRestartConfirmPopup;


    public Canvas canvasReturn;
    public GameObject backGroundPanel;
    public GameObject battleReturnPopupPanel;
    public Text popupBattleTimeText;
    public Text popupItemCountText;
    public Text popupStageText;
    public GameObject resultPopup;
    public Text ItemNameText;
    public Text ItemCountText;
    
    
    public enum BattleMenuState
    {
        NotDefined,
        Hero,
        Quest,
        Artifact,
        Info
    }

    static BattleMenuState _battleMenuState = BattleMenuState.NotDefined;
    static public BattleMenuState battleMenuState
    {
        get
        {
            return _battleMenuState;
        }
        set
        {
            bool isValueChanged = _battleMenuState != value;
            
            _battleMenuState = value;

            if (isValueChanged && onBattleMenuChanged != null)
                onBattleMenuChanged();

            Instance.objHeroList.SetActive(value == BattleMenuState.Hero);
            Instance.objQuest.SetActive(value == BattleMenuState.Quest);
            Instance.objArtifactList.SetActive(value == BattleMenuState.Artifact);
            Instance.objInfo.SetActive(value == BattleMenuState.Info);
        }
    }

    //###############################################################################################
    void Awake()
    {
        if (!UserDataManager.Instance)
            gameObject.SetActive(false);

        Instance = this;

        //전투 관련 캔버스들 카메라 UICamera로 지정
        Scene scene = SceneManager.GetSceneByName("Battle");
        GameObject[] objs = scene.GetRootGameObjects();
        for(int i = 0; i < objs.Length; i++)
        {
            Canvas canvas = objs[i].GetComponent<Canvas>();
            if (!canvas)
                continue;

            canvas.worldCamera = Camera.main;
        }
    }

    void OnEnable()
    {
        Battle.onChangedBattleGroupList += OnChangedBattleGroupList;
        Battle.onChangedBattleGroup += OnChangedBattleGroup;
        PopupPanelAllClose();
    }
    
    void OnDisable()
    {
        Battle.onChangedBattleGroupList -= OnChangedBattleGroupList;
    }

    void OnChangedBattleGroup(BattleGroup _battleGroup)
    {
        OnChangedBattlePhase(Battle.currentBattleGroup);
    }

    IEnumerator Start()
    {
        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        battleMenuState = BattleMenuState.Quest;

        yield break;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Battle && state != LobbyState.BattlePreparation)
            Close();

        if (resultPopup.activeSelf)
            OnClickResultPopupOkButton();

        PopupPanelAllClose();
    }

    void Close()
    {
        Battle.ShowBattle(false);
        //Battle.Instance.battleCamera.enabled = true;

        battleCanvas.enabled = false;
        canvasFrame.enabled = false;
    }
            
    /// <summary> 무엇에 쓰이는 물건이지?? </summary>
    void OnChangedBattleGroupList()
    {
        for(int i = 0; i < Battle.battleGroupList.Count; i++)
        {
            Battle.battleGroupList[i].onChangedBattlePhase += OnChangedBattlePhase;
            Battle.battleGroupList[i].onChangedStage += OnChangedStage;
        }
    }

    void OnChangedBattlePhase(BattleGroup battleGroup)
    {
        if (!battleGroup ||!battleGroup.isRenderingActive)
            return;

        if (coroutineFadeInOut != null)
            StopCoroutine(coroutineFadeInOut);

        coroutineFadeInOut = StartCoroutine(FadeInOut(battleGroup));

        //StopCoroutine("FadeInOut");
        //StartCoroutine("FadeInOut", battleGroup);
    }

    Coroutine coroutineFadeInOut = null;
    IEnumerator FadeInOut(BattleGroup battleGroup)
    {
        //BattleGroup 클래스의 처리와 멀티쓰레딩으로 진행이라, 한 프레임 늦게 해줘야 플리킹 현상 없음
        yield return null;

        imageFadeInOut.enabled = true;

        if (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
        {
            while(battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
            {
                imageFadeInOut.color = new Color(0f, 0f, 0f, 1f - battleGroup.fadeInProcess);
                yield return null;
            }
        }

        if (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut)
            while (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut)
            {
                imageFadeInOut.color = new Color(0f, 0f, 0f, battleGroup.fadeOutProcess);
                yield return null;
            }

        imageFadeInOut.enabled = false;

        coroutineFadeInOut = null;
    }

    void OnChangedStage(BattleGroup battleGroup)
    {
        if (Battle.currentBattleGroup != battleGroup)
            return;

        UpdateDungeonName();
    }

    void OnChangedWave(BattleGroup battleGroup)
    {
        if (Battle.currentBattleGroup != battleGroup)
            return;

        UpdateDungeonName();
    }

    void UpdateDungeonName()
    {
        BattleGroup battleGroup = Battle.currentBattleGroup;

        int stage = battleGroup.stage;

        stageText.text = GameDataManager.dungeonBaseDataDic[Battle.currentBattleGroup.dungeonID].dungeonName + " " + stage;
    }

    /// <summary> 파티편집 버튼 눌렀을 때 </summary>
    public void OnClickEditParty()
    {
        Battle.ShowBattlePreparation(Battle.currentBattleGroup.battleType, Battle.currentBattleGroup.originalMember);
    }

    /// <summary> 전투 화면 닫기 버튼 </summary>
    public void OnClickCloseBattle()
    {        
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        Close();
    }

   
    /// <summary> 복귀(회군?) 버튼 </summary>
    public void OnClickReturn()
    {
        //Todo: 복귀 확인 창 떠야함. 이 화면에 획득한 전리품, 최고 층수, 플레이 타임 등이 표시
        //복귀 확인 누르면 복귀 프로세스 진행
        StartCoroutine(ShowReturnPopupPanel());        
    }



    //회군 팝업
    //##########################################################################
    /// <summary> 회군 팝업 띄움 </summary>
    static public void ShowRestartConfirmPopup()
    {
        Instance.objRestartConfirmPopup.SetActive(true);
    }

    /// <summary> 회군 팝업에서 No 버튼 누름 </summary>
    public void OnClickRestartConfirmNo()
    {
        //Debug.Log("회군 취소");
        PopupPanelAllClose();
    }

    /// <summary> 회군 팝업에서 네 버튼 누름 </summary>
    public void OnClickRestartConfirmYes()
    {
        if (!Battle.currentBattleGroup)
            return;

        StartCoroutine(Battle.currentBattleGroup.Restart(BattleGroup.RestartType.Normal));
        StartCoroutine(HeroManager.UpdateHeroProficiency());

        StartReturnCinematic();

        PopupPanelAllClose();
    }

    /// <summary> 회군 팝업에서 강화석 두 배 버튼 누름 </summary>
    public void OnClickRestartConfirmYesDouble()
    {
        if (!Battle.currentBattleGroup)
            return;

        //비용 체크
        if(MoneyManager.GetMoney(MoneyType.ruby).value < 300)
        {
            UIPopupManager.ShowOKPopup("", "루비가 부족합니다.", null);
            return;
        }
     

        StartCoroutine(Battle.currentBattleGroup.Restart(BattleGroup.RestartType.Double));
        StartCoroutine(HeroManager.UpdateHeroProficiency());
        StartReturnCinematic();

        PopupPanelAllClose();
    }

    public Animator animatorReturn;


    static public void StartReturnCinematic()
    {
        Instance.animatorReturn.SetTrigger("startReturn");
    }

    //회군 결과
    //##########################################################################
    /// <summary> 결과 팝업에서 ok 버튼 누름 </summary>
    public void OnClickResultPopupOkButton()
    {
        PopupPanelAllClose();

        //첫번째 배틀 그룹은 그냥 창 닫기
        if (Battle.currentBattleGroup.battleType == "Battle_1")
            return;

        Battle.RemoveBattle();        
    }

    
    /// <summary> 팝업 결과창에서 다시시작 버튼 눌렀을 때 </summary>
    public SimpleDelegate onClickReStartButton;

    /// <summary> 결과 팝업에서 다시하기 버튼 누름 </summary>
    public void OnClickResultPopupReStartButton()
    {
        PopupPanelAllClose();

        if (onClickReStartButton != null)
            onClickReStartButton();

        //첫번째 배틀 그룹은 그냥 창 닫기
        if (Battle.currentBattleGroup.battleType == "Battle_1")
            return;

        Battle.currentBattleGroup.ReStartBattle();        
    }

    /// <summary> 팝업 관련 패널 전부 끄기 </summary>
    void PopupPanelAllClose()
    {
        objRestartConfirmPopup.SetActive(false);

        battleReturnPopupPanel.SetActive(false);
        resultPopup.SetActive(false);
        backGroundPanel.SetActive(false);
        canvasReturn.gameObject.SetActive(false);

        //if (UIFadeController.isFinishFadeOut)
        UIFadeController.FadeIn();
    }

    /// <summary> 회군 팝업 띄우기 </summary>
    IEnumerator ShowReturnPopupPanel()
    {
        canvasReturn.gameObject.SetActive(true);
        backGroundPanel.SetActive(true);
        battleReturnPopupPanel.SetActive(true);
        battleReturnPopupPanel.GetComponent<Animation>().Play();
        string itemName = GameDataManager.moneyBaseDataDic[GameDataManager.dungeonBaseDataDic[Battle.currentBattleGroup.dungeonID].dropItemID].name;
        while (battleReturnPopupPanel.activeSelf)
        {
            popupBattleTimeText.text = "진행 시간 [ " + SecChangeToDateTime(Battle.currentBattleGroup.battleTime) + " ]";
            popupItemCountText.text = "현재까지 모은 ( " + itemName + " )갯수 : [ " + Battle.currentBattleGroup.totalEnhanceStoneCount + " ]";
            popupStageText.text = "현재까지 도달한 던전 층수 : [ " + Battle.currentBattleGroup.stage + " ]";
            yield return null;
        }
        yield break;
        
    }

    /// <summary> 보상 팝업 띄우기 </summary>
    void ShowResultPopupPanel()
    {
        canvasReturn.gameObject.SetActive(true);
        backGroundPanel.SetActive(true);
        resultPopup.SetActive(true);
        ItemNameText.text = GameDataManager.moneyBaseDataDic[GameDataManager.dungeonBaseDataDic[Battle.currentBattleGroup.dungeonID].dropItemID].name;
        
        ItemCountText.text = Battle.currentBattleGroup.totalEnhanceStoneCount.ToString();
        
    }

    /// <summary> 초를 날/시/분/초 로 바꿈 </summary>
    string SecChangeToDateTime(float time)
    {
        int second = (int)time;
        int hour = 0;
        int minute = 0;

        bool isChack = false;
        while (true)
        {
            if (second > 59)
            {
                minute++;
                second -= 60;
                if (minute > 59)
                {
                    isChack = true;
                    hour++;
                    minute = 0;
                }
            }
            else
            {
                if(!isChack)
                {
                    string result = string.Format("{0:00} : {1:00}", minute, second);
                    return result;
                }
                else
                {
                    string result = string.Format("{0:00} : {1:00} : {2:00}", hour, minute, second);
                    return result;
                }
               
            }
        }
    }
    
    /// <summary> 전투 화면 보여줌 </summary>
    static public void ShowBattlePanel(string dungeonID)
    {
        Instance.battleCanvas.enabled = true;
        Instance.canvasFrame.enabled = true;
        //Instance.battleReturnButton.gameObject.SetActive(true);
        Instance.dungeonNameText.gameObject.SetActive(true);
        Instance.dungeonNameText.text = GameDataManager.dungeonBaseDataDic[Battle.currentBattleGroup.dungeonID].dungeonName;

        Instance.UpdateDungeonName();
        //Instance.stageText.text = GameDataManager.dungeonBaseDataDic[dungeonID].dungeonName + " " + Battle.battleGroupList.Find(x => x.id == dungeonID).stage.ToString() + "층";

        if (onShowBattle != null)
            onShowBattle();
    }
        
    /// <summary> 미사용... </summary>
    void OnChangedFadeInOutValue()
    {
        if (!Battle.currentBattleGroup)
            return;

        if(Battle.currentBattleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
        {
            imageFadeInOut.color = new Color(0f, 0f, 0f, 1f - Battle.currentBattleGroup.fadeInProcess);
        }
        else if (Battle.currentBattleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut)
        {
            imageFadeInOut.color = new Color(0f, 0f, 0f, Battle.currentBattleGroup.fadeInProcess);
        }
    }

    /// <summary> 영웅, 유물, 정보 등 토글버튼에 의해 전투 화면 메뉴가 변경될 때 </summary>
    /// <param name="value">토글버튼에 설정되어 있음</param>
    public void OnValueChangedMenu(string value)
    {
        if (value == "Hero")
        {
            battleMenuState = BattleMenuState.Hero;
        }
        else if (value == "Quest")
        {
            battleMenuState = BattleMenuState.Quest;
        }
        else if (value == "Artifact")
        {
            battleMenuState = BattleMenuState.Artifact;
        }
        else if (value == "Info")
        {
            battleMenuState = BattleMenuState.Info;
        }
    }
    
}
