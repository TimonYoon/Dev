using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


/// <summary> 전투화면에 </summary>
public class BattleMoveCamera : MonoBehaviour
{
    private static readonly float PanSpeed = 12f;
    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float ZoomSpeedMouse = 30f;

    public Vector3 cameraStartVector { get; private set; }
    public Vector3 cameraEndVector { get; private set; }
    
    List<BattleHero> heroList
    {
        get
        {
            if (battleGroup == null || battleGroup.redTeamList == null)
                return null;
            else
                return battleGroup.redTeamList;// .battleHeroList.FindAll(x => x.mineTeam == BattleUnit.team.Red && !x.isDie && x.gameObject.activeSelf == true);
        }
    }
    List<BattleHero> monsterList
    {
        get
        {
            if (battleGroup == null || battleGroup.blueTeamList == null)
                return null;
            else
                return battleGroup.blueTeamList;// .battleHeroList.FindAll(x => x.mineTeam == BattleUnit.team.Blue && !x.isDie && x.gameObject.activeSelf == true);
        }
    }

    public BattleGroup battleGroup;
    public GameObject middle;
    public Transform pivot;
    float offsetVector;

    new public Camera camera;
    Camera uiCamera;
    Resolution lastSreenResolution;

    public List<GameObject> cachedObject = new List<GameObject>();

    //##################################################################################################
    void Awake()
    {
        camera = GetComponent<Camera>();
        uiCamera = Camera.main;

        lastSreenResolution = Screen.currentResolution;
    }

    UnityEngine.EventSystems.EventSystem eventSystem;

    private void Start()
    {
        if (AdController.Instance)
            AdController.Instance.onResizeCamera += OnResizeUICamera;

        if (UIBattle.Instance && UIBattle.Instance.battleScreen)
            UpdateCameraRect();

        CalculateClampRange();

        eventSystem = UnityEngine.EventSystems.EventSystem.current;

        Battle.onChangedBattleGroup += OnChangedBattleGroup;

        battleGroup.onChangedBattlePhase += OnChangedBattlePhase;

        transform.position = cameraStartVector;

        startPos = transform.position;


        lastFocusPos = startPos;
    }

