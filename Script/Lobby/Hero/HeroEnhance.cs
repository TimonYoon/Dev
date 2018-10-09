using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;


/// <summary> 영웅 강화  </summary>
public class HeroEnhance : MonoBehaviour
{
    public static HeroEnhance Instance;

    HeroData heroData { get { return UIHeroInfo.Instance.heroData; } }

    /// <summary> 강화석 타입 </summary>
    string enhancePointType = "";

    /// <summary> 환생시 필요 재화 타입 </summary>
    ObscuredString rebirthType = "ruby";

    public delegate void OnChangedEnhanceValue();
    /// <summary> 소지 강화석 수량이 변경될 때 </summary>
    public OnChangedEnhanceValue onChnagedEnhanceValue;

    double _enhanceValue = 0;
    /// <summary> 소지하고 있는 강화석 수량 (서버작업을 close할때 진행하기 위해 따로 빼둠) </summary>
    public double enhanceValue
    {
        get
        {
            return _enhanceValue;
        }
        private set
        {
            _enhanceValue = value;

            if (onChnagedEnhanceValue != null)
                onChnagedEnhanceValue();
        }
    }

    /// <summary> 테스트 용 강화석 사용량 체크해서 서버에 알려주는 역할 </summary>
    double testUsingEnhancePoint;
    /// <summary> 테스트 용 환생 재화 루비 사용량 체크해서 서버에 알려주는 역할 </summary>
    double testUsingRebirthPoint;

    /// <summary> 환생 가능한 상태인가? (Read only) </summary>
    public bool canRebirth { get { return heroData != null && heroData.enhance >= 100; } }

    /// <summary> 강화 가능한 상태인가? (Read only) </summary>
    public bool canEnhance { get { return needEnhancePoint <= enhanceValue; } }

    /// <summary> +10 강화 가능한 상태인가? (Read only) </summary>
    public bool canTenEnhance {
        get
        {
            if (heroData == null)
                return false;
            if (100 - heroData.enhance < 10)
                return false;

            return needTenEnhancePoint <= enhanceValue; } }

    /// <summary> max 강화 가능한 상태인가? (Read only) </summary>
    public bool canMaxEnhance { get { return needMaxEnhancePoint <= enhanceValue; } }

    /// <summary> 강화에 필요한 강화석 수량(Read only) </summary>
    public double needEnhancePoint
    {
        get
        {
            if (heroData == null)
                return 0;

            // 필독!! ------ 몇강할껀지에 따라 비용이 달라짐-------
            int enhanceAmount = 1;  //아래 식은 1강씩 하는 경우의 예시임

            int rebirth = heroData.rebirth * 100; //1환생당 100강한걸로 침
                        
            //정수값 이하에서는 버림 처리..2147483647
            double curCost = 40 * System.Math.Pow(1.05, rebirth + heroData.enhance - 1);
            curCost = curCost < int.MaxValue ? System.Math.Truncate(curCost) : curCost;

            double destCost = 40 * System.Math.Pow(1.05, heroData.rebirth * 100 + heroData.enhance + enhanceAmount - 1);
            destCost = destCost < int.MaxValue ? System.Math.Truncate(destCost) : destCost;

            return destCost - curCost;

            double i = System.Math.Floor((heroData.enhance + 1) * (heroData.rebirth + 0.5f) * ((heroData.heroGrade + 1) * 10));
            return i;
        }
    }

    /// <summary> 10 강 비용 </summary>
    public double needTenEnhancePoint
    {
        get
        {
            if (heroData == null)
                return 0;

           
            int enhanceAmount = 10; 

            int rebirth = heroData.rebirth * 100;

            //정수값 이하에서는 버림 처리..2147483647
            double curCost = 40 * System.Math.Pow(1.05, rebirth + heroData.enhance - 1);
            curCost = curCost < int.MaxValue ? System.Math.Truncate(curCost) : curCost;

            double destCost = 40 * System.Math.Pow(1.05, heroData.rebirth * 100 + heroData.enhance + enhanceAmount - 1);
            destCost = destCost < int.MaxValue ? System.Math.Truncate(destCost) : destCost;

            return destCost - curCost;

            //double i = System.Math.Floor((heroData.enhance + 1) * (heroData.rebirth + 0.5f) * ((heroData.heroGrade + 1) * 10));
            //return i;
        }
    }

