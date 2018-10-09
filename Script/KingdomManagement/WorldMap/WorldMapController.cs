using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapController : MonoBehaviour {

    public static WorldMapController Instance;
    
    public Canvas worldMapCanvas;

    public Canvas uiCanvas;

    public Camera mapCamera;

    bool isTouch = false;

    RaycastHit2D hit;

    Vector3 touchPos;

    
    

    void Awake()
    {
        Instance = this;    
    }

    /// <summary> 전체 영지 딕셔너리 </summary>
    public Dictionary<string, Place> placeDic { get; private set; }

    public bool isInitialized { get; private set; }

    IEnumerator Start ()
    {
        while (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("WorldMap").isLoaded)
            yield return null;

        while (!SceneLobby.Instance)
            yield return null;

        mapCamera.enabled = true;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        InitTerritory();
        Show();
        isInitialized = true;
    }

    void OnChangedMenu(LobbyState state)
    {
        RendererSwitch(state == LobbyState.WorldMap);
    }

    /// <summary> 영지 초기화 </summary>
    void InitTerritory()
    {
        List<Place> placeList = new List<Place>(worldMapCanvas.GetComponentsInChildren<Place>());

        placeDic = new Dictionary<string, Place>();

        for (int i = 0; i < placeList.Count; i++)
        {
            placeDic.Add(placeList[i].placeID, placeList[i]);
        }
    }

    public delegate void OnClickShowTerritoryInfo();
    public OnClickShowTerritoryInfo onShowTerritorInfo;

    public void OnClickTerritoryButton()
    {
        if (onShowTerritorInfo != null)
            onShowTerritorInfo();
    }

    

    public void Show()
    {
        
        for (int i = 0; i < TerritoryManager.Instance.placeDataList.Count; i++)
        {
            PlaceData place = TerritoryManager.Instance.placeDataList[i];

            if (placeDic.ContainsKey(place.placeID))
            {
                placeDic[place.placeID].InitPlace(place);
            }
        }
    }

    /// <summary> 월드 맵 랜더러 on/off 스위치 </summary>
    public void RendererSwitch(bool value)
    {
        if (worldMapCanvas.enabled == value)
            return;

        worldMapCanvas.enabled = value;
        uiCanvas.enabled = value;

        SpriteRenderer[] spriteRenderer = worldMapCanvas.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderer != null || spriteRenderer.Length > 0)
        {
            for (int i = 0; i < spriteRenderer.Length; i++)
            {
                spriteRenderer[i].enabled = value;
            }
        }

    }

    private void Update()
    {
        /// 영지 터치 부분
        if (UIDraw.Instance == null && (SceneLobby.currentState == LobbyState.WorldMap))
        {


#if !UNITY_EDITOR // 폰에서만 작동
            if (Input.touchCount ==1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    touchPos = Input.mousePosition;
                }
            }


            if (Input.touchCount ==1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    // Handle Touch
                    float dis = Vector3.Distance(touchPos, Input.mousePosition);
                    if (dis < 10f)
                    {
                        Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
                        hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                        isTouch= true;
                    }
                }                      
            }
#endif
#if UNITY_EDITOR

            if (Input.GetMouseButtonDown(0))
            {
                touchPos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    float dis = Vector3.Distance(touchPos, Input.mousePosition);
                    if (dis < 10f)
                    {
                        //LayerMask  EnemyLayer = LayerMask.NameToLayer("Lobby");
                        Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
                        hit = Physics2D.GetRayIntersection (ray, Mathf.Infinity, 1 << LayerMask.NameToLayer("Lobby"));
                        //Debug.Log("hit : " + hit.collider.gameObject.name);
                        isTouch = true;


                    }
                }
            }
#endif

            if (isTouch)
            {
                if (hit.collider != null)
                {
                    Place place = hit.collider.GetComponent<Place>();
                    if (place != null)
                    {
                        //currentPlaceID = place.placeID;
                        place.OnClickPlace();

                    }
                }
                isTouch = false;
            }
        }
    }
}
