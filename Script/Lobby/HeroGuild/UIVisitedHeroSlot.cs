using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class UIVisitedHeroSlot : MonoBehaviour {

    [Header("영웅 정보 부분")]
    public GameObject[] gradeArray;
    public Image bg;
    public Color[] colorArray;

    public Image heroImage;
    public Text heroNameText;

    [Header("남은시간 & 고용가격")]

    [SerializeField]
    Text textRemainingTime;

    [SerializeField]
    Text textCostText;
    

    public string heroID { get; private set; }

    VisitedHeroData visitedHeroData;

    HeroData heroData;

    public delegate void OnClickEmploy(string heroID);
    public OnClickEmploy onClickEmploy;

    public double cost = 0;

    /// <summary> 슬롯 초기화 </summary>
    public void initHeroGuildSlot(VisitedHeroData _visitedHeroData)
    {
        heroID = _visitedHeroData.heroID;
        visitedHeroData = _visitedHeroData;
        

        heroData = new HeroData(HeroManager.heroBaseDataDic[heroID]);

        if (heroData != null)
            ShowGrade(heroData.heroGrade);

        heroNameText.text = heroData.heroName;
        AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);

        // 가격 공식 변경되면 서버에서 변경해야함...
        cost = 10000 * (Mathf.Pow(10, heroData.heroGrade));
        textCostText.text = cost.ToStringABC();

        textRemainingTime.text = RemainTime();
    }
    string Conversion(string data)
    {
        string returnData = string.Empty;
        char[] charArray = data.ToCharArray().Reverse().ToArray();

        int count = 3;
        for (int i = 0; i < charArray.Length; i++)
        {
            returnData = charArray[i] + returnData;
            if ((i + 1) % 3 == 0 && (i + 1) < charArray.Length)
                returnData = ',' + returnData;

        }
        return returnData;
    }

    /// <summary> 고용 버튼 눌림 </summary>
    public void OnClickEmployButton()
    {
        // 골드 체크 // 서버로 날림 해당 영웅 아이디와 골드량 체크해서 넣어주도록
        //int cost = cost;
        if (cost > MoneyManager.GetMoney(MoneyType.gold).value)
            UIPopupManager.ShowYesNoPopup("재화 부족!!", "골드가 부족합니다. 상점으로 가시겠습니까?", PopupResult);
        else
        {
            if (onClickEmploy != null)
                onClickEmploy(heroID);
        }
        
    }
    void PopupResult(string result)
    {
        if(result == "yes")
        {
            SceneLobby.Instance.ShowShop(ShopType.Gold);
        }
    }

    public void OnClickHeroSlot()
    {
        if(heroData != null)
            ShowHeroInfo(heroData);
    }

    private void Update()
    {
        if (visitedHeroData == null)
            return;

        textRemainingTime.text = RemainTime();
    }

    string RemainTime()
    {
        string result = "";


        if (visitedHeroData == null)
            return result;

        float totalSecond = visitedHeroData.remainingTime;

        if (totalSecond > 3600f)
        {
            int a = (int)totalSecond % 3600;

            int hour = ((int)totalSecond - a) / 3600;

            int b = (int)a % 60;

            int minute = ((int)a - b) / 60;

            result = hour + "h " + minute + "m";
        }
        else if (totalSecond > 60f)
        {
            int a = (int)totalSecond % 60;

            int minute = ((int)totalSecond - a) / 60;

            result = minute + "m " + a + "s";
        }
        else
        {
            result = totalSecond.ToString("N0") + "s";
        }


        return result;
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
    public void ShowHeroInfo(HeroData heroData)
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
            UIHeroInfo.Init(heroData,true,null,HeroInfoType.OnlyInfo);
    }



}
