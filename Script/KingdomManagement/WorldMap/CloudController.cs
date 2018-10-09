using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour {

    public static CloudController Instance;

    public float minStartDelay = 0f; 
    public float maxStartDelay = 0.1f;

    public float movingPointX = 50;
    public float speed = 10f;
    
    public RectTransform centerPoint;

    public Camera mapCamera;

    public GameObject cloudPanel;

    void Awake()
    {
        Instance = this;
        cloudPanel.SetActive(true);
    }

    bool isReset = false;

    //private void Start()
    //{
    //    //coroutine = StartCoroutine(Show());
    //}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            isReset = !isReset;
        }

        if(!isReset)
        {
            if (coroutine == null)
            {
                coroutine = StartCoroutine(Show());
            }
            
        }
        else
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                mapCamera.orthographicSize = startCameraSize;
            }
        }
       
    }
    Coroutine coroutine;
    public float startCameraSize = 14f;
    public float zoomTime = 1f;
    IEnumerator Show()
    {
        SoundManager.Play(SoundType.MapCloud);
        float startTime = Time.unscaledTime;
        while(true)
        {
            float t = (Time.unscaledTime - startTime)/ zoomTime;
            mapCamera.orthographicSize = Mathf.Lerp(startCameraSize, 12f, t);
            yield return null;
        }
    }
}
