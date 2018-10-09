using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using UnityEngine.SceneManagement;

public enum DicionaryState
{
    Default,
}


public class UIDictionarySlot : MonoBehaviour
{
    
    DicionaryState _state = DicionaryState.Default;
    public DicionaryState state;

    /// <summary> 히어로 데이타 참조용. (처음 초기화할 때 저장) </summary>
    public string id;

    public HeroData heroData
    {
        get
        {
            if (DictionaryManager.heroDictionaryDataDic.ContainsKey(id))
                return DictionaryManager.heroDictionaryDataDic[id].heroData;
            else
                return null;
        }
    }

    public string heroDataID;

    [Header("등급별 표시")]
    public GameObject[] gradeArray;
    public Image bg;
    public Color[] colorArray;
    public bool showGradeColor = true;

    [Header("속성별 표시할 오브젝트들")]
    public GameObject objectElementalFire;
    public GameObject objectElementalWater;
    public GameObject objectElementalEarth;
    public GameObject objectElementalLight;
    public GameObject objectElementalDark;
    public GameObject objectElementalNotDefined;

    [Header("기본 정보")]
    public Image heroImage;
    public Texture texture { get; private set; }
    public Text heroNameText;
    public Button heroSlotButton = null;

    public int dictionaryLevel;
    public int achievementLevel = 0;

    public Image[] dictionaryLevelStar;

    [Header("보상획득가능 표시")]
    public Image canRewardImage;
    

    private void OnDestroy()
    {
        heroDicData.onChangedValue -= OnChangedHeroParam; ;
    }

    void OnChangedHeroParam(PropertyInfo property)
    {
        if (property.Name == "dictionaryLevel")
        {
            dictionaryLevel = heroDicData.dictionaryLevel;
            if (dictionaryLevel == 1)
            {
                canRewardImage.gameObject.SetActive(heroDicData.dictionaryLevel > heroDicData.rewardStep);
                InitImage();
                ShowStar();
            }
            else
            {
                ShowStar();
            }
        }

        if (property.Name == "rewardStep")
        {
            achievementLevel = heroDicData.rewardStep;
        }

    }

    Color originalBGColor;
    void Start()
    {
        originalBGColor = bg.color;
        
    }

    public void SlotDataInit(string _id, DicionaryState _state)
    {

        texture = null;
        id = _id;
        state = _state;

        dictionaryLevel = heroDicData.dictionaryLevel;

        heroDicData.onChangedValue += OnChangedHeroParam;
        InitUI();

    }

    DictionaryManager.HeroDictionaryData heroDicData
    {
        get
        {
            if (DictionaryManager.heroDictionaryDataDic.ContainsKey(id))
                return DictionaryManager.heroDictionaryDataDic[id];
            else
                return null;
        }
    }
    
    void InitUI()
    {
        if (heroData == null)
        {
            Debug.LogError("not defined hero data");
            return;
        }
        heroDataID = heroData.heroID;
        ShowGrade(heroData.heroGrade);
        heroNameText.text = heroData.heroName;

        canRewardImage.gameObject.SetActive(heroDicData.dictionaryLevel > heroDicData.rewardStep);

        ShowStar();

        InitImage();
    }
    //도감레벨만큼 별개수 보여줌
    void ShowStar()
    {
        switch (dictionaryLevel)
        {
            case 0:
                break;
            case 1:
                dictionaryLevelStar[0].color = new Color(255, 255, 255);
                break;
            case 2:
                dictionaryLevelStar[0].color = new Color(255, 255, 255);
                dictionaryLevelStar[1].color = new Color(255, 255, 255);
                break;
            case 3:
                dictionaryLevelStar[0].color = new Color(255, 255, 255);
                dictionaryLevelStar[1].color = new Color(255, 255, 255);
                dictionaryLevelStar[2].color = new Color(255, 255, 255);
                break;
        }
    }
    // 영웅 slot에 등급 만큼 별 이미지 표시
    void ShowGrade(int grade)
    {
        foreach (var i in gradeArray)
        {
            i.SetActive(false);
        }
        if (grade < 5 && grade > 0)
            gradeArray[grade - 1].SetActive(true);
        else
            gradeArray[0].SetActive(true);

        if (showGradeColor)
        {
            if (grade < 1 || grade > colorArray.Length)
                bg.color = Color.gray;

            bg.color = colorArray[grade - 1];
        }
        else
        {
            //Color 안 보이기 하면 그냥 시작 색으로
            bg.color = originalBGColor;
        }


    }
    

    public void InitImage()
    {
        AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);

        Material grayScaleMeterial = new Material(heroImage.material);

        if (dictionaryLevel == 0)
        {
            grayScaleMeterial.SetFloat("_GrayScale", 0f);
            heroImage.material = grayScaleMeterial;
        }
        else
        {
            grayScaleMeterial.SetFloat("_GrayScale", 1f);
            heroImage.material = grayScaleMeterial;
            //heroImage.color = new Color(255, 255, 255);
        }

    }

    void OnFinisthInitImage(string result)
    {
#if UNITY_EDITOR
        if (result.Contains("Error"))
            Debug.Log(result);
#endif
    }

    /// <summary> 영웅 슬롯 버튼 누름 </summary>
    public void OnClickHeroSlotButton()
    {



        switch (state)
        {
            case DicionaryState.Default:
                OnClickShowHeroInfo();
                break;
            default:
                break;
        }
    }

    /// <summary> 캐릭터 정보 상세보기 </summary>
    void OnClickShowHeroInfo()
    {
        if (coroutineShowHeroInfo != null)
            StopCoroutine(coroutineShowHeroInfo);

        coroutineShowHeroInfo = StartCoroutine(ShowHeroInfo());
    }

    Coroutine coroutineShowHeroInfo;

    IEnumerator ShowHeroInfo()
    {
        //씬 불러옴
        Scene scene = SceneManager.GetSceneByName("HeroInfo");
        if (!scene.isLoaded)
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/heroinfo", "HeroInfo", true));
            //SceneLobby.Instance.SceneChange(state);

            scene = SceneManager.GetSceneByName("HeroInfo");

            while (!scene.isLoaded)
                yield return null;
        }

        if (UIHeroInfo.Instance)
            UIHeroInfo.Init(heroData, true, id);
    }
}