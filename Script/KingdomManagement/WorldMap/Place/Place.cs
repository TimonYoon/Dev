using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using LitJson;

public enum PlaceState
{
    None,
    Enable,
    MyPlace,
}


/// <summary> 영지 </summary>
public class Place : MonoBehaviour
{
    [SerializeField]
    string _placeID;
    /// <summary> 해당 장소의 고유값 </summary>
    public string placeID { get { return _placeID; } set { _placeID = value; } }

    PlaceState _placeState = PlaceState.None;
    /// <summary> 영지 상태 </summary>
    public PlaceState placeState
    {
        get
        {
            return _placeState;
        }
        private set
        {
            switch (value)
            {
                case PlaceState.None:                 
                    break;
                case PlaceState.Enable:
                    if (_placeState == PlaceState.None)
                        _placeState = value;
                    break;
                case PlaceState.MyPlace:
                    _placeState = value;
                    ConnectedPlaceEnable();
                    break;
                default:
                    break;
            }
            UpdatePlaceUI();
        }
    }

    /// <summary> 영지 정적 데이터 </summary>
    public PlaceBaseData placeBaseData { get { return placeData.placeBaseData; } }
    
    /// <summary> 해당 지역과 연결된 지역리스트 (null 예외처리 꼭 하시오) </summary>
    [Header("해당 영지와 연결된 영지")]
    public List<Place> connectedPlaceList;    

    /// <summary> 영지 레벨 </summary>
    public int placeLevel { get { return placeData.placeLevel; } }

    //-----------------------------------------------------------------------

    /// <summary> 영지 spriteRanderer </summary>
    SpriteRenderer placeSpriteRenderer = null;

    /// <summary> 건물 이미지 </summary>
    Image imageProductIcon;


    [SerializeField]
    Text nameText;

    [SerializeField]
    UIPlace uiPlace = null;

    /// <summary> disable 상태의 영지 색표현 </summary>
    Color disablePlaceColor = new Color(0.4f, 0.4f, 0.4f, 1);

    string availableHex = "#FFA22DFF";

    //-----------------------------------------------------------------------

    /// <summary> 이름 그라데이션 </summary>
    GradientColor nameGradientColor;

    string disableFirstHex = "#7C7C7CFF";
    string disableSecondHex = "#605D52FF";

    // 비 활성화 name color
    Color disableNameFirstColor;
    Color disableNameSecondColor;

    // 활성화 name color
    Color origialNameFirstColor;
    Color origialNameSecondColor;

    //--------------------------------------------------------------------------

    void Awake()
    {
        if (nameGradientColor == null)
        {
            nameGradientColor = nameText.GetComponent<GradientColor>();

            ColorUtility.TryParseHtmlString(disableFirstHex, out disableNameFirstColor);
            ColorUtility.TryParseHtmlString(disableSecondHex, out disableNameSecondColor);

            origialNameFirstColor = nameGradientColor.firstColor;
            origialNameSecondColor = nameGradientColor.secondColor;
        }

        if (placeSpriteRenderer == null)
            placeSpriteRenderer = GetComponent<SpriteRenderer>();

        imageProductIcon = uiPlace.imageProductIcon;
        imageProductIcon.CrossFadeAlpha(0, 0, true);
        imageProductIcon.GetComponent<Button>().onClick.AddListener(OnClickPlace);
        imageProductIcon.RegisterDirtyMaterialCallback(OnChangedImage);
    }
    void OnChangedImage()
    {
        imageProductIcon.CrossFadeAlpha(1f, 0.2f, true);
    }

    IEnumerator Start()
    {
        while (GameDataManager.Instance == false)
            yield return null;

        while (!WorldMapController.Instance.isInitialized)
            yield return null;
            
        if (placeBaseData != null)
        {
            string buildingImageName = GameDataManager.itemDic[placeBaseData.productID].image;
            AssetLoader.AssignImage(imageProductIcon,"sprite/material", "Atlas_Material", buildingImageName);
        }

        uiPlace.InitUIPlace(placeID);
        
        nameText.text = GameDataManager.placeBaseDataDic[placeID].name;

    }

    public PlaceData placeData { get; private set; }
    /// <summary> 해당 영지가 서버에 저장되어있을 때 초기화해줌  </summary>
    public void InitPlace(PlaceData _placeData)
    {
        placeData = _placeData;

        placeData.onChangedPlaceLevel += OnChangedPlaceLevel;
        placeData.onChangedState += OnChangedState;

        placeState = placeData.placeState;        
    }

    void OnChangedPlaceLevel()
    {

    }

    void OnChangedState()
    {
        placeState = placeData.placeState;
        
    }

