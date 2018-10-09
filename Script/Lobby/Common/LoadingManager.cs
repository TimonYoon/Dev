using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 로딩 표현을 하는 클래스 </summary>
public class LoadingManager : MonoBehaviour {

    static LoadingManager Instance;

    public void Awake()
    {
        Instance = this;

        Instance.imageBlack.color = new Color(0f, 0f, 0f, 1f);
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);        
    }
    public GameObject backPanel;
    public GameObject loadingImage;

    public Image imageBlack;

    public Animator anim;

    public static void ShowFullSceneLoading()
    {
        Instance.backPanel.SetActive(true);
        Instance.anim.SetTrigger("Start");
    }

    public static void Show()
    {
        Instance.loadingImage.SetActive(true);
        Instance.backPanel.SetActive(true);
    }

    public static void Close()
    {
        Instance.anim.SetTrigger("End");
        Instance.loadingImage.SetActive(false);
        Instance.backPanel.SetActive(false);
    }

    public float crossFadeTime = 1f;

    static public IEnumerator FadeOutScreen()
    {
        Instance.imageBlack.gameObject.SetActive(true);
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < Instance.crossFadeTime)
        {
            float e = Time.unscaledTime - startTime;
            float a = e * 1f / Instance.crossFadeTime;
            Instance.imageBlack.color = new Color(0f, 0f, 0f, a);

            yield return null;
        }

        Instance.imageBlack.color = new Color(0f, 0f, 0f, 1f);
    }

    static public IEnumerator FadeInScreen()
    {
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < Instance.crossFadeTime)
        {
            float e = Time.unscaledTime - startTime;
            float a = 1 - e * 1f / Instance.crossFadeTime;
            Instance.imageBlack.color = new Color(0f, 0f, 0f, a);

            yield return null;
        }

        Instance.imageBlack.color = new Color(0f, 0f, 0f, 0f);
        Instance.imageBlack.gameObject.SetActive(false);
    }


}