    /// <summary> 환생전 Max까지의 강화 비용 </summary>
    public double needMaxEnhancePoint
    {
        get
        {
            if (heroData == null)
                return 0;


            int enhanceAmount = 100 - heroData.enhance;

            int rebirth = heroData.rebirth * 100;

            //정수값 이하에서는 버림 처리..2147483647
            double curCost = 40 * System.Math.Pow(1.05, rebirth + heroData.enhance - 1);
            curCost = curCost < int.MaxValue ? System.Math.Truncate(curCost) : curCost;

            double destCost = 40 * System.Math.Pow(1.05, heroData.rebirth * 100 + heroData.enhance + enhanceAmount - 1);
            destCost = destCost < int.MaxValue ? System.Math.Truncate(destCost) : destCost;

            return destCost - curCost;

            //double i = System.Math.Floor((heroData.enhance + 1) * (heroData.rebirth + 0.5f) * ((heroData.heroGrade + 1) * 10));
            //return i;
        }
    }

    /// <summary> 환생시 필요 포인트 (Read only)</summary>
    public double needRebirthPoint
    {
        get
        {
            if (heroData == null)
                return 0;
            double i = System.Math.Floor(((heroData.rebirth + 0.5f) * ((heroData.heroGrade + 1) * 100)));
            return i;
        }
    }

    [SerializeField]
    public IHeroInfoUpdateUI heroInfoUpdateUI;

    ObscuredInt dailyMissionEnhance;
    ObscuredInt userQuestRebirth;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        while (UIHeroInfo.Instance == null)
            yield return null;

