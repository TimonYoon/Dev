using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// 로비 상태
public enum LobbyState
{    
    
    //Attendance,
    //Achievement,
    //Mail,
    None,
    Hero,
    Lobby,
    Battle,
    BattlePreparation,
    Territory,
    Shop,
    
    //Option,

    SubMenu,
    WorldMap,
    HeroTraining,
    HeroGuild,
    PVPBattle,
    PvPBattleLobby,


    DayDungeonLobby,
    DayDungeon,


};

/// <summary> 서브메뉴 상태 </summary>
public enum SubMenuState
{
    None,
    Option,
    Attendance,
    Mail,
    Achievement,
    IllustratedBook,
    PackageBox,
    Dictionary,
    Building,
    Market,
    Notice,
    UserQuest,
    Rank,
    PvP,
}

public class SceneLobby : MonoBehaviour {

    public static SceneLobby Instance;
    public static LobbyState currentState { get; private set; }


    /// <summary> 서브 메뉴 상태 </summary>
    public static SubMenuState currentSubMenuState { get; private set; }

    public Toggle shopToggle;
    public Toggle heroToggle;
    public Toggle worldToggle;
    public Toggle battleToggle;



    public delegate void ChangedMenu(LobbyState state);
    public ChangedMenu OnChangedMenu;


    //public delegate void OnBuildingSearch();
    //public OnBuildingSearch onBuildingSearch;

    public Toggle subMenuToggle;
    public Transform subMenuTransfrom;

    [SerializeField]
    Transform heroMenuTransform;

    [SerializeField]
    Canvas dailyMissionCanvas;

    public Transform territoryMenuTransfrom;

    //public GameObject searchButton;

    void Awake()
    {
        Instance = this;
    }



    void OnEnable()
    {
        UISceneContraller.onSceneContrallerCallBack += SceneChange;
        //searchButton.SetActive(false);
    }
    void OnDisable()
    {
        UISceneContraller.onSceneContrallerCallBack -= SceneChange;
    }

    bool isQuitShowPopup = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isQuitShowPopup)
                return;

            isQuitShowPopup = true;
            UIPopupManager.ShowYesNoPopup("가지마~", "그만 할꺼에요??", GameQuit);
        }

        if(Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteKey("userID");
        }
    }
    void GameQuit(string result)
    {
        if(result == "yes")
        {
            Application.Quit();
        }
        else if(result == "no")
        {
            isQuitShowPopup = false;
        }
    }
    public bool isBuildingMode { get; private set; }

    
    //public void OnClickBuildingModeButton()
    //{
    //    //Debug.Log("빌딩모드 클릭");
    //    //isBuildingMode = !isBuildingMode;
    //    //if (onBuildingMode != null)
    //    //    onBuildingMode(isBuildingMode);
    //    if (isBuildingMode == false)
    //        SceneChange(LobbyState.SubMenu, SubMenuState.Building);
    //    else
    //        SceneChange(LobbyState.Lobby,SubMenuState.Building);
    //    //if (!isBuildingMode)
    //    //    SceneChange(LobbyState.Lobby);

    //    //searchButton.SetActive(isBuildingMode);


    //}
    //public void OnClickBuildingSearchButton()
    //{
    //    if (onBuildingSearch != null)
    //        onBuildingSearch();
    //}

    private IEnumerator Start()
    {        
        //lobbyButton.SetActive(false);
        //territoryButton.SetActive(true);

#if UNITY_EDITOR
        Application.runInBackground = true;
#endif

        yield return StartCoroutine(ShowScene("scene/territory", "Territory", true));

        while (!TerritoryManager.Instance.isInitialized)
            yield return null;

        yield return StartCoroutine(ShowScene("scene/heroinventory", "HeroInventory", true));

        yield return StartCoroutine(ShowScene("scene/battle", "Battle", true));

        while (!Battle.Instance.isInitialized)
            yield return null;

        SceneChange(LobbyState.Battle);

        //SceneChange(LobbyState.Territory);

        yield return null;



        yield return StartCoroutine(LoadingManager.FadeInScreen());

        if (AttendanceManager.Instance.isAttendance)
            yield return StartCoroutine(ShowScene("scene/attendance", "Attendance", true));

        if (NoticeController.Instance)
            yield return StartCoroutine(NoticeController.Instance.InitNotice());
    }

    public IEnumerator ShowScene(string assetBundle, string sceneName, bool isAdditive = true)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            while (!AssetLoader.Instance)
                yield return null;

            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync(assetBundle, sceneName, isAdditive));

        }
        yield break;
    }
   
    
    /// <summary> 상점에서 원하는 타입의 상점 보여주기 </summary>
    public void ShowShop(ShopType type)
    {
        StartCoroutine(ShowShopCoroutine(type));
    }
    
    IEnumerator ShowShopCoroutine(ShopType type)
    {
        yield return StartCoroutine(ShowScene("scene/shop", "Shop", true));
        //Debug.Log("여기");
        SceneChange(LobbyState.Shop);
       
        while (!ShopDataController.Instance)
            yield return null;

        ShopDataController.Instance.ShowShop(type);
    }
    
    public void OnClickDailyMissionButton()
    {
        dailyMissionCanvas.enabled = !dailyMissionCanvas.enabled;
    }

    public void OnClickSubMenuButton()
    {
        subMenuTransfrom.gameObject.SetActive(subMenuToggle.isOn);
    }

    public void OnClickHeroMenuButton()
    {
        heroMenuTransform.gameObject.SetActive(heroToggle.isOn);
    }

    public void SceneChange(LobbyState state = LobbyState.Battle, SubMenuState subMenuState = SubMenuState.None)
    {        

        if (currentState == state)
            return;
        //Debug.Log("들어옴" + state.ToString());
        if(state == LobbyState.Lobby)
        {
            if (currentState != LobbyState.Lobby)
            {
                if (currentState == LobbyState.BattlePreparation)
                    state = LobbyState.Battle;
                else
                    state = currentState;
            }
        }

        switch (state)
        {
            case LobbyState.Shop:
                shopToggle.isOn = true;
                break;
            case LobbyState.Hero:
                heroToggle.isOn = true;
                break;
            case LobbyState.Lobby:
                worldToggle.isOn = true;
                break;
            case LobbyState.Battle:
                battleToggle.isOn = true;
                break;
            case LobbyState.BattlePreparation:
                //state = LobbyState.Battle;
                break;
            case LobbyState.Territory:
                worldToggle.isOn = true;
                break;
            case LobbyState.SubMenu:
                state = currentState;
                break;
            default:
                break;
        }

        subMenuTransfrom.gameObject.SetActive(false);
        subMenuToggle.isOn = false;
        if (territoryMenuTransfrom != null)
            territoryMenuTransfrom.gameObject.SetActive(state == LobbyState.Territory);

        heroMenuTransform.gameObject.SetActive(false);

        currentState = state;
        currentSubMenuState = subMenuState;
        if (OnChangedMenu != null)
            OnChangedMenu(currentState);

    }

    public void SubMenuChange(SubMenuState subMenuState = SubMenuState.None)
    {

    }
}
