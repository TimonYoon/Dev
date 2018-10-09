using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour {

    public AudioSource audioSourceNormal;
    public AudioSource audioSourceDouble;
    public AudioSource audioSourceTriple;

    enum SpeedType
    {
        NotDefined = -1,
        Normal = 0,
        Double = 1,
        Triple = 2
    }

    SpeedType _speedType = SpeedType.NotDefined;
    SpeedType speedType
    {
        get { return _speedType; }
        set
        {
            bool isChange = _speedType != value;
            
            _speedType = value;

            if (isChange)
                ChangeBGM(value);
        }
    }

    Coroutine coroutineFadeOut = null;
    IEnumerator FadeOut(AudioSource audio)
    {
        while(audio.volume > 0)
        {
            audio.volume -= Time.deltaTime;
            yield return null;
        }
    }

    void ChangeBGM(SpeedType destSpeed)
    {
        if (coroutineChangeBGM != null)
            StopCoroutine(coroutineChangeBGM);

        coroutineChangeBGM = StartCoroutine(ChangeBGMA(destSpeed));

    }

    public float CrossFadeSpeed = 1f;

    Coroutine coroutineChangeBGM = null;
    IEnumerator ChangeBGMA(SpeedType destSpeed)
    {
        //bool isFinishFadeOut = false;
        bool isFinishFadeIn = false;

        //대상 속도 음악 재생
        audioSourceList[(int)destSpeed].Play();

        //기존 음악, 새로 재생한 음악 크로스페이드
        while (!isFinishFadeIn)
        {
            for (int i = 0; i < audioSourceList.Count; i++)
            {
                AudioSource audio = audioSourceList[i];

                //목표 속도 음악은 볼륨 업
                if (i == (int)destSpeed)
                {
                    audio.volume += CrossFadeSpeed * Time.deltaTime;
                    if (audio.volume > 1f)
                    {
                        isFinishFadeIn = true;
                        audio.volume = 1f;
                    }
                        
                }
                //목표 속도 음악 아닌 애들은 볼륨 다운
                else if (i != (int)destSpeed)
                {
                    audio.volume -= CrossFadeSpeed * Time.deltaTime;
                }
            }

            yield return null;
        }

        for (int i = 0; i < audioSourceList.Count; i++)
        {
            if (i != (int)destSpeed)
            {
                audioSourceList[i].volume = 0f;
                audioSourceList[i].Stop();
            }
        }

        coroutineChangeBGM = null;
    }

    List<AudioSource> audioSourceList = new List<AudioSource>();

    void Awake()
    {
        audioSourceList.Add(audioSourceNormal);
        audioSourceList.Add(audioSourceDouble);
        audioSourceList.Add(audioSourceTriple);
    }

    void Start()
    {
        //audioSourceList[(int)speedType].volume = 1f;
        //audioSourceList[(int)speedType].Play();
    }
        
    void Update ()
    {
        if (Time.timeScale < 2f)
            speedType = SpeedType.Normal;
        else if (Time.timeScale >= 2f && Time.timeScale < 3f)
            speedType = SpeedType.Double;
        else
            speedType = SpeedType.Triple;


    }
}
