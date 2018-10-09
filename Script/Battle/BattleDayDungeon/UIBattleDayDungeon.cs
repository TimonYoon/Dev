using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIBattleDayDungeon : MonoBehaviour {

    [Header("영웅 리스트관련")]
    public GameObject pvpHeroSlotPrefab;
    public GridLayoutGroup scrollViewContent;
    RectTransform rectContent;
    List<PvPHeroSlot> heroSlotPool = new List<PvPHeroSlot>();

    [Header("전투 포기 관련")]
    public GameObject battleGiveupPanel;
    public Button buttonGiveup;
    [Header("결과 창 관련")]
    public GameObject resultPanel;

    public Animator winAnim;
    public Animator lossAnim;


    public GameObject iconRankUp;
    public GameObject iconRankDown;
    public Text textDiffRank;
    public Text textRank;

    public GameObject iconScoreUp;
    public GameObject iconScoreDown;
    public Text textDiffScore;
    public Text textScore;


    public Text textBattleTime;
    void Start()
    {
        buttonGiveup.interactable = true;
        battleGiveupPanel.SetActive(false);
        resultPanel.SetActive(false);
        BattleDayDoungen.Instance.onStartBattle += OnStartBattle;
        BattleDayDoungen.Instance.onEndBattle += OnEndBattle;
    }


    void OnStartBattle()
    {
        for (int i = 0; i < BattleDayDoungen.Instance.redTeamList.Count; i++)
        {
            BattleHero hero = BattleDayDoungen.Instance.redTeamList[i];
            PvPHeroSlot slot = CreateSlot();
            slot.InitSlot((i + 1), hero);

            hero.onChangedCumulativeDamage += OnChangedCumulativeDamage;
        }

        int count = BattleDayDoungen.Instance.redTeamList.Count;
        SizeControl(count);
        StartCoroutine(StartBattleCoroutine());

    }
    void OnChangedCumulativeDamage()
    {
        double topDamage = 0;
        if (heroSlotPool.Count > 0)
        {
            heroSlotPool.Sort(delegate (PvPHeroSlot a, PvPHeroSlot b)
            {
                return b.battleHero.cumulativeDamage.CompareTo(a.battleHero.cumulativeDamage);
            });


            topDamage = heroSlotPool[0].battleHero.cumulativeDamage;
        }

        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            heroSlotPool[i].transform.SetSiblingIndex(i);
            float value = (float)(heroSlotPool[i].battleHero.cumulativeDamage / topDamage);
            heroSlotPool[i].UpdateSlot((i + 1), value);
        }
    }

    PvPHeroSlot CreateSlot()
    {
        PvPHeroSlot slot = null;
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            if (heroSlotPool[i].gameObject.activeSelf == false)
            {
                slot = heroSlotPool[i];
                break;
            }
        }
        if (slot == null)
        {
            GameObject go = Instantiate(pvpHeroSlotPrefab, scrollViewContent.transform, false);
            slot = go.GetComponent<PvPHeroSlot>();
            heroSlotPool.Add(slot);
        }

        return slot;
    }

    void OnEndBattle()
    {
        for (int i = 0; i < BattleDayDoungen.Instance.redTeamList.Count; i++)
        {
            BattleHero hero = BattleDayDoungen.Instance.redTeamList[i];
            hero.onChangedCumulativeDamage -= OnChangedCumulativeDamage;
        }
        //for (int i = 0; i < heroSlotPool.Count; i++)
        //{
        //    heroSlotPool[i].gameObject.SetActive(false);
        //}

        winAnim.gameObject.SetActive(BattleDayDoungen.Instance.isWin);
        lossAnim.gameObject.SetActive(BattleDayDoungen.Instance.isWin == false);

        iconRankUp.SetActive(BattlePvPManager.userPvPRank > BattlePvPManager.userPvPResultRank);
        iconScoreUp.SetActive(BattlePvPManager.userPvPResultScore > BattlePvPManager.userPvPScore);

        iconRankDown.SetActive(BattlePvPManager.userPvPRank < BattlePvPManager.userPvPResultRank);
        iconScoreDown.SetActive(BattlePvPManager.userPvPResultScore < BattlePvPManager.userPvPScore);


        int diffRank = BattlePvPManager.userPvPRank - BattlePvPManager.userPvPResultRank;
        string rankTaxt = diffRank == 0 ? "순위변동 없음" : diffRank.ToString();

        textDiffRank.text = rankTaxt;
        textDiffScore.text = (BattlePvPManager.userPvPResultScore - BattlePvPManager.userPvPScore).ToString();
        textRank.text = BattlePvPManager.userPvPResultRank.ToString();
        textScore.text = BattlePvPManager.userPvPResultScore.ToString();


        resultPanel.SetActive(true);
    }

    IEnumerator StartBattleCoroutine()
    {
        while (true)
        {
            string time = BattleDayDoungen.Instance.pvpReminingTime <= 0 ? "" : BattleDayDoungen.Instance.pvpReminingTime.ToStringTime();
            textBattleTime.text = time;

            if (resultPanel.activeSelf)
            {
                if (winAnim.gameObject.activeSelf)
                {
                    if (winAnim.GetCurrentAnimatorStateInfo(0).IsName("LevelUpAnimation_End"))
                        break;
                }

                if (lossAnim.gameObject.activeSelf)
                {
                    if (lossAnim.GetCurrentAnimatorStateInfo(0).IsName("LevelUpAnimation_End"))
                        break;
                }
            }
            yield return null;
        }

        Close();
    }

    public void OnClickGiveUpButton()
    {
        battleGiveupPanel.SetActive(true);
    }
    public void OnClickGiveUpPanelYesButton()
    {
        buttonGiveup.interactable = false;
        BattleDayDoungen.Instance.isGiveUp = true;
        battleGiveupPanel.SetActive(false);
    }
    public void OnClickGiveUpPanelNoButton()
    {
        battleGiveupPanel.SetActive(false);
    }

    void Close()
    {
        if (coroutine != null)
            return;

        coroutine = StartCoroutine(BattleEnd());
        
       
        
    }
    Coroutine coroutine;
    IEnumerator BattleEnd()
    {
        LoadingManager.ShowFullSceneLoading();
        float startTime = Time.unscaledTime + 2f;
        while (true)
        {
            float t = startTime - Time.unscaledTime;
            if (t <= 0)
            {
                BattleDayDoungen.Instance.DespawnHero();
                resultPanel.SetActive(false);
                break;
            }
            yield return null;
        }
        coroutine = null;
        LoadingManager.Close();
        SceneManager.UnloadSceneAsync("BattleDayDungeon");
        
    }

    void SizeControl(float count)
    {
        if (rectContent == null)
            rectContent = scrollViewContent.GetComponent<RectTransform>();

        double quotient = System.Math.Ceiling((double)count);

        float sizeDeltaY = (scrollViewContent.cellSize.y + scrollViewContent.spacing.y) * ((int)quotient);

        rectContent.sizeDelta = new Vector2(rectContent.sizeDelta.x, sizeDeltaY);
    }

}
