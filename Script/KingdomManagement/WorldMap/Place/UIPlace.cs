using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public enum PlaceAlarmType
{
    None,
    Stock,
    DeployedHero,

}

public class UIPlace : MonoBehaviour {


    public Image imageProductIcon;

    public GameObject timeTextPanel;
    public Text timeText;
    public Image plusImage;

    [Header("알람관련")]
    public GameObject alarm;
    public GameObject alarmPanel;
    public GameObject alarmIconPrefab;

    string placeID;

    [Header("속성")]
    [SerializeField]
    GameObject elementalPanel;
    [SerializeField]
    GameObject iconEarth;
    [SerializeField]
    GameObject iconFire;
    [SerializeField]
    GameObject iconWater;
    [SerializeField]
    GameObject iconDark;
    [SerializeField]
    GameObject iconLight;

    [Header("전리품")]
    [SerializeField]
    GameObject itemPanel;
    [SerializeField]
    GameObject iconGreenStone;
    [SerializeField]
    GameObject iconRedStone;
    [SerializeField]
    GameObject iconBlueStone;
    [SerializeField]
    GameObject iconPupleStone;
    [SerializeField]
    GameObject iconOrangeStone;

    [Header("점령 필요 스테이지")]
    [SerializeField]
    GameObject stagePanel;
    [SerializeField]
    Text textStage;



    public Place place
    {
        get
        {
            if (string.IsNullOrEmpty(placeID))
                return null;

            if (WorldMapController.Instance.placeDic.ContainsKey(placeID))
                return WorldMapController.Instance.placeDic[placeID];
            else
                return null;
        }
    }

    public List<UIPlaceAlarmIcon> placeAlarmIconList = new List<UIPlaceAlarmIcon>();

    void Start()
    {
        alarmPanel.SetActive(false);
        
    }
    
    public void InitUIPlace(string _placeID)
    {
        placeID = _placeID;
        place.placeData.onChangedState += OnChangedState;
        place.placeData.onChangedHeroList += OnChangedState;
        

        heroSlotPool = new List<TerritoryDeployedHeroSlot>(deployHeroListParent.GetComponentsInChildren<TerritoryDeployedHeroSlot>());

        OnChangedState();


        InitPlaceElementalTypeIcon();
        InitGetItemIcon();
    }


    public SkeletonGraphic skeletonGraphic;
    [SpineAnimation]
    public string farmer = "Farmer";

    [SpineAnimation]
    public string bushCollector = "BushCollector";


    [SpineAnimation]
    public string fisherman = "Fisherman";

    [SpineAnimation]
    public string mineWorker = "MineWorker";

    [SpineAnimation]
    public string woodCollector = "WoodCollector";

    [SpineAnimation]
    public string woodCutter = "WoodCutter";


    [Header("영웅 배치")]
    public GameObject deployHeroListParent;

    List<TerritoryDeployedHeroSlot> heroSlotPool = new List<TerritoryDeployedHeroSlot>();

    void OnChangedState()
    {
        
        skeletonGraphic.gameObject.SetActive(place.placeData.placeState == PlaceState.MyPlace && place.placeData.heroList.Count > 0);
        if(skeletonGraphic.gameObject.activeSelf)
            InitSkeletonAnimation();
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            heroSlotPool[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < place.placeData.heroList.Count; i++)
        {
            heroSlotPool[i].gameObject.SetActive(true);
            heroSlotPool[i].InitSlot(place.placeData.heroList[i]);
        }
    }

