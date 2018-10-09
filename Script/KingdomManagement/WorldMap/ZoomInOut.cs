using UnityEngine;
using System.Collections;

public class ZoomInOut : MonoBehaviour {

    public static ZoomInOut Instance;

    private static readonly float PanSpeed = 12f;
    public float ZoomSpeedTouch = 0.01f;
    private static readonly float ZoomSpeedMouse = 30f;

    Camera cam;

    Vector3 lastPanPosition;
    int panFingerId; // Touch mode only

    bool wasZoomingLastFrame; // Touch mode only
    Vector2[] lastZoomPositions; // Touch mode only

    
    public Collider2D boundObject = null;

    public float minFOV = 6f;
    public float maxFOV = 12f;

    public float springSpeed = 25f;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    void Update()
    { 

        if(SceneLobby.currentState == LobbyState.WorldMap)
        {
            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }
        }        
    }

    void HandleTouch()
    {
        return;

        switch (Input.touchCount)
        {
             

            case 1: // Panning

                //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) break;

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
                    //PanCamera(touch.position);
                }
                break;

            case 2: // Zooming

                //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) break;

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

    void HandleMouse()
    {

        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
            //Debug.Log("mouse down point : " + lastPanPosition.ToString());
        }
        else if (Input.GetMouseButton(0))
        {
            //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            //PanCamera(Input.mousePosition);
        }

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }


    void LateUpdate()
    {
        if (!boundObject)
            return;

        //스프링 효과
        //터치 안하고 있을 때에만 작동
        if (Input.GetMouseButton(0))
            return;
        

        //바운더리 삐져 나간 만큼 복귀해준다.
        Vector3 boundMin = cam.WorldToViewportPoint(boundObject.bounds.min);
        Vector3 boundMax = cam.WorldToViewportPoint(boundObject.bounds.max);

        Vector3 offset = Vector3.zero;

        if (boundMin.y > 0f)
            offset += Vector3.up * boundMin.y;
        if (boundMin.x > 0f)
            offset += Vector3.right * boundMin.x;
        if (boundMax.x < 1f)
            offset += Vector3.left * (1f - boundMax.x);
        if (boundMax.y < 1f)
            offset += Vector3.down * (1f - boundMax.y);


        if (offset != Vector3.zero)
        {
            //Debug.Log("off set : " + offset);
            transform.position = Vector3.Slerp(transform.position, transform.position + offset, springSpeed * Time.unscaledDeltaTime);
        }

    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        cam.orthographicSize = cam.orthographicSize - (offset*speed);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minFOV, maxFOV);
    }

}
