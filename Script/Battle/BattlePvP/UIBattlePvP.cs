using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIBattlePvP : MonoBehaviour
{
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
        BattlePvP.Instance.onStartBattle += OnStartBattle;
        BattlePvP.Instance.onEndBattle += OnEndBattle;
    }
    

    void OnStartBattle()
    {
        for (int i = 0; i < BattlePvP.Instance.redTeamList.Count; i++)
        {
            BattleHero hero = BattlePvP.Instance.redTeamList[i];           
            PvPHeroSlot slot = CreateSlot();
            slot.InitSlot((i + 1), hero);

            hero.onChangedCumulativeDamage += OnChangedCumulativeDamage;
        }

        int count = BattlePvP.Instance.redTeamList.Count;
        SizeControl(count);
        StartCoroutine(StartBattleCoroutine());

    }
    void OnChangedCumulativeDamage()
    {
        double topDamage = 0;
        if (heroSlotPool.Count > 0)
        {
            heroSlotPool.Sort(delegate(PvPHeroSlot a, PvPHeroSlot b)
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
            if(heroSlotPool[i].gameObject.activeSelf == false)
            {
                slot = heroSlotPool[i];
                break;
            }
        }
        if(slot == null)
        {
            GameObject go = Instantiate(pvpHeroSlotPrefab, scrollViewContent.transform, false);
            slot = go.GetComponent<PvPHeroSlot>();
            heroSlotPool.Add(slot);
        }

        return slot;
    }

    void OnEndBattle()
    {
        for (int i = 0; i < BattlePvP.Instance.redTeamList.Count; i++)
        {
            BattleHero hero = BattlePvP.Instance.redTeamList[i];
            hero.onChangedCumulativeDamage -= OnChangedCumulativeDamage;
        }

        winAnim.gameObject.SetActive(BattlePvP.Instance.isWin);
        lossAnim.gameObject.SetActive(BattlePvP.Instance.isWin == false);

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
        

        StartCoroutine(EndBattleCoroutine());
    }


    IEnumerator StartBattleCoroutine()
    {
        while(true)
        {
            string time = BattlePvP.Instance.pvpReminingTime <= 0 ? "" : BattlePvP.Instance.pvpReminingTime.ToStringTime();
            textBattleTime.text = time;

            if (BattlePvP.Instance.pvpReminingTime <= 0)
                break;
            
            yield return null;
        }
        
    }

    IEnumerator EndBattleCoroutine()
    {
        resultPanel.SetActive(true);

        while (true)
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
        BattlePvP.Instance.isGiveUp = true;
        battleGiveupPanel.SetActive(false);
    }

    public void OnClickGiveUpPanelNoButton()
    {
        battleGiveupPanel.SetActive(false);
    }

    void Close()
    {
        if (battleEndCoroutine != null)
            return;

        battleEndCoroutine = StartCoroutine(BattleEnd());



    }
    Coroutine battleEndCoroutine;
    IEnumerator BattleEnd()
    {
        LoadingManager.ShowFullSceneLoading();
        float idleTime = Time.unscaledTime + 2f;
        while (true)
        {
            float t = idleTime - Time.unscaledTime;
            if (t <= 0)
            {
                BattlePvP.Instance.DespawnHero();
                resultPanel.SetActive(false);
                break;
            }
            yield return null;
        }
        battleEndCoroutine = null;

        LoadingManager.Close();
        SceneManager.UnloadSceneAsync("BattlePvP");

    }

    /// <summary> 출전 영웅 누적 데미지 표시 슬롯 리스트 슬롯 수에 맞게 스크롤 사이즈 조절 </summary>
    void SizeControl(float count)
    {
        if (rectContent == null)
            rectContent = scrollViewContent.GetComponent<RectTransform>();

        double quotient = System.Math.Ceiling((double)count);

        float sizeDeltaY = (scrollViewContent.cellSize.y + scrollViewContent.spacing.y) * ((int)quotient);

        rectContent.sizeDelta = new Vector2(rectContent.sizeDelta.x, sizeDeltaY);
    }

}
