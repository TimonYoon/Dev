using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 배너광고를 위해 카메라 수정을 위한 클래스 </summary>
public class CameraController : MonoBehaviour {
    
    private new Camera camera;

    private void OnEnable()
    {
        camera = this.gameObject.GetComponent<Camera>();

        if (AdController.Instance)
            AdController.Instance.onResizeCamera += ResizeUiCamera;
        
    }

    void Start ()
    {     
       
#if !UNITY_EDITOR

        float screenDpi = Screen.dpi;

        Debug.Log("SCREEN HEIGHT : " + Screen.height);
        float pixel = 0;
        if (Screen.height <= 400 * (Screen.dpi / 160))
        {
            pixel = 32 * (Screen.dpi / 160);
        }
        else if (Screen.height <= 720 * (Screen.dpi / 160))
        { 
            pixel = 50 * (Screen.dpi / 160);
        }
        else
        {
            pixel = 90 * (Screen.dpi / 160);
        }
       
        float result = pixel / Screen.height;

        camera.rect = new Rect(0, result, 1, 1);
        
#endif

    }

    //배너광고 삭제시 카메라 사이즈 조정을 위한 메소드
    public void ResizeUiCamera()
    {
        Debug.Log("RESIZE CAMERA");
        camera.rect = new Rect(0, 0, 1, 1);
    }
	
}
