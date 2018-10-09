using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;

/// <summary> 전투에서 사용하는 (출전중인)영웅 슬롯 </summary>
public class BattleHeroSlot : MonoBehaviour {

    public Image imageHero;

    public Text heroName;

    public Text heroLevel;

    public Text textRebirth;

    public Text textEnhance;

    public Text textCurHP;

    public Text textMaxHP;

    public Text textAttackPower;

    public Text textDefensePower;

    public Text textRegenTime;

    /// <summary> 사망 상태일 때 보이는 오브젝트 </summary>
    public GameObject toggledObjectDie;

    public Text levelUpExp;

    public Button buttonLevelUp;

    public delegate void OnClickLevelUp(BattleHero heroData);
    public OnClickLevelUp onClickLevelUp;

    BattleHero battleHero;

    void Start()
    {
        Battle.onChangedBattleGroup += OnChangedBattleGroup;        
    }
    
    void OnEnable()
    {
        if (!battleHero)
            return;

        //HP정보 갱신
        UpdateHP();

        //그 외 수치 정보 갱신
        UpdateStats();
    }

    void OnDisable()
    {
        //사망상태 표시, 부활 쿨타임 표시 연출 중단
        if (coroutineShowRegenTime != null)
        {
            StopCoroutine(coroutineShowRegenTime);
            coroutineShowRegenTime = null;
        }

        textRegenTime.gameObject.SetActive(false);
    }

    public void SetBattleHero(BattleHero _battleHero)
    {
        //데이타 바뀐거 없으면 패스
        if (battleHero == _battleHero)
            return;

        //기존 데이타에 걸려 있는 콜백들 제거
        if (battleHero)
        {
            battleHero.onChangedHP -= OnChangedHeroHP;
            //battleHero.onDie -= OnChangedHeroHP;
            if (battleHero.heroData != null)
            {
                battleHero.heroData.onChangedLevel -= OnChangedHeroLevel;
                //battleHero.stats.GetParam(StatType.MaxHP).
                battleHero.stats.GetParam(StatType.MaxHP).onChangedValue -= OnChangedParam;
                battleHero.stats.GetParam(StatType.AttackPower).onChangedValue -= OnChangedParam;
                battleHero.stats.GetParam(StatType.DefensePower).onChangedValue -= OnChangedParam;
                //battleHero.onChangedMaxHPRatio -= OnChangedParam;
                //battleHero.onChangedMaxHPValue -= OnChangedParam;
                //battleHero.onChangedAttackPowerRatio -= OnChangedParam;
                //battleHero.onChangedAttackPowerValue -= OnChangedParam;
                //battleHero.onChangedDefensePowerRatio -= OnChangedParam;
                //battleHero.onChangedDefensePowerValue -= OnChangedParam;
            }
                

            if (battleHero.battleGroup)
                battleHero.battleGroup.battleLevelUpController.onChangedTotalExp -= OnChangedTotalExp;

            //사망상태 표시, 부활 쿨타임 표시 연출 중단
            if (coroutineShowRegenTime != null)
            {
                StopCoroutine(coroutineShowRegenTime);
                coroutineShowRegenTime = null;
            }

            textRegenTime.gameObject.SetActive(false);
        }

        //데이타 교체
        battleHero = _battleHero;

        //새로운 데이타에 다시 콜백 걸기
        if (battleHero)
        {
            battleHero.onChangedHP += OnChangedHeroHP;
            //battleHero.onDie += OnChangedHeroHP;
            if (battleHero.heroData != null)
            {
                battleHero.heroData.onChangedLevel += OnChangedHeroLevel;
                battleHero.stats.GetParam(StatType.MaxHP).onChangedValue += OnChangedParam;
                battleHero.stats.GetParam(StatType.AttackPower).onChangedValue += OnChangedParam;
                battleHero.stats.GetParam(StatType.DefensePower).onChangedValue += OnChangedParam;
                //battleHero.onChangedMaxHPRatio += OnChangedParam;
                //battleHero.onChangedMaxHPValue += OnChangedParam;
                //battleHero.onChangedAttackPowerRatio += OnChangedParam;
                //battleHero.onChangedAttackPowerValue += OnChangedParam;
                //battleHero.onChangedDefensePowerRatio += OnChangedParam;
                //battleHero.onChangedDefensePowerValue += OnChangedParam;
            }
                

            battleHero.battleGroup.battleLevelUpController.onChangedTotalExp += OnChangedTotalExp;
            
        }

        //초상화 표시
        if (imageHero)
            InitImage();
            //StartCoroutine(InitHeroImage());

        //이름
        heroName.text = battleHero.heroData.heroName;

        //등급
        ShowGrade(battleHero.heroData.baseData.grade);

        //속성
        ShowElemental();

        //HP정보 갱신
        UpdateHP();

        //그 외 수치 정보 갱신
        UpdateStats();

        UpdateLevelUpButtonStatus();
    }    
    
    //체력, 공격력이 바뀌었을 때
    void OnChangedParam()
    {
        UpdateStats();
    }

    void OnChangedBattleGroup(BattleGroup battleGroup)
    {
        //if (Battle.currentBattleGroup == battleGroup)
            //UpdateLevelUpButtonStatus();
    }

    //현재 배틀 그룹의 총 경험치량이 변경되었을 때
    void OnChangedTotalExp(BattleGroup battleGroup)
    {
        if (Battle.currentBattleGroup == battleGroup)
            UpdateLevelUpButtonStatus();
    }