    float fadeInStartTime;
    void OnChangedBattlePhase(BattleGroup b)
    {
        if(battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
        {
            //페이드인 시작 시간 저장해둠. 스크롤 및 자동 따라다니기 막기 위한 용도
            fadeInStartTime = Time.time;

            //시작 위치로 강제로 옮김
            transform.position = cameraStartVector;
        }
            
    }

    Vector3 startPos;

    void CalculateClampRange()
    {
        //List<MeshRenderer> sr = middle.GetComponentsInChildren<MeshRenderer>().ToList().OrderBy(x => x.transform.position.x).ToList();
        //MeshRenderer min = sr[0];
        //MeshRenderer max = sr[sr.Count - 1];
        //float left = min.bounds.min.x;
        //float right = max.bounds.max.x;

        float left = battleGroup.spwonPointXMin + 0f;
        float right = battleGroup.spwonPointXMax - 0f;

        float y = pivot.transform.position.y;

        offsetVector = camera.ScreenToWorldPoint(Vector3.right * Screen.width * 0.5f).x;
        Vector3 destPos = camera.WorldToScreenPoint(new Vector3(left, y, transform.position.z));
        cameraStartVector = camera.ScreenToWorldPoint(destPos + Vector3.right * Screen.width * 0.5f);
        destPos = camera.WorldToScreenPoint(new Vector3(right, y, transform.position.z));
        cameraEndVector = camera.ScreenToWorldPoint(destPos + Vector3.left * Screen.width * 0.5f);

    }
    
    void OnChangedBattleGroup(BattleGroup b)
    {
        if(battleGroup == b)
            Follow(true);
    }

    /// <summary> 배너 광고 제거로 화면 사이즈(정확히는 UICamera의 rect position)가 바뀌었을 때 발생 </summary>
    void OnResizeUICamera()
    {
        UpdateCameraRect();
    }

    void UpdateCameraRect()
    {
        float h = UIBattle.Instance.battleCanvas.GetComponent<RectTransform>().sizeDelta.y;
        
        float originalHeight = h / (1f - uiCamera.rect.position.y);
        float modifyByBanner = h / originalHeight;

        float posY = (h + UIBattle.Instance.battleScreen.anchoredPosition.y * modifyByBanner) / h;

        Vector2 pos = new Vector2(0f, posY);
        Vector2 size = new Vector2(1f, (UIBattle.Instance.battleScreen.rect.height / h) * (1f - uiCamera.rect.position.y));
        
        camera.rect = new Rect(pos, size);

        lastSreenResolution = Screen.currentResolution;
    }

    public float b { get; private set; }


    void Update()
    {
        CalculateClampRange();

        //if (Battle.currentBattleGroup == battleGroup)
        //    HandleMouse();

        b = transform.position.x - startPos.x;

        //b = Mathf.Lerp(cameraStartVector.x, cameraEndVector.x, transform.position.x);// Vector2.Distance(new Vector2(cameraStartVector.x, transform.position.y), transform.position);
        
#if UNITY_EDITOR
        UpdateCameraRect();
#endif

        if (Battle.currentBattleGroup == battleGroup)
        {
            //if (Input.touchSupported)
            //{
            //    HandleTouch();
            //}
            //else
            {
                HandleMouse();

            }
        }

        //if(string.IsNullOrEmpty(battleGroup.battleType) == false)
        //{
        //    HandleMouse();
        //}

    }
    
    Vector3 lastPanPosition;
    int panFingerId; // Touch mode only

    bool wasZoomingLastFrame; // Touch mode only
    Vector2[] lastZoomPositions; // Touch mode only

    void HandleTouch()
    {
        for(int i = 0; i < Input.touchCount; i++)
        {
            Debug.Log(Input.GetTouch(i).position);
        }
        

        switch (Input.touchCount)
        {


            case 1: // Panning

                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) break;

                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }
                break;

            case 2: // Zooming

                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) break;

                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    bool isDragging = false;    
    void HandleMouse()
    {
        if (battleGroup.battlePhase != BattleGroup.BattlePhase.Battle)
            return;

        //카메라뷰포트(전투화면)을 터치했는지 체크
        Vector3 touchPosInViewport = camera.ScreenToViewportPoint(Input.mousePosition);
        bool isTouchInViewport = touchPosInViewport.x >= 0f && touchPosInViewport.x <= 1f
                                && touchPosInViewport.y >= 0f && touchPosInViewport.y <= 1f;
        

        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0))
        {
            if (isTouchInViewport)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                lastPanPosition = Input.mousePosition;

                isDragging = true;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            PanCamera(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            lastTouchUpTime = Time.unscaledTime;
        }
        else
        {
                Follow();
        }

        if (isDragging)
        {
            PanCamera(Input.mousePosition);
        }
        
    }

    float lastTouchUpTime = float.MinValue;

    public float minFOV = 5f;
    public float maxFOV = 10f;

    /// <summary> 줌 인/아웃 처리 </summary>
    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        camera.orthographicSize = camera.orthographicSize - (offset * speed);
        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, minFOV, maxFOV);
    }

    /// <summary> 터치해서 드래그할 때 카메라 이동 </summary>
    void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = camera.ScreenToViewportPoint(lastPanPosition - newPanPosition);


        //Vector3 move = new Vector3(offset.x * PanSpeed, 0f, 0f);

        float moveAmount = offset.x * PanSpeed;

        float x = transform.position.x + moveAmount;
        x = Mathf.Clamp(x, cameraStartVector.x, cameraEndVector.x);   //스폰 포인트 바뀔 때 화면 튐

        //float x = Mathf.Lerp(transform.position.x, x2, 10f * Time.unscaledDeltaTime);

        transform.position = new Vector3(x, transform.position.y, transform.position.z);
        
        // Perform the movement
        //transform.Translate(move, Space.World);



        //float x = Mathf.Clamp(transform.position.x, cameraStartVector.x, cameraEndVector.x);
        //transform.position = new Vector3(x, transform.position.y, transform.position.z);

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    public Vector3 offset = Vector3.zero;

    public float smoothTime = 1f;

    public float maxSpeed = 15f;

    public float boost = 1f;
    
    float velocity;

    public void Follow(bool focusImmediatly = false)
    {
        if (battleGroup.battlePhase == BattleGroup.BattlePhase.Finish || battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut)
            return;

        if (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
        {
            transform.position = cameraStartVector;
            return;
        }

        BattleHero hero = GetHeroToFocus();
        focusHero = hero;
        if (!hero)
            return;

        float flipModify = hero.flipX ? -1f : 1f;

        Vector3 focusPoint = hero.transform.position + offset * flipModify;

        Vector3 destPos = Vector3.Lerp(transform.position, focusPoint, 20f * Time.deltaTime);

        if (focusImmediatly)
        {
            transform.position = new Vector3(focusPoint.x, transform.position.y, transform.position.z);
            return;
        }
            

        float distance = 0f;
        if (destPos.x > transform.position.x)
            distance = destPos.x - transform.position.x;
        else
            distance = transform.position.x - destPos.x;

        float x = transform.position.x;

        if (float.IsNaN(velocity))
            velocity = 0f;

        if (Time.time > fadeInStartTime + 3f && Time.unscaledTime > lastTouchUpTime + 1f)
            x = Mathf.SmoothDamp(transform.position.x, destPos.x, ref velocity, smoothTime, maxSpeed, distance * 0.1f * boost * Time.deltaTime);

        if (!float.IsNaN(x))
            transform.position = new Vector3(x, transform.position.y, transform.position.z);

        lastFocusPos = destPos;
    }

    Vector3 lastFocusPos;

    //확인용
    BattleHero focusHero;

    /// <summary> 영웅들의 중심지역 </summary>
    BattleHero GetHeroToFocus()
    {
        if (battleGroup.frontMostHero)
            return battleGroup.frontMostHero;
        else if (battleGroup.frontMostMonster)
            return battleGroup.frontMostMonster;
        else
            return null;
    }

}
