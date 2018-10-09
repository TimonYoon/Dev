using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WorldmapCamera : MonoBehaviour {

    public static WorldmapCamera Instance;

    bool isTouch = false;

    Vector2 curPos;

    Vector3 moveAmount;

    public float scrollSpeed = 5f;
    public float rotateSpeed = 5f;
    public float rotationResetSpeed = 3f;

    public Camera worldMapCamera;

    public Collider2D worldBounds;

    void Awake()
    {
        Instance = this;

        worldMapCamera.enabled = false;
    }

    IEnumerator Start()
    {           
        if (!worldMapCamera)
            yield break;
               
        yield return null;
    }

    Vector2 upPoint = Vector2.zero;
    Vector2 downPoint = Vector2.zero;

    Vector3 boundMin;
    Vector3 boundMax;
    
    Vector3 offset;

    void LateUpdate()
    {
        if (SceneLobby.currentState == LobbyState.WorldMap)
        {
            if (Time.time < lastFocusTime + 0.3f)
                return;

            boundMin = worldMapCamera.WorldToViewportPoint(worldBounds.bounds.min);
            boundMax = worldMapCamera.WorldToViewportPoint(worldBounds.bounds.max);

            if (Input.touchSupported)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }            
        }
    }

    //bool wasZoomingLastFrame; // Touch mode only
    //Vector2[] lastZoomPositions; // Touch mode only
    int panFingerId; // Touch mode only

    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 0:
                //wasZoomingLastFrame = false;
                Inertia();
                break;

            case 1:
                //wasZoomingLastFrame = false;
                Touch touch = Input.GetTouch(0);
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    offset = Vector3.zero;
                    isTouch = false;
                    return;
                } 

                if (touch.phase == TouchPhase.Began)
                {
                    downPoint = touch.position;
                    panFingerId = touch.fingerId;
                    isTouch = true;
                }
                else if(touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    MoveCamera(touch.position);
                }
                else if(touch.fingerId == panFingerId && touch.phase == TouchPhase.Ended)
                {
                   
                    if(isTouch)
                    {
                        //UIPopupManager.ShowInstantPopup("손가락 땜");
                        //Handheld.Vibrate();
                        upPoint = touch.position;

                        float dis = Vector3.Distance(upPoint, transform.position);
                        if (dis > 1)
                        {
                            offset = (downPoint - upPoint) * 0.2f;
                            offset += transform.position;
                        }
                        else
                        {
                            //UIPopupManager.ShowInstantPopup("꾹눌림");
                            offset = Vector3.zero;
                        }
                        isTouch = false;
                    }
                    
                }
                else
                {
                    //UIPopupManager.ShowInstantPopup("뭐지?");
                    //downPoint = Vector2.zero;
                    //upPoint = Vector2.zero;
                    //offset = Vector3.zero;
                    //curPos = Vector2.zero;
                }
                break;

            case 2:
                //Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                //if (!wasZoomingLastFrame)
                //{
                //    lastZoomPositions = newPositions;
                //    wasZoomingLastFrame = true;
                //}
                //else
                //{
                //    // Zoom based on the distance between the new positions compared to the 
                //    // distance between the previous positions.
                //    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                //    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                //    float offset = newDistance - oldDistance;

                //    ZoomCamera(offset, zoomSpeedTouch);

                //    lastZoomPositions = newPositions;
                //}
                break;

            default:
                //wasZoomingLastFrame = false;
                break;
        }
    }

  
    void HandleMouse()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            offset = Vector3.zero;
            isTouch = false;
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            //UIPopupManager.ShowInstantPopup("마우스????");
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                offset = Vector3.zero;
                isTouch = false;
                return;
            }
            

            isTouch = true;
            downPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && isTouch)
        {
            //UIPopupManager.ShowInstantPopup("마우스 땜????");
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                offset = Vector3.zero;
                isTouch = false;
                return;
            } 

            upPoint = Input.mousePosition;
            float dis = Vector3.Distance(upPoint, transform.position);
            if (dis > 1)
            {
                offset = (downPoint - upPoint) * 0.2f;
                offset += transform.position;
            }
            else
            {
                offset = Vector3.zero;
            }
        }

        if (Input.GetMouseButton(0))
        {
            //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            MoveCamera(Input.mousePosition);
        }
        else if (Input.touchCount == 0)
        {
            Inertia();
        }
        else
        {
            downPoint = Vector2.zero;
            upPoint = Vector2.zero;
            offset = Vector3.zero;
            curPos = Vector2.zero;
        }
    }
    bool isStop = false;
    void Inertia()
    {
        if (isStop)
            return;

        if (boundMin.y > 0f)
            offset.y = 0;// = new Vector3(0, 0, 0);// Vector3.zero;
        if (boundMin.x > 0f)
            offset.x = 0;// = new Vector3(0, 0, 0);
        if (boundMax.x < 1f)
            offset.x = 0;// = new Vector3(0, 0, 0);
        if (boundMax.y < 1f)
            offset.y = 0;//= new Vector3(0, 0, 0);

        if (offset != Vector3.zero)
        {
            float dis = Vector3.Distance(transform.position, offset);
            if (dis > 1)
                transform.position = Vector3.Lerp(transform.position, offset, scrollSpeed * Time.unscaledDeltaTime);
        }
    }

    void MoveCamera(Vector2 touchPos)
    {
        if (touchPos.x > 0 && touchPos.x < Screen.width && touchPos.y > 0 && touchPos.y < Screen.height)
        {
            curPos = touchPos;

            if (downPoint != Vector2.zero)
                moveAmount = downPoint - curPos;
            else
                moveAmount = Vector2.zero;

            if (boundMax.x < 1.05f && moveAmount.x > 0f)
                moveAmount.x = 0f;
            else if (boundMin.x > -0.05f && moveAmount.x < 0f)
                moveAmount.x = 0f;

            if (boundMax.y < 1.05f && moveAmount.y > 0f)
                moveAmount.y = 0f;
            else if (boundMin.y > -0.05f && moveAmount.y < 0f)
                moveAmount.y = 0f;


            transform.position = Vector3.Lerp(transform.position, transform.position + moveAmount, scrollSpeed * Time.unscaledDeltaTime);

            downPoint = curPos;
        }
        else
        {
            moveAmount = Vector2.zero;
        }
    }
    //void ZoomCamera(float offset, float speed)
    //{
    //    if (offset == 0)
    //    {
    //        return;
    //    }

    //    camera.orthographicSize = camera.orthographicSize - (offset * speed);
    //    camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, minFOV, maxFOV);
    //}
    //public float zoomSpeedTouch = 0.01f;
    //public float minFOV = 6f;
    //public float maxFOV = 12f;

    float lastFocusTime = 0f;

    void OnApplicationFocus(bool isFocus)
    {
        lastFocusTime = Time.time;        
    }


    //void BeginDrag()
    //{
    //    Debug.Log("BeginDrag");
    //}

}
