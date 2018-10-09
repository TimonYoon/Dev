using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 전투중 레벨업 관련 UI 컨트롤러 => 가 아니라, 전투 화면에 보이는 영웅 리스트 관리 화면? </summary>
public class UIBattleLevelUp : MonoBehaviour {

    static UIBattleLevelUp Instance;

    static public SimpleDelegate onClickLevelUpEvenly;

    public RectTransform rectTransfromScrollViewContent;
    public GridLayoutGroup battleHeroScrollViewContent;
    public GameObject battleHeroSlotPrefab;
    public GameObject _lootObjectExpPrefab;
    static public GameObject lootObjectExpPrefab { get { return Instance._lootObjectExpPrefab; } }
    public Text expValueText;

    static public Transform pivotTotalExp
    {
        get
        {
            return Instance.expValueText.transform;
        }
    }
    
    static public List<BattleHeroSlot> battleHeroSlotList = new List<BattleHeroSlot>();

    /// <summary> 레벨업 컨트롤러는 각 배틀 그룹 마다 하나씩 지정되어 있음 현재 열려 있는 배틀 그룹의 레벨업 컨트롤러를 반환함 </summary>
    BattleLevelUpController battleLevelUpController
    {
        get
        {
            if (!Battle.Instance)
                return null;

            if (!Battle.currentBattleGroup)
                return null;

            if (!Battle.currentBattleGroup.battleLevelUpController)
                return null;
            
            return Battle.currentBattleGroup.battleLevelUpController;
        }
    }
    

    //########################################################################################################
    void Awake()
    {
        Instance = this;

        Battle.battleGroupList.onAdd += OnAddBattleGroup;
        Battle.battleGroupList.onRemove += OnRemoveBattleGroup;
    }

    void Start()
    {
        InitHeroSlots();

        //UIBattle.onShowBattle += OnShowBattle;
        Battle.onChangedBattleGroup += OnChangedBattleGroup;
    }

    void OnAddBattleGroup(BattleGroup battleGroup)
    {
        battleGroup.battleLevelUpController.onChangedTotalExp += OnChangedTotalExp;
    }

    void OnRemoveBattleGroup(BattleGroup battleGroup)
    {
        battleGroup.battleLevelUpController.onChangedTotalExp -= OnChangedTotalExp;
    }



    void OnChangedBattleGroup(BattleGroup battleGroup)
    {
        StartCoroutine(BattleHeroSlotSetting());
        
        //골고루 레벨업 버튼 활성/비활성
        buttonLevelUpEvenly.interactable = battleLevelUpController.expLevelUpEvenly <= battleLevelUpController.totalExp;
    }

    static public void ScaleText()
    {
        if(Instance.coroutineScaleText != null)
        {
            Instance.StopCoroutine(Instance.coroutineScaleText);
            Instance.coroutineScaleText = null;
        }

        Instance.coroutineScaleText = Instance.StartCoroutine(Instance.ScaleTextA());
    }

    Coroutine coroutineScaleText = null;
    IEnumerator ScaleTextA()
    {
        Text text = Instance.expValueText;
        text.transform.localScale = new Vector2(2f, 1.5f);
        float startTime = Time.time;
        while(Time.time - startTime < 1f)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, Vector3.one, 6f * Time.deltaTime);

            if (text.transform.localScale.x < 0.01f)
                break;

            yield return null;
        }

        text.transform.localScale = Vector3.one;

        coroutineScaleText = null;
    }


    //void OnShowBattle()
    //{

    //    StartCoroutine(BattleHeroSlotSetting());
    //}

    void InitHeroSlots()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject go = Instantiate(battleHeroSlotPrefab);
            go.transform.SetParent(battleHeroScrollViewContent.transform, false);
            BattleHeroSlot slot = go.GetComponent<BattleHeroSlot>();
            battleHeroSlotList.Add(slot);
            go.SetActive(false);
        }

        return;
        Vector2 v = battleHeroScrollViewContent.GetComponent<RectTransform>().rect.size;
        
        battleHeroScrollViewContent.cellSize = new Vector2(v.x - battleHeroScrollViewContent.padding.left - battleHeroScrollViewContent.padding.right,
                                                            (v.y - battleHeroScrollViewContent.spacing.y * 5) * 0.2f );
    }

    IEnumerator BattleHeroSlotSetting()
    {
        while (battleHeroSlotList.Count <= 0)
            yield return null;

        for (int i = 0; i < battleHeroSlotList.Count; i++)
        {
            battleHeroSlotList[i].gameObject.SetActive(false);
        }
        //LoadingManager.Show();
        while (battleLevelUpController == null)
        {
            yield return null;
        }
           

        while (!Battle.currentBattleGroup.isInitialized)
            yield return null;

        expValueText.text = battleLevelUpController.totalExp.ToStringABC();

        for (int i = 0; i < Battle.currentBattleGroup.originalMember.Count; i++)
        {
            battleHeroSlotList[i].gameObject.SetActive(true);
            battleHeroSlotList[i].SetBattleHero(Battle.currentBattleGroup.originalMember[i]);
            
        }

        float slotHeight = 0f;
        slotHeight = battleHeroSlotList[0].GetComponent<RectTransform>().sizeDelta.y;

        rectTransfromScrollViewContent.sizeDelta = new Vector2(rectTransfromScrollViewContent.sizeDelta.x, slotHeight * Battle.currentBattleGroup.originalMember.Count);

    }

    public Button buttonLevelUpEvenly;

    void OnChangedTotalExp(BattleGroup battleGroup)
    {
        if (Battle.currentBattleGroup != battleGroup)
            return;

        expValueText.text = battleLevelUpController.totalExp.ToStringABC();

        //골고루 레벨업 버튼 활성/비활성
        buttonLevelUpEvenly.interactable = battleLevelUpController.expLevelUpEvenly <= battleLevelUpController.totalExp;
    }


    public void OnPointerDown()
    {
        if (Battle.currentBattleGroup == null)
            return;

        if (coroutineLevelUp != null)
            StopCoroutine(coroutineLevelUp);

        coroutineLevelUp = StartCoroutine(LevelUpEvenly());
    }

    public void OnPointerUp()
    {
        if (coroutineLevelUp != null)
            StopCoroutine(coroutineLevelUp);
    }


    public AudioSource audioSourceLevelUp;

    Coroutine coroutineLevelUp = null;
    IEnumerator LevelUpEvenly()
    {
        float interval = 0.5f;
        while (buttonLevelUpEvenly.interactable)
        {
            if (onClickLevelUpEvenly != null)
                onClickLevelUpEvenly();

            PlaySoundLevelUp();

            yield return new WaitForSecondsRealtime(interval);

            if (interval > 0.01f)
                interval *= 0.8f;

            if (interval < 0.01f)
                interval = 0.01f;
        }

        yield break;
    }

    static public void PlaySoundLevelUp()
    {
        //소리 재생
        //audioSourceLevelUp.Stop();
        Instance.audioSourceLevelUp.Play();
    }

}