        UIHeroInfo.Instance.onHide += OnEnhanceServerConnect;

    }
    
    public void InitEnhance()
    {
        if (heroData.heroType == HeroData.HeroType.Battle)
        {
            switch (heroData.baseData.elementalType)
            {
                case ElementalType.NotDefined:
                    enhancePointType = "";
                    enhanceValue = 0;
                    break;
                case ElementalType.Fire:
                    enhancePointType = "enhancePointA";
                    break;
                case ElementalType.Water:
                    enhancePointType = "enhancePointB";
                    break;
                case ElementalType.Earth:
                    enhancePointType = "enhancePointC";
                    break;
                case ElementalType.Light:
                    enhancePointType = "enhancePointD";
                    break;
                case ElementalType.Dark:
                    enhancePointType = "enhancePointE";
                    break;
                default:
                    enhancePointType = "";
                    enhanceValue = 0;
                    break;
            }
        }
        else if (heroData.heroType == HeroData.HeroType.NonBattle)
        {
            switch (heroData.baseData.elementalType)
            {
                case ElementalType.NotDefined:
                    enhancePointType = "";
                    enhanceValue = 0;
                    break;
                case ElementalType.Fire:
                    enhancePointType = "enhancePointA";
                    break;
                case ElementalType.Water:
                    enhancePointType = "enhancePointB";
                    break;
                case ElementalType.Earth:
                    enhancePointType = "enhancePointC";
                    break;
                case ElementalType.Light:
                    enhancePointType = "enhancePointD";
                    break;
                case ElementalType.Dark:
                    enhancePointType = "enhancePointE";
                    break;
                default:
                    enhancePointType = "";
                    enhanceValue = 0;
                    break;
            }
        }
        
        enhanceValue = MoneyManager.GetMoney(enhancePointType).value;
        dailyMissionEnhance = 0;
        userQuestRebirth = 0;
    }

    /// <summary> 강화 서버 연결 </summary>
    void OnEnhanceServerConnect()
    {
        if (heroData != null && string.IsNullOrEmpty(heroData.id) == false)
            StartCoroutine(EnhanceServerConnect(heroData.id, heroData.enhance, heroData.rebirth));
    }

    IEnumerator EnhanceServerConnect(string heroID, int enhanceValue, int rebirthValue)
    {
        string php = "Hero.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", heroID);
        form.AddField("type", 5);


        form.AddField("enhancePointType", enhancePointType);
        form.AddField("enhanceValue", enhanceValue);
        form.AddField("rebirthType", rebirthType);
        form.AddField("rebirthValue", rebirthValue);


        form.AddField("testUsingEnhancePoint", testUsingEnhancePoint.ToString());
        form.AddField("testUsingRebirthPoint", testUsingRebirthPoint.ToString());


        string result = "";
        string error = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x, x => error = x));

        if (!string.IsNullOrEmpty(error))
            Debug.Log("Enhance Server Result Error : " + error);
        if (!string.IsNullOrEmpty(result))
        {
            Debug.Log("Enhance Result : " + result);
            
        }

        if (DailyMissionManager.Instance && DailyMissionManager.Instance.heroEnhanceCount < 5)
        {
            DailyMissionManager.Instance.heroEnhanceCount += dailyMissionEnhance;
            StartCoroutine(DailyMissionManager.Instance.SetDailyMission(DailyMissionType.HeroEnhance));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.heroEnhanceCount < 10)
        {
            UserQuestManager.Instance.heroEnhanceCount += dailyMissionEnhance;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.HeroEnhance));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.heroRebirthCount < 20)
        {
            UserQuestManager.Instance.heroRebirthCount += userQuestRebirth;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.HeroRebirth));
        }

        testUsingEnhancePoint = 0;
        testUsingRebirthPoint = 0;
    }


    /// <summary> 강화 </summary>
    public void Enhance()
    {
        if (enhanceValue < needEnhancePoint)
            return;

        MoneyManager.GetMoney(enhancePointType).value -= needEnhancePoint;

        //MoneyManager.Instance.moneyData.OnChangedMoney(enhancePointType, int.Parse("-" + needEnhancePoint));

        enhanceValue -= needEnhancePoint;
        testUsingEnhancePoint += needEnhancePoint;

        heroData.enhance++;
        dailyMissionEnhance += 1;

        UIHeroInfo.Instance.UpdateUI();
        //heroInfoUpdateUI.UpdateUI();
    }


    /// <summary> + 10 강화 </summary>
    public void TenEnhance()
    {
        if (enhanceValue < needTenEnhancePoint)
            return;

        MoneyManager.GetMoney(enhancePointType).value -= needTenEnhancePoint;

        //MoneyManager.Instance.moneyData.OnChangedMoney(enhancePointType, int.Parse("-" + needEnhancePoint));

        enhanceValue -= needTenEnhancePoint;
        testUsingEnhancePoint += needTenEnhancePoint;

        heroData.enhance += 10;
        dailyMissionEnhance += 10;

        UIHeroInfo.Instance.UpdateUI();
    }

    /// <summary> 최대까지 강화 </summary>
    public void MaxEnhance()
    {
        if (enhanceValue < needMaxEnhancePoint)
            return;

        MoneyManager.GetMoney(enhancePointType).value -= needMaxEnhancePoint;

        //MoneyManager.Instance.moneyData.OnChangedMoney(enhancePointType, int.Parse("-" + needEnhancePoint));

        enhanceValue -= needMaxEnhancePoint;
        testUsingEnhancePoint += needMaxEnhancePoint;

        heroData.enhance += (100 - heroData.enhance);
        dailyMissionEnhance += (100 - heroData.enhance);

        UIHeroInfo.Instance.UpdateUI();
    }
    /// <summary> 환생 </summary>
    public void Rebirt()
    {
        if (MoneyManager.GetMoney(MoneyType.ruby).value < needRebirthPoint)
        {
            UIPopupManager.ShowYesNoPopup("루비 부족", "상점으로 이동하시겠습니까?", OnShowShop);
            //UIPopupManager.ShowInstantPopup("루비가 부족합니다.");
            return;
        }

        MoneyManager.GetMoney(rebirthType).value -= needRebirthPoint;

        //MoneyManager.Instance.moneyData.OnChangedMoney(rebirthType, int.Parse("-" + needRebirthPoint));

        testUsingRebirthPoint += needRebirthPoint;

        heroData.rebirth++;
        heroData.enhance = 0;

        userQuestRebirth++;

        UIHeroInfo.Instance.SetDictionaryDataLevel(2);
        UIHeroInfo.Instance.UpdateUI();
        //heroInfoUpdateUI.UpdateUI();
    }
    void OnShowShop(string result)
    {
        if(result == "yes")
        {
            if (SceneLobby.Instance)
                SceneLobby.Instance.ShowShop(ShopType.Ruby);
        }
    }
}
