using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIDraw : MonoBehaviour
{
    public static UIDraw Instance;

    [Header("메시지 문구")]
    [SerializeField]
    Text messageText;


    [Header("스킵버튼")]
    [SerializeField]
    Button skipButton;

    
    [Header("뽑기 카메라")]
    [SerializeField]
    Camera drawCamera;

    [Header("단일 뽑기")]
    [SerializeField]
    GameObject drawSingularPanel;
    [SerializeField]
    UIHeroSlot heroSlot;
    [SerializeField]
    Animator anim;




    [Header("10개 뽑기")]
    [SerializeField]
    GameObject drawPluralPanel;

    [SerializeField]
    List<UIHeroSlot> heroSlotList;
    [SerializeField]
    List<Animator>  animList;

    //public AudioSource audioSource;
    private void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        drawCamera.fieldOfView = 175;
        // 씬 활성화
        Scene scene = SceneManager.GetSceneByName("Draw");
        SceneManager.SetActiveScene(scene);

        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0);
        skipButton.gameObject.SetActive(false);
        drawSingularPanel.SetActive(false);
        drawPluralPanel.SetActive(false);

        StartCoroutine(DrawCameraMoveing());
        //InitDraw();   
    }

    bool isInitialized = false;
    int count = 0;
    public void InitDraw()
    {
        
        if (DrawManager.Instance.isSingular)
        {
            drawSingularPanel.SetActive(true);
            if(DrawManager.isSpecialDraw)
            {
                anim.SetBool("special", true);
                DrawManager.isSpecialDraw = false;
            }
            
            anim.SetInteger("grade", HeroManager.heroDataDic[DrawManager.Instance.drowHeroIDList[0]].heroGrade);

               

            count = HeroManager.heroDataDic[DrawManager.Instance.drowHeroIDList[0]].heroGrade;

            heroSlot.SlotDataInit(DrawManager.Instance.drowHeroIDList[0], HeroSlotState.Default);
            heroSlot.InitImage();
            
        }
        else
        {
            drawPluralPanel.SetActive(true);

            for (int i = 0; i < animList.Count; i++)
            {
                animList[i].SetInteger("grade", HeroManager.heroDataDic[DrawManager.Instance.drowHeroIDList[i]].heroGrade);
            }

            for (int i = 0; i < heroSlotList.Count; i++)
            {
                heroSlotList[i].SlotDataInit(DrawManager.Instance.drowHeroIDList[i], HeroSlotState.Default);
                heroSlotList[i].InitImage();
            }
            
        }
        //skipButton.gameObject.SetActive(true);
        

        //heroSlot.SlotDataInit();
    }

    float speed = 100f;
    float a = 40f;
    float b = 50f;
    IEnumerator DrawCameraMoveing()
    {
        SoundManager.Play(SoundType.CardIntro);
        
        float start = Time.unscaledTime;
        while(drawCamera.fieldOfView > a)
        {

            float t = (Time.unscaledTime - start)/2.6f;
            drawCamera.fieldOfView = Mathf.Lerp(drawCamera.fieldOfView, a, t);
            drawCamera.transform.eulerAngles = Vector3.MoveTowards(drawCamera.transform.eulerAngles, new Vector3(drawCamera.transform.rotation.x, drawCamera.transform.rotation.y, drawCamera.transform.rotation.z + 1f), speed * Time.unscaledDeltaTime);
            yield return null;
        }
        InitDraw();
        
        start = Time.unscaledTime;
        while (drawCamera.fieldOfView < b)
        {
            float t = (Time.unscaledTime - start)/1.5f;
            drawCamera.fieldOfView = Mathf.Lerp(drawCamera.fieldOfView, b, t);
            yield return null;
        }
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
        messageText.gameObject.SetActive(true);
        isInitialized = true;
        while (true)
        {
            drawCamera.transform.eulerAngles = Vector3.MoveTowards(drawCamera.transform.eulerAngles ,new Vector3(drawCamera.transform.rotation.x, drawCamera.transform.rotation.y, drawCamera.transform.rotation.z +1f),Time.unscaledDeltaTime);// Mathf.Lerp(drawCamera.fieldOfView, b, t);
            yield return null;
        }

    }
    Coroutine showMessageTextCoroutine;
    bool isDraw = false;
    bool isSkip = false;
    bool isFinish = false;
    Coroutine drawCoroutine;
    private void Update()
    {
        if (isInitialized == false)
            return;
        
        if (!isDraw && Input.GetMouseButtonDown(0))
        {
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0);
            //Debug.Log("alpha value : " + messageText.color.a);
            if (DrawManager.Instance.isSingular)
            {                
                anim.SetTrigger("result");
            }
            else
            {                
                if (drawCoroutine != null)
                    return;

                drawCoroutine = StartCoroutine(DrawCoroutine());
            }             
        }
        else if(isDraw && Input.GetMouseButtonDown(0))
        {
            if (showMessageTextCoroutine != null && messageText.color.a > 0.7f)
                OnClickCloseButton();

            OnClickSkipButton();
        }

        if(!isDraw)
        {
            if (DrawManager.Instance.isSingular)
            {
                isDraw = anim.GetCurrentAnimatorStateInfo(0).IsTag("open");
            }
            else
            {
                for (int i = 0; i < animList.Count; i++)
                {
                    if(animList[i].GetCurrentAnimatorStateInfo(0).IsTag("open"))
                    {
                        isDraw = true;
                    }
                }                
            }
        }


        if(isDraw)
        {
            isFinish = false;
            if (DrawManager.Instance.isSingular)
            {
                isFinish = anim.GetCurrentAnimatorStateInfo(0).IsTag("finish");
            }
            else
            {
                isFinish = true;
                for (int i = 0; i < animList.Count; i++)
                {
                    if (animList[i].GetCurrentAnimatorStateInfo(0).IsTag("finish") == false)
                    {
                        isFinish = false;
                    }
                }                
            }

            if (isFinish)
            {
                //Debug.Log("끝남 연출");
                if (showMessageTextCoroutine == null)
                    showMessageTextCoroutine = StartCoroutine(ShowMessageText());
            }
        }
    }

    /// <summary> 스킵버튼 눌렀을 때 </summary>
    public void OnClickSkipButton()
    {
        isDraw = true;

        if (isSkip)
            return;

        isSkip = true;
        if(isFinish == false)
            SoundManager.Play(SoundType.CardResult);

        if (DrawManager.Instance.isSingular)
        {
            anim.SetTrigger("skip");
        }
        else
        {
            for (int i = 0; i < animList.Count; i++)
            {
                animList[i].SetTrigger("skip");
            }
            
        }
        if (showMessageTextCoroutine == null)
        {
            //Debug.Log("스킵 끝남 연출");
            showMessageTextCoroutine = StartCoroutine(ShowMessageText());
        }
            
    }

    IEnumerator DrawCoroutine()
    {
        int i = 0;
        float startTime = 0;
        float delayTime = 0.5f;
        float t = 1f;
        while (i < animList.Count)
        {            
            if (t >= 1)
            {
                startTime = Time.unscaledTime;
                animList[i].SetTrigger("result");
                i++;
            }
            t = (Time.unscaledTime - startTime) / delayTime;
            yield return null;
        }
    }

    IEnumerator ShowMessageText()
    {
        float start = Time.unscaledTime;
        yield return new WaitForSeconds(1.8f);

        float time = 1f;
        //Debug.Log("시작 : " + start + " / " + messageText.color.a);
        Vector4 goal = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
        
        while (messageText.color.a < 1f)
        {
            float t = (Time.unscaledTime - start) / time;            
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, Mathf.Lerp(0, 1, t));// = Vector4.Lerp(messageText.color, goal, t);
            yield return null;
        }        
    }
    public void OnClickCloseButton()
    {
        if (UIShop.Instance != null && UIShop.Instance.loadingPanel.activeSelf)
            UIShop.Instance.loadingPanel.SetActive(false);

        Scene scene = SceneManager.GetSceneByName("Lobby");
        SceneManager.SetActiveScene(scene);

        SceneManager.UnloadSceneAsync("Draw");
    }



}