    /// <summary> 영지의 상태에 따라 UI 초기화 준다</summary>
    void UpdatePlaceUI()
    {
        if (placeState == PlaceState.MyPlace)
        {
            uiPlace.ChangeIconColor(new Color(1, 1, 1, 1));
            nameGradientColor.firstColor = origialNameFirstColor;
            nameGradientColor.secondColor = origialNameSecondColor;
            imageProductIcon.color = new Color(1, 1, 1, 1f);
            placeSpriteRenderer.color = Color.white;
        }
        else if(placeState == PlaceState.Enable)
        {
            uiPlace.ChangeIconColor(new Color(1, 1, 1, 1));
            Color color = Color.white;
            ColorUtility.TryParseHtmlString(availableHex, out color);
            placeSpriteRenderer.color = color;
            imageProductIcon.color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        else
        {
            uiPlace.ChangeIconColor(new Color(1, 1, 1, 0.5f));
            nameGradientColor.firstColor = disableNameFirstColor;
            nameGradientColor.secondColor = disableNameSecondColor;
            imageProductIcon.color = new Color(1, 1, 1, 0.5f);
            placeSpriteRenderer.color = disablePlaceColor;
        }
    }

    

    /// <summary> 해당 영지 클릭했을 때 </summary>
    public void OnClickPlace()
    {
        switch (placeState)
        {
            case PlaceState.None:
                UIPlaceInfo.Instance.Show(placeID, placeInfoType.OnlyInfo);
                //Debug.Log(placeID + ") 비활성화 영지입니다.");
                break;
            case PlaceState.Enable:
                UIPlaceInfo.Instance.Show(placeID, placeInfoType.AddPlace);
                //Debug.Log(placeID + ") 미획득 영지입니다.");
                break;
            case PlaceState.MyPlace:
                UIPlaceInfo.Instance.Show(placeID, placeInfoType.EmptyPlace);
                //Debug.Log(placeID + ") 획득한 영지입니다.");
                break;
            default:
                break;
        }   
    }

    public double addCost { get { return TerritoryManager.placeCost; } }

    public double upgradeCost { get { return TerritoryManager.placeCost; } }


    Coroutine placeAddCoroutine;
    /// <summary> 영지 추가 </summary>
    public void PlaceAdd()
    {
        if (placeAddCoroutine != null)
            return;
        placeAddCoroutine = StartCoroutine(PlaceAddCoroutine());
    }
    
    /// <summary> 영지 추가 </summary>
    IEnumerator PlaceAddCoroutine()
    {
        

        string result = null;
        yield return StartCoroutine(TerritoryServerConnect(TerritoryManager.territoryPHPConnectType.PlaceAdd, placeID,x => result = x, addCost));
        placeAddCoroutine = null;
        // 서버 채크
        if (result == null)
        {
            UIPopupManager.ShowInstantPopup("[" + GameDataManager.placeBaseDataDic[placeID].name + "]를 점령!!");
        }
        else if(result == "NoMoney")
        {
            UIPopupManager.ShowInstantPopup("재화가 부족합니다.");
        }
    }
    

    Coroutine placeUpgradeCoroutine;
    /// <summary> 영지 업그레이드 </summary>
    public void PlaceUpgrade()
    {
        if (placeUpgradeCoroutine != null)
            return;

        placeUpgradeCoroutine = StartCoroutine(PlaceUpgradeCoroutine());
    }

    IEnumerator PlaceUpgradeCoroutine(int amount = 1)
    {
        string result = null;
        yield return StartCoroutine(TerritoryServerConnect(TerritoryManager.territoryPHPConnectType.PlaceUpgrade, placeID, x => result = x, upgradeCost, amount));
        placeUpgradeCoroutine = null;
        // 서버 채크
        if (result == null)
        {
            UIPopupManager.ShowInstantPopup("[" + GameDataManager.placeBaseDataDic[placeID].name + "]를 ["+ placeLevel + "]레벨으로 강화 완료!!");
        }
        else if (result == "NoMoney")
        {
            UIPopupManager.ShowInstantPopup("재화가 부족합니다.");
        }
    }

    /// <summary> 영지 관련 서버 통신 부분 </summary>
    public IEnumerator TerritoryServerConnect(TerritoryManager.territoryPHPConnectType type, string placeID, Action<string> _result, double cost, int upgradeLevel = 1)
    {
        string php = "Territory.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", (int)type);

        form.AddField("upgradeLevel", upgradeLevel);
        form.AddField("cost", (-cost).ToString());

        if (!string.IsNullOrEmpty(placeID))
            form.AddField("placeID", placeID);

        string result = null;
        string error = null;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x, x => error = x));

        if (error != null)
        {
            Debug.Log("territory.php error : " + error);
        }


        if (_result != null)
            _result(result);
    }

    /// <summary> 영지 활성화 </summary>
    public void PlaceEnable()
    {
        placeState = PlaceState.Enable;
    }

    /// <summary> 연결된 영지 활성화 </summary>
    void ConnectedPlaceEnable()
    {
        if (connectedPlaceList == null) return;

        for (int i = 0; i < connectedPlaceList.Count; i++)
        {
            connectedPlaceList[i].PlaceEnable();
        }
    }

    
}
