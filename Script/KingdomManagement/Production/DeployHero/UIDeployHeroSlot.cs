using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
using System.Linq;


public class UIDeployHeroSlot : MonoBehaviour {

    [Header("영웅 정보 부분")]
    public GameObject[] gradeArray;
    public Image bg;
    public Color[] colorArray;

    public Image heroImage;
    public Text heroNameText;

    [SerializeField]
    Text textEnhance;

    [SerializeField]
    Text textRebirth;
    //public string heroID { get; private set; }
     
    HeroData heroData;

 
    /// <summary> 슬롯 초기화 </summary>
    public void initDeployHeroSlot(string id)
    {
       
        heroData = HeroManager.heroDataDic[id];
       
        if (heroData != null)
            ShowGrade(heroData.heroGrade);

        heroNameText.text = heroData.heroName;
        AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);
        textEnhance.gameObject.SetActive(heroData.enhance > 0);
        textEnhance.text = "+" + heroData.enhance;

        textRebirth.gameObject.SetActive(heroData.rebirth > 0);
        textRebirth.text = heroData.rebirth.ToString();

    }
    string Conversion(string data)
    {
        string returnData = string.Empty;
        char[] charArray = data.ToCharArray().Reverse().ToArray();

        
        for (int i = 0; i < charArray.Length; i++)
        {
            returnData = charArray[i] + returnData;
            if ((i + 1) % 3 == 0 && (i + 1) < charArray.Length)
                returnData = ',' + returnData;

        }
        return returnData;
    }

  

    public void OnClickHeroSlot()
    {
        if (heroData != null)
            ShowHeroInfo(heroData);
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

        if (grade < 1 || grade > colorArray.Length)
            bg.color = Color.gray;

        bg.color = colorArray[grade - 1];
    }

    /// <summary> 캐릭터 정보 상세보기 </summary>
    void ShowHeroInfo(HeroData heroData)
    {
        if (coroutineShowHeroInfo != null)
            StopCoroutine(coroutineShowHeroInfo);

        coroutineShowHeroInfo = StartCoroutine(ShowHeroInfoA(heroData));
    }

    Coroutine coroutineShowHeroInfo;

    IEnumerator ShowHeroInfoA(HeroData heroData)
    {
        //씬 불러옴
        Scene scene = SceneManager.GetSceneByName("HeroInfo");
        if (!scene.isLoaded)
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/heroinfo", "HeroInfo", true));

            scene = SceneManager.GetSceneByName("HeroInfo");

            while (!scene.isLoaded)
                yield return null;
        }

        if (UIHeroInfo.Instance)
            UIHeroInfo.Init(heroData, true, null, HeroInfoType.OnlyInfo);
    }
}
