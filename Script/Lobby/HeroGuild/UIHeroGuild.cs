using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.ObscuredTypes;

public class UIHeroGuild : MonoBehaviour {

    [SerializeField]
    Text textRemainingVisitTime;

    [SerializeField]
    GameObject visitedHeroSlotPrefab;

    [SerializeField]
    GridLayoutGroup content;

    RectTransform rectContent;


    //################################################

    /// <summary> 영웅 슬롯 풀  </summary>
    List<UIVisitedHeroSlot> heroSlotPool = new List<UIVisitedHeroSlot>();
    

    void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        Show();
    }

    void OnDisable()
    {
        HeroGuildManager.onAddVisitedHero -= OnAddVisitedHero;
        HeroGuildManager.onRemoveVisitedHero -= OnRemoveVisitedHero;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.HeroGuild)
            Close();
    }


    void Show()
    {
        if (rectContent == null)
            rectContent = content.GetComponent<RectTransform>();

        //if (TerritoryManager.Instance == null)
        //    return;
        //Building building = null;

        //if (TerritoryManager.Instance.placeDic[TerritoryManager.Instance.currentPlaceID].building != null)
        //    building = TerritoryManager.Instance.placeDic[TerritoryManager.Instance.currentPlaceID].building;


        //if (building is BuildingHeroGuild)
        //{
        //    currentHeroGuild = building as BuildingHeroGuild;
        //    currentHeroGuild.onAddVisitedHero += OnAddVisitedHero;
        //    currentHeroGuild.onRemoveVisitedHero += OnRemoveVisitedHero;
        //    //Debug.Log("길드 데이터 존재");

        //    InitVisitedHeroList();
        //}
        HeroGuildManager.onAddVisitedHero += OnAddVisitedHero;
        HeroGuildManager.onRemoveVisitedHero += OnRemoveVisitedHero;
        InitVisitedHeroList();
    }
    /// <summary> 방문 영웅 리스트 초기화 </summary>
    void InitVisitedHeroList()
    {
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            heroSlotPool[i].gameObject.SetActive(false);
        }

        if (HeroGuildManager.Instance == null)
            return;

        for (int i = 0; i < HeroGuildManager.Instance.visitedHeroList.Count; i++)
        {
            // 방문 영웅 slot 생성 & 초기화
            UIVisitedHeroSlot slot = CreateVisitedHeroSlot(HeroGuildManager.Instance.visitedHeroList[i]);
            slot.gameObject.SetActive(true);
        }

        SizeControl(HeroGuildManager.Instance.visitedHeroList.Count);
    }

    /// <summary> 방문 영웅 슬롯 생성 </summary>
    UIVisitedHeroSlot CreateVisitedHeroSlot(VisitedHeroData heroData)
    {
        UIVisitedHeroSlot slot = null;
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            if (heroSlotPool[i].gameObject.activeSelf == false && heroSlotPool[i].heroID == heroData.heroID)
            {
                slot = heroSlotPool[i];
                break;

            }
            else if (heroSlotPool[i].gameObject.activeSelf == false)
            {
                slot = heroSlotPool[i];
                break;
            }
        }

        if (slot == null)
        {
            GameObject go = Instantiate(visitedHeroSlotPrefab);
            go.transform.SetParent(content.transform, false);
            slot = go.GetComponent<UIVisitedHeroSlot>();
            slot.onClickEmploy += OnClickEmploy;
            heroSlotPool.Add(slot);
        }

        slot.initHeroGuildSlot(heroData);

        return slot;
    }

    void OnClickEmploy(string heroID)
    {
        if (string.IsNullOrEmpty(heroID))
            return;

        HeroGuildManager.Instance.Employ(heroID);
    }

    /// <summary> 방문 영웅 추가 </summary>
    void OnAddVisitedHero()
    {
        InitVisitedHeroList();
    }

    /// <summary> 방문 영웅 제거 </summary>
    void OnRemoveVisitedHero()
    {
        InitVisitedHeroList();
    }


    /// <summary> mail content size 조절 </summary>
    void SizeControl(float count)
    {
        count /= 3;
        double quotient = System.Math.Ceiling((double)count);

        rectContent.sizeDelta = new Vector2(0, (content.spacing.y + content.cellSize.y) * (int)quotient);
    }

    private void Update()
    {
        if (HeroGuildManager.Instance == null)
            return;

        if (HeroGuildManager.Instance.remainingVisitTime >= 0f)
        {
            float remainTime = HeroGuildManager.Instance.remainingVisitTime;
            textRemainingVisitTime.text = remainTime.ToStringTimeHMS();
        }   
        else
            textRemainingVisitTime.text = 0f.ToStringTimeHMS();
    }

    public void OnClickCloseButton()
    {
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
    }

    public void Close()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        SceneManager.UnloadSceneAsync("HeroGuild");
    }
}