    void UpdateLevelUpButtonStatus()
    {
        //레벨업 가능 상태에 따라서 레벨업 버튼 활성 비활성 처리
        buttonLevelUp.interactable = battleHero.LevelUpExpValue < Battle.currentBattleGroup.battleLevelUpController.totalExp;
    }

    /// <summary> 현재 슬롯의 영웅 레벨이 변경되었을 때 </summary>
    void OnChangedHeroLevel()
    {
        UpdateHP();
        UpdateStats();
    }

    /// <summary> 수치 정보 변경 </summary>
    void UpdateStats()
    {
        if (!gameObject.activeInHierarchy)
            return;

        heroLevel.text = battleHero.heroData.level.ToString();
        levelUpExp.text = battleHero.LevelUpExpValue.ToStringABC();

        //공격력 표시
        if (textAttackPower)
        {
            double attackPower = battleHero.stats.GetParam(StatType.AttackPower).value;
            textAttackPower.text = attackPower.ToStringABC();
        }            

        //방어력 표시
        if (textDefensePower)
        {
            double defensePower = battleHero.stats.GetParam(StatType.DefensePower).value;
            textDefensePower.text = defensePower.ToStringABC();
        }

        //환생
        string rebirth = battleHero.heroData.rebirth > 0 ? battleHero.heroData.rebirth.ToString() : string.Empty;
        textRebirth.text = rebirth;

        //강화
        string enhance = battleHero.heroData.enhance > 0 ? battleHero.heroData.enhance.ToString() : string.Empty;
        textEnhance.text = enhance;

    }

    void InitImage()
    {
        AssetLoader.AssignImage(imageHero, "sprite/hero", "Atlas_HeroImage", battleHero.heroData.heroImageName, OnFinisthInitImage);
    }

    void OnFinisthInitImage(string result)
    {
    }
    
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

    Color originalBGColor;

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
    /// <summary> 속성 표시 </summary>
    void ShowElemental()
    {
        objectElementalFire.SetActive(false);
        objectElementalWater.SetActive(false);
        objectElementalEarth.SetActive(false);
        objectElementalLight.SetActive(false);
        objectElementalDark.SetActive(false);
        objectElementalNotDefined.SetActive(false);

        //일단 현재 시점에서 중요한 정보가 아닌 것 같아서 하이드 시킴 (2018-02-20. 화중)
        return;

        HeroBaseData data = battleHero.heroData.baseData;
        objectElementalFire.SetActive(data.elementalType == ElementalType.Fire);
        objectElementalWater.SetActive(data.elementalType == ElementalType.Water);
        objectElementalEarth.SetActive(data.elementalType == ElementalType.Earth);
        objectElementalLight.SetActive(data.elementalType == ElementalType.Light);
        objectElementalDark.SetActive(data.elementalType == ElementalType.Dark);
        objectElementalNotDefined.SetActive(false);
    }

    void OnChangedHeroHP()
    {
        UpdateHP();
    }

    void UpdateHP()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (textCurHP)
        {
            double curHP = battleHero.curHP;
            textCurHP.text = curHP.ToStringABC();
        }            

        if (textMaxHP)
        {
            double maxHP = battleHero.stats.GetParam(StatType.MaxHP).value;
            textMaxHP.text = maxHP.ToStringABC();
        }   

        toggledObjectDie.SetActive(battleHero.isDie);

        return;
        //부활 타이머 표시
        if (battleHero.isDie)
        {
            if (coroutineShowRegenTime == null)
                coroutineShowRegenTime = StartCoroutine(ShowRegenTime());
        }
        else
        {
            if (coroutineShowRegenTime != null)
            {
                StopCoroutine(coroutineShowRegenTime);
                coroutineShowRegenTime = null;
            }

            textRegenTime.gameObject.SetActive(false);
        }
    }

    Coroutine coroutineShowRegenTime = null;
    IEnumerator ShowRegenTime()
    {
        textRegenTime.gameObject.SetActive(true);

        while (battleHero.isDie)
        {
            int time = (int)Mathf.Ceil(battleHero.regenTime - Time.time);
            textRegenTime.text = time.ToString();
            yield return null;
        }

        textRegenTime.gameObject.SetActive(false);

        coroutineShowRegenTime = null;
    }

    public void OnClickLevelUpButton()
    {
        //사용 안 함. OnPointerDown으로 변경
        return;
    }

    public void OnPointerDown()
    {
        if (Battle.currentBattleGroup == null)
            return;

        if (coroutineLevelUp != null)
            StopCoroutine(coroutineLevelUp);

        coroutineLevelUp = StartCoroutine(LevelUp());
    }

    public void OnPointerUp()
    {
        if (coroutineLevelUp != null)
            StopCoroutine(coroutineLevelUp);
    }

    Coroutine coroutineLevelUp = null;
    IEnumerator LevelUp()
    {
        float interval = 0.5f;
        while (battleHero.LevelUpExpValue <= Battle.currentBattleGroup.battleLevelUpController.totalExp)
        {
            if (onClickLevelUp != null)
                onClickLevelUp(battleHero);

            UIBattleLevelUp.PlaySoundLevelUp();

            yield return new WaitForSecondsRealtime(interval);

            if (interval > 0.01f)
                interval *= 0.8f;

            if (interval < 0.01f)
                interval = 0.01f;
        }

        yield break;
    }

    Coroutine coroutineShowHeroInfo;
    public void OnClick()
    {
        if (coroutineShowHeroInfo != null)
            StopCoroutine(coroutineShowHeroInfo);

        coroutineShowHeroInfo = StartCoroutine(ShowHeroInfo());
    }


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
            UIHeroInfo.Init(battleHero);
    }
}