    void InitSkeletonAnimation()
    {
        if (place == null)
            return;
        //skeletonAnimation.state.SetAnimation(0, animFarmer, true);
        //to do : item json 에 애니 정의 해야함
        if (place.placeBaseData.productID == "material_001"
            || place.placeBaseData.productID == "material_006"
            || place.placeBaseData.productID == "material_009"
            || place.placeBaseData.productID == "material_003")
            skeletonGraphic.AnimationState.SetAnimation(0, farmer, true);

        if (place.placeBaseData.productID == "material_015"
             || place.placeBaseData.productID == "material_013"
             || place.placeBaseData.productID == "material_014")
            skeletonGraphic.AnimationState.SetAnimation(0, bushCollector, true);

        if (place.placeBaseData.productID == "material_004")
            skeletonGraphic.AnimationState.SetAnimation(0, fisherman, true);

        if (place.placeBaseData.productID == "material_010"
            || place.placeBaseData.productID == "material_011"
            || place.placeBaseData.productID == "material_002"
            || place.placeBaseData.productID == "material_007"
            || place.placeBaseData.productID == "material_005")
            skeletonGraphic.AnimationState.SetAnimation(0, mineWorker, true);

        if (place.placeBaseData.productID == "material_008"
            || place.placeBaseData.productID == "material_006"
            || place.placeBaseData.productID == "material_009")
            skeletonGraphic.AnimationState.SetAnimation(0, woodCollector, true);

        if (place.placeBaseData.productID == "material_012")
            skeletonGraphic.AnimationState.SetAnimation(0, woodCutter, true);

    }

    void InitPlaceElementalTypeIcon()
    {
        
        iconEarth.SetActive(false);  
        iconFire.SetActive(false);        
        iconWater.SetActive(false);      
        iconDark.SetActive(false);       
        iconLight.SetActive(false);

        if (string.IsNullOrEmpty(placeID))
            return;

        PlaceBaseData data = GameDataManager.placeBaseDataDic[placeID];
        for (int i = 0; i < data.placeElementalTypeList.Count; i++)
        {
            if (data.placeElementalTypeList[i] == PlaceElementalType.Earth)
                iconEarth.SetActive(true);
            else if (data.placeElementalTypeList[i] == PlaceElementalType.Fire)
                iconFire.SetActive(true);
            else if (data.placeElementalTypeList[i] == PlaceElementalType.Water)
                iconWater.SetActive(true);
            else if (data.placeElementalTypeList[i] == PlaceElementalType.Dark)
                iconDark.SetActive(true);
            else if (data.placeElementalTypeList[i] == PlaceElementalType.Light)
                iconLight.SetActive(true);
        }

        //elementalPanel.SetActive(false);
    }

    void InitGetItemIcon()
    {
        iconGreenStone.SetActive(false);
        iconRedStone.SetActive(false);
        iconBlueStone.SetActive(false);
        iconPupleStone.SetActive(false);
        iconOrangeStone.SetActive(false);

        if (string.IsNullOrEmpty(placeID))
            return;

        //PlaceBaseData data = GameDataManager.placeBaseDataDic[placeID];
        //for (int i = 0; i < data.getItemIDList.Count; i++)
        //{
        //    if (data.getItemIDList[i] == "enhancePointA")
        //        iconBlueStone.SetActive(true);
        //    else if (data.getItemIDList[i] == "enhancePointB")
        //        iconGreenStone.SetActive(true);
        //    else if (data.getItemIDList[i] == "enhancePointC")
        //        iconOrangeStone.SetActive(true);
        //    else if (data.getItemIDList[i] == "enhancePointD")
        //        iconPupleStone.SetActive(true);
        //    else if (data.getItemIDList[i] == "enhancePointE")
        //        iconRedStone.SetActive(true);
        //}

      
        //itemPanel.SetActive(false);
    }


    public void ChangeIconColor(Color color)
    {
        if(place != null)
        {
            //stagePanel.SetActive(place.placeState == PlaceState.Enable);
            //textStage.text = "stage " + place.placeAddNeedStageCount.ToString() + "+";

        }

        iconEarth.GetComponent<Image>().color = color;
        iconFire.GetComponent<Image>().color = color;
        iconWater.GetComponent<Image>().color = color;
        iconDark.GetComponent<Image>().color = color;
        iconLight.GetComponent<Image>().color = color;

        iconBlueStone.GetComponent<Image>().color = color;
        iconGreenStone.GetComponent<Image>().color = color;
        iconOrangeStone.GetComponent<Image>().color = color;
        iconPupleStone.GetComponent<Image>().color = color;
        iconRedStone.GetComponent<Image>().color = color;
    }
}
