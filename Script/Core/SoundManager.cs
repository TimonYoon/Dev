using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    None,
    LandSelect, //땅 획득
    LandUpgrade, //땅 업그레이드
    BookUnfoldLight, // 뽑기 연출 2
    CardLight, // 뽑기 연출 1
    CardResult, // 뽑기 연출 3
    PackageItem, // 패키지 창 열때
    NomalButton, // 기본 버튼음,
    Cancel,
    ItemLevelUp,
    heroLevelUp,
    DropCoin,
    CardIntro,
    MapCloud,
    CardSelect,
    StrongContract,


}

public class SoundManager : MonoBehaviour {

    public static SoundManager Instance;

    public AudioClip clipCardLight;

    public AudioClip clipBookUnfoldLight;

    public AudioClip clipCardResult;

    public AudioClip clipLandSelect;

    public AudioClip clipLandUpgrade;

    public AudioClip clipPackageItem;

    public AudioClip clipNomalButton;

    public AudioClip clipCancel;

    public AudioClip clipFoodLevelUp;

    public AudioClip clipUILevelUp;

    public AudioClip clipUICoin;

    public AudioClip clipCardIntro;

    public AudioClip clipMapCloud;

    public AudioClip clipCardSelect;

    public AudioClip clipStrongContract;
    void Awake()
    {
        Instance = this;
    }
    
    public static void Play(SoundType soundType)
    {
        Instance.sound(soundType).Play();
        //switch (soundType)
        //{
        //    case SoundType.None:
        //        break;
        //    case SoundType.LandSelect:
        //        break;
        //    case SoundType.LandUpgrade:
        //        break;
        //    case SoundType.BookUnfoldLight:
        //        Instance.audioBookUnfoldLight.Play();
        //        break;
        //    case SoundType.CardLight:
        //        Instance.audioCardLight.Play();
        //        break;
        //    case SoundType.CardResult:
        //        Instance.audioCardResult.Play();
        //        break;
        //    case SoundType.PackageItem:
        //        break;
        //    case SoundType.NomalButton:
        //        break;
        //    case SoundType.Cancel:
        //        break;
        //    default:
        //        break;
        //}
    }
    public int poolingCount = 5;


    public Dictionary<SoundType, Queue<AudioSource>> stackDic = new Dictionary<SoundType, Queue<AudioSource>>();


    AudioSource sound(SoundType soundType)
    {
        AudioSource audioSource = null;

        if(stackDic.ContainsKey(soundType))
        {
            for (int i = 0; i < stackDic[soundType].Count; i++)
            {
                AudioSource a = stackDic[soundType].Dequeue();
                if (a == null)
                    break;

                stackDic[soundType].Enqueue(a);
                if (a.isPlaying == false)
                {
                    audioSource = a;                    
                    break;                    
                }
            }
            // 최대 갯수라면 가장 오래된 것 부터 사용
            if(stackDic[soundType].Count >= poolingCount)
            {
                AudioSource a = stackDic[soundType].Dequeue();
                stackDic[soundType].Enqueue(a);
                audioSource = a;
                //Debug.Log(soundType.ToString() + "재활용");
            }
        }
        else
        {
            Queue<AudioSource> queue = new Queue<AudioSource>();
            stackDic.Add(soundType, queue);
        }

        if(audioSource == null)
        {
            GameObject go = new GameObject(soundType.ToString());
            go.transform.SetParent(transform);
            audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = Clip(soundType);
            stackDic[soundType].Enqueue(audioSource);
        }
        

        return audioSource;
    }


    AudioClip Clip(SoundType soundType)
    {
        AudioClip clip = null;

        switch (soundType)
        {
            case SoundType.None:
                break;
            case SoundType.LandSelect:
                clip = clipLandSelect;
                break;
            case SoundType.LandUpgrade:
                clip = clipLandUpgrade;
                break;
            case SoundType.BookUnfoldLight:
                clip = clipBookUnfoldLight;
                break;
            case SoundType.CardLight:
                clip = clipCardLight;
                break;
            case SoundType.CardResult:
                clip = clipCardResult;
                break;
            case SoundType.PackageItem:
                clip = clipPackageItem;
                break;
            case SoundType.NomalButton:
                clip = clipNomalButton;
                break;
            case SoundType.Cancel:
                clip = clipCancel;
                break;
            case SoundType.ItemLevelUp:
                clip = clipFoodLevelUp;
                break;
            case SoundType.heroLevelUp:
                clip = clipUILevelUp;
                break;
            case SoundType.DropCoin:
                clip = clipUICoin;
                break;
            case SoundType.CardIntro:
                clip = clipCardIntro;
                break;
            case SoundType.MapCloud:
                clip = clipMapCloud;
                break;
            case SoundType.CardSelect:
                clip = clipCardSelect;
                break;
            case SoundType.StrongContract:
                clip = clipStrongContract;
                break;
            default:
                break;
        }
        
        return clip;
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //{
        //    Play(SoundType.BookUnfoldLight);
        //}
    }

}
