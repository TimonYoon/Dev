using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIFadeController : MonoBehaviour {

    static UIFadeController Instance;

    public Canvas canvas;
    public Image image;

    void Awake()
    {
        Instance = this;

        canvas.gameObject.SetActive(false);
    }

    IEnumerator Start()
    {
        //fadeCamera = gameObject.GetComponent<Camera>();
        //canvas = GetComponentInChildren<Canvas>();
        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;


        while (!UIBattle.Instance)
            yield return null;

        Battle.onRemoveBattle += OnRemoveBattle;

        UIBattle.Instance.onClickReStartButton += OnClickReStartButton;
    }

    void OnChangedMenu(LobbyState state)
    {
        ////Debug.Log("로비 상태 :" + state.ToString());
        if (state == LobbyState.Battle)
        {
            //fadeCamera.enabled = true;
            
        }
        else
        {
            //fadeCamera.enabled = false;
            //canvas.enabled = false;
        }

        //canvas.enabled = false;

    }
    
    /// <summary> 회군 후 완전 종료. 재시작 x </summary>
    void OnRemoveBattle()
    {
        FadeIn();
    }

    /// <summary> 회군 후 재시작 </summary>
    void OnClickReStartButton()
    {
        FadeIn();
    }
    
    static public bool isFinishFadeOut
    {
        get
        {
            if (!Instance || !Instance.canvas.gameObject.activeSelf)
                return false;

            return Instance.image.color == Color.black;
        }
    }

    /// <summary> 화면 점점 어두워짐 </summary>
    static public void FadeOut(float time = 1f)
    {
        if (!Instance)
            return;

        if (coroutineFadeIn != null)
        {
            Instance.StopCoroutine(coroutineFadeIn);
            coroutineFadeIn = null;
        }

        if (coroutineFadeOut != null)
        {
            Instance.StopCoroutine(coroutineFadeOut);
            coroutineFadeOut = null;
        }
                
        coroutineFadeOut = Instance.StartCoroutine(Instance.FadeOutA(time));
    }

    /// <summary> 화면 점점 밝아짐 </summary>
    static public void FadeIn(float time = 1f)
    {
        if (!Instance)
            return;

        if (coroutineFadeIn != null)
        {
            Instance.StopCoroutine(coroutineFadeIn);
            coroutineFadeIn = null;
        }

        if (coroutineFadeOut != null)
        {
            Instance.StopCoroutine(coroutineFadeOut);
            coroutineFadeOut = null;
        }            

        coroutineFadeIn = Instance.StartCoroutine(Instance.FadeInA(time));
    }

    static Coroutine coroutineFadeOut = null;
    static Coroutine coroutineFadeIn = null;

    IEnumerator FadeInA(float fadeTime = 1f)
    {
        canvas.gameObject.SetActive(true);

        //image.color = Color.black;
        
        float startTime = Time.time;
        while (true)
        {
            float time = Time.time - startTime;
            float t = time / fadeTime;
            image.color = Vector4.Lerp(image.color, Color.clear, t);
            if (image.color == Color.clear)
            {
                break;
            }
            yield return null;
        }

        canvas.gameObject.SetActive(false);

        coroutineFadeIn = null;
    }

    IEnumerator FadeOutA(float fadeTime = 1f)
    {
        canvas.gameObject.SetActive(true);
        //fadePanel.color = fadePanel.color * new Color(1, 1, 1, 0);
        
        float startTime = Time.time;
        while (image.color.a <= Color.black.a)
        {
            float time = Time.time - startTime;
            float t = time / fadeTime;
            image.color = Vector4.Lerp(image.color, Color.black, t);
            if(image.color == Color.black)
                break;

            yield return null;
        }

        coroutineFadeOut = null;
    }


    

}
