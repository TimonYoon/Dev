using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIBattlePreparation : MonoBehaviour {

    static public UIBattlePreparation Instance;

    public RectTransform rectTransformScreen;

    public Button buttonStart;

    Canvas canvas;

    Scene scene;

    static public UIHeroSelectSlot emptySlot;

    static public List<UIHeroSelectSlot> heroSelectSlotList;// = new List<UIHeroSelectSlot>();

    /// <summary> 전투에 출전 시킬 영웅 리스트 </summary>
    static public CustomList<HeroData> selectedHeroDataList = new CustomList<HeroData>();

    public GameObject toggledObjectNewBattle;
    public GameObject toggledObjectEditBattle;

    void Awake()
    {
        Instance = this;

        scene = SceneManager.GetSceneByName("BattlePreparation");
        GameObject[] objs = scene.GetRootGameObjects();

        //캔버스 찾기
        for (int i = 0; i < objs.Length; i++)
        {
            canvas = objs[i].GetComponent<Canvas>();
            if (canvas)
            {
                canvas.worldCamera = Camera.main;
                break;
            }
        }

        UIHeroSelectSlot[] slots = canvas.GetComponentsInChildren<UIHeroSelectSlot>();
        if (slots != null)
        {
            heroSelectSlotList = new List<UIHeroSelectSlot>(slots);
            //Debug.Log(heroSelectSlotList.Count);
        }

        
    }

    void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedLobbyMenu;

        selectedHeroDataList.onAdd += OnAddHero;
    }

    void OnDisable()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedLobbyMenu;

        selectedHeroDataList.onAdd -= OnAddHero;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    /// <summary> 전투 출전 영웅 리스트에서 추가 </summary>
    public static void AddHero(HeroData heroData)
    {
        //5명 꽉 찼으면 패스
        if (selectedHeroDataList.Count >= 8)
        {
            //UIPopupManager.ShowOKPopup("알림", "출전 영웅 정원 초과", null);
            return;
        }

        //이미 추가된 경우 패스. 옴??
        if (selectedHeroDataList.Exists(x => x == heroData))
            return;
                

        //출전 리스트에 추가
        selectedHeroDataList.Add(heroData);
        
        UpdatePreviewSlots();
    }

    void OnAddHero(HeroData heroData)
    {
        //가장 앞 순서의 빈 슬롯 
        emptySlot = heroSelectSlotList.Find(x => string.IsNullOrEmpty(x.id));
        //emptySlot.AssignHero(heroData);

        //시작 버튼 활성
        Instance.buttonStart.interactable = selectedHeroDataList.Count > 0;
    }

    /// <summary> 전투 출전 영웅 리스트에서 제거 </summary>
    public static void RemoveHero(HeroData heroData)
    {        
        selectedHeroDataList.Remove(heroData);
        Instance.buttonStart.interactable = selectedHeroDataList.Count > 0;

        UpdatePreviewSlots();
    }

    static public string battleGroupID;
    static public void Show(string _battleGroupID, List<BattleHero> list = null)
    {
        battleGroupID = _battleGroupID;

        //새로운 배틀 그룹인지 아닌지에 따라 표시되는 오브젝트 다름
        bool isNewBattle = true;
        if (Battle.currentBattleGroup && battleGroupID == Battle.currentBattleGroup.battleType)
            isNewBattle = false;

        Instance.toggledObjectNewBattle.SetActive(isNewBattle);
        Instance.toggledObjectEditBattle.SetActive(!isNewBattle);

        //확인 버튼은 최소 한 명 이상 출전 한 상태여야 누를 수 있음
        Instance.buttonStart.interactable = selectedHeroDataList.Count > 0;

        //씬 액티브 해야 스카이박스가 보임
        //SceneManager.SetActiveScene(Instance.scene);

        //인벤토리 부분의 사이즈를 조절해서 보여줌. (화면 절반 정도?)
        RectTransform t = UIHeroInventory.Instance.objListRoot.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(t.sizeDelta.x, -Instance.rectTransformScreen.sizeDelta.y - t.anchoredPosition.y);

        //캔버스 보여주기
        Instance.canvas.gameObject.SetActive(true);

        //화면 전환
        if(!isNewBattle)
            Battle.lastBattleGroupID = battleGroupID;

        SceneLobby.Instance.SceneChange(LobbyState.BattlePreparation);

        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                selectedHeroDataList.Add(list[i].heroData);
            }
        }
        else
            selectedHeroDataList.Clear();

        UpdatePreviewSlots();
    }

    static public void UpdatePreviewSlots()
    {
        if (selectedHeroDataList.Count > 8)
            return;

        if (Instance.coroutineUpdatePreviewSlots != null)
        {
            Instance.StopCoroutine(Instance.coroutineUpdatePreviewSlots);
            Instance.coroutineUpdatePreviewSlots = null;
        }

        Instance.StartCoroutine(Instance.UpdatePreviewSlotsA());
    }

    Coroutine coroutineUpdatePreviewSlots = null;

    IEnumerator UpdatePreviewSlotsA()
    {
        //LoadingManager.Show();



        for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
        {
            UIHeroSlotContainer s = UIHeroInventory.heroSlotContainerList[i];
            bool isChack = false;            

            for (int j = 0; j < selectedHeroDataList.Count; j++)
            {
                if (selectedHeroDataList[j].id == s.heroInvenID)
                {
                    s.isSelectedToBattle = true;
                    isChack = true;
                    break;
                }                    
            }

            if (isChack == false)
            {
                s.isSelectedToBattle = false;
            }

            s.isUnableToSelect = selectedHeroDataList.Count >= 8;
        }




        //for (int i = 0; i < heroSelectSlotList.Count; i++)
        //{
        //    UIHeroSelectSlot s = heroSelectSlotList[i];
        //    bool isChack = false;
        //    HeroData heroData = null;
        //    for (int j = 0; j < selectedHeroDataList.Count; j++)
        //    {
        //        heroData = selectedHeroDataList[j];
        //        if (selectedHeroDataList[j].id == s.id)
        //        {
        //            isChack = true;   
        //            break;                    
        //        }
        //        else if(string.IsNullOrEmpty(s.id))
        //        {
        //            GameObject go = null;
        //            yield return (CharacterEmptyPool.Instance.GetHero(heroData.heroID, x => go = x));
        //            heroSelectSlotList[i].AssignHero(heroData.id, go);

        //        }
        //    }
        //    if (isChack == false)
        //    {
        //        s.Dispose();
        //    }
        //}

        //for (int i = 0; i < heroSelectSlotList.Count; i++)
        //{
        //    if (i >= selectedHeroDataList.Count)
        //    {
        //        heroSelectSlotList[i].Dispose();
        //        break;
        //    }

        //    HeroData heroData = selectedHeroDataList[i];

        //    if (heroSelectSlotList[i].id == heroData.id)
        //        continue;

        //    if (heroSelectSlotList[i].heroObj != null)
        //        heroSelectSlotList[i].Dispose();

        //    GameObject go = null;
        //    yield return (CharacterEmptyPool.Instance.GetHero(heroData.heroID, x => go = x));

        //    heroSelectSlotList[i].AssignHero(heroData.id, go);
        //}

        //coroutineUpdatePreviewSlots = null;
        //for (int i = 0; i < heroSelectSlotList.Count; i++)
        //{
        //    for (int j = 0; j < UIHeroInventory.heroSlotContainerList.Count; j++)
        //    {
        //        UIHeroSlotContainer s = UIHeroInventory.heroSlotContainerList[j];
        //        if (s.heroInvenID == heroSelectSlotList[i].id)
        //        {
        //            if (selectedHeroDataList.Find(x => x.id == s.heroInvenID) == null)
        //                s.isSelectedToBattle = false;
        //        }
        //    }
        //}

        for (int i = 0; i < heroSelectSlotList.Count; i++)
        {
            if (i >= selectedHeroDataList.Count)
            {
                heroSelectSlotList[i].Dispose();
                break;
            }

            HeroData heroData = selectedHeroDataList[i];

            if (heroSelectSlotList[i].id == heroData.id)
                continue;

            if (heroSelectSlotList[i].heroObj != null)
                heroSelectSlotList[i].Dispose();

            GameObject go = null;
            yield return StartCoroutine(CharacterEmptyPool.Instance.GetHero(heroData.heroID, x => go = x));

            heroSelectSlotList[i].AssignHero(heroData.id, go);
        }

        for (int i = 0; i < heroSelectSlotList.Count; i++)
        {
            for (int j = 0; j < UIHeroInventory.heroSlotContainerList.Count; j++)
            {
                UIHeroSlotContainer s = UIHeroInventory.heroSlotContainerList[j];
                if (s != null && s.heroInvenID == heroSelectSlotList[i].id)
                {
                    s.isSelectedToBattle = true;
                }
            }
        }

        ////LoadingManager.Close();

        coroutineUpdatePreviewSlots = null;
    }

    void Close()
    {
        Scene sceneLobby = SceneManager.GetSceneByName("Lobby");
        SceneManager.SetActiveScene(sceneLobby);

        canvas.gameObject.SetActive(false);

        selectedHeroDataList.Clear();

        for (int i = 0; i < heroSelectSlotList.Count; i++)
        {
            heroSelectSlotList[i].Dispose();//.id = string.Empty;
        }
    }

    void OnChangedLobbyMenu(LobbyState menu)
    {
        if (menu == LobbyState.BattlePreparation)
            return;

        if(Instance.canvas.gameObject.activeSelf)
            Close();
    }

    //시작 버튼 클릭 시
    public void OnClickStartButton()
    {
        Instance.buttonStart.interactable = false;

        StartCoroutine(StartExploration());
    }

    //IEnumerator CloseAfterRun()
    //{

    //    for (int i = 0; i < heroSelectSlotList.Count; i++)
    //    {
    //        if (heroSelectSlotList[i].battleHero != null)
    //            heroSelectSlotList[i].battleHero.skeletonAnimation.state.SetAnimation(0, heroSelectSlotList[i].battleHero.runAnimation, true);
    //    }
        
    //    yield return new WaitForSeconds(1.7f);
    //}


    /// <summary> 영웅 출전 </summary>
    IEnumerator StartExploration()
    {
        bool isNewBattle = Battle.currentBattleGroup.battleType != battleGroupID;

        //던전 아이디
        //string dungeonID = Battle.currentBattleGroupID;

        //선택된 영웅을 해당 씬에 배치하면서 전투 시작
        if (selectedHeroDataList == null || selectedHeroDataList.Count == 0)
        {
            Debug.Log("선택한 영웅이 없습니다.");
            yield break;
        }

        for (int i = 0; i < heroSelectSlotList.Count; i++)
        {
            if (heroSelectSlotList[i].battleHero != null)
                heroSelectSlotList[i].battleHero.skeletonAnimation.state.SetAnimation(0, heroSelectSlotList[i].battleHero.spineAnimationRun/*runAnimation*/, true);
        }


        //yield return new WaitForSeconds(2.2f);

        for (int i = 0; i < selectedHeroDataList.Count; i++)
        {
            HeroData d = HeroManager.heroDataDic[selectedHeroDataList[i].id];
            d.battleGroupID = battleGroupID;
            //d.isUsing = true;
        }
                
        //리스트 새로 만들어서 배틀그룹 생성
        List<HeroData> heroList = new List<HeroData>(selectedHeroDataList);

        if (isNewBattle)
        {
            UIFadeController.FadeOut(1f);

            while (!UIFadeController.isFinishFadeOut)
                yield return null;
        }

        //Battle.CreateBattleGroup(battleGroupID, heroList);

        BattleGroup battleGroup = Battle.battleGroupList.Find(x => x.battleType == battleGroupID);

        //새로운 배틀그룹 생성
        if (!battleGroup)
        {
            Battle.CreateBattleGroup(battleGroupID, heroList);

            battleGroup = Battle.battleGroupList.Find(x => x.battleType == battleGroupID);

            while (!battleGroup.isInitialized)
                yield return null;
        }
        //기본 배틀그룹의 멤버 변경
        else
        {

            
            //기존 멤버중 없던애는 배틀 그룹에 새로 추가 & 스폰
            for (int i = 0; i < heroList.Count; i++)
            {
                //기존 멤버는 가만히 냅둠
                if (battleGroup.originalMember.Find(x => x.heroData == heroList[i]))
                    continue;
                
                BattleHero battleHero = null;
                yield return StartCoroutine(actorPool.Instance.GetActor(heroList[i].heroID, x => battleHero = x));

                if (!battleHero)
                    continue;

                battleHero.team = BattleUnit.Team.Red;
                battleHero.Init(battleGroup, heroList[i]);
                battleHero.gameObject.SetActive(true);
                battleHero.ReGen();

                battleGroup.redTeamList.Add(battleHero);
                battleGroup.originalMember.Add(battleHero);
            }

            //기존 멤버 중 제외된 애는 배틀그룹에서 제외
            for (int i = battleGroup.originalMember.Count - 1; i >= 0; i--)
            {
                HeroData heroData = heroList.Find(x => x == battleGroup.originalMember[i].heroData);
                if (heroData != null)
                    continue;

                BattleHero h = battleGroup.originalMember[i];
                h.heroData.battleGroupID = string.Empty;
                h.Despawn();
                //battleGroup.originalMember[i].SetBattleGroup(null);
                //battleGroup.originalMember[i].gameObject.SetActive(false);
                battleGroup.originalMember.Remove(h);
                battleGroup.redTeamList.Remove(h);

                //h.heroData.battleGroupID = string.Empty;
                //h.Despawn();
            }

            //데이타 저장
            Battle.SaveStageInfo(battleGroup);

        }
        

        //Battle.ShowBattle(battleGroupID);

        //이렇게 안 하면 가장 마지막에 봤던 전투그룹으로 돌아가서 출전 후 다른 전투그룹이 비쳐짐
        Battle.lastBattleGroupID = battleGroupID;

        //전투 화면으로 돌아가고 창 닫기
        SceneLobby.Instance.SceneChange(LobbyState.Battle);
        Close();

        //한프레임 쉬어야 프리킹 현상 없음
        yield return null;

        //페이드 인
        if(isNewBattle)
            UIFadeController.FadeIn(1f);
    }


    public void OnClickClose()
    {
        Close();

        SceneLobby.Instance.SceneChange(LobbyState.Battle);
    }
}
