using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary> 전투 캐릭터의 UI 표현 (데미지 표현/HP표현) </summary>
public class HPGauge : BattleGroupElement
{
    public RectTransform canvasTransform;
    public Image progressBar;

    /// <summary> HP바 배경 오브젝트. HP바 끄고 켜기 위한 용도 </summary>
    public Transform hpBar;
    
    BattleHero battleHero;
    Transform pivot = null;
    BattleUnit battleUnit;
    float progressBarOrigalWidth;

    public Color colorRedTeam = Color.green;
    public Color colorBlueTeam = Color.red;
    public Color colorBoss = Color.yellow;

    //bool isBoss = false;

    //##############################################################
    private void Start()
    {
        progressBarOrigalWidth = progressBar.rectTransform.sizeDelta.x;
    }

    public void Init(BattleUnit _battleUnit)
    {
        if(battleUnit)
        {
            battleUnit.onChangedHP -= OnChangedHP;
            battleUnit.onDie -= OnBattleUnitDie;
            battleUnit.onRevive -= OnBattleUnitRevive;
        }

        battleUnit = _battleUnit;

        if (battleUnit is BattleHero)
            battleHero = battleUnit as BattleHero;

        if (battleUnit)
        {
            battleUnit.onChangedHP += OnChangedHP;
            battleUnit.onDie += OnBattleUnitDie;
            battleUnit.onRevive += OnBattleUnitRevive;



            SetBattleGroup(battleUnit.battleGroup);

            Battle.onChangedBattleGroup += OnChangedBattleGroup;

            //if (battleUnit.heroData.baseData.id.Contains("_Mon") && battleUnit.heroData.baseData.grade >= 3)
            //    isBoss = true;
            //else
            //    isBoss = false;

            if (battleUnit.team == BattleUnit.Team.Red)
            {
                progressBar.color = colorRedTeam;
            }   
            else
            {
                if (battleHero.isBoss)
                {
                    progressBar.color = colorBoss;
                    hpBar.transform.localScale = Vector2.one * 1.5f;
                }
                else
                {
                    progressBar.color = colorBlueTeam;
                    hpBar.transform.localScale = Vector2.one;
                }
            }


            //orderController = battleUnit.GetComponent<OrderController>();

            pivot = battleUnit.transform.Find("UIPivot");
            if (!pivot)
                pivot = battleUnit.transform.Find("Character").transform.Find("UIPivot");
                    }
        else
        {
            SetBattleGroup(null);
            Battle.onChangedBattleGroup -= OnChangedBattleGroup;
            pivot = null;

        }

        UpdateGuage();
    }

    void OnChangedBattleGroup(BattleGroup b)
    {
        if (b != Battle.currentBattleGroup)
            return;

        UpdateGuage();
    }

    void Update()
    {
        if (battleUnit == null)
            return;

        if (battleUnit.battleGroup == null)
            return;

        if (Battle.currentBattleGroup == null)
            return;

        //if (battleUnit.battleGroup && battleUnit.battleGroup == Battle.currentBattleGroup)
        //    OnChangedHP();

        if (hpBar && !hpBar.gameObject.activeSelf)
            return;

        if(pivot != null)
            transform.position = pivot.transform.position;
    }

    void UpdateGuage()
    {
        if (battleGroup != Battle.currentBattleGroup)
            return;

        if (!hpBar)
            return;

        if (!battleUnit)
        {
            hpBar.gameObject.SetActive(false);
            return;
        }
        
        if(battleUnit.stats.GetParam(StatType.CurHP) == null || battleUnit.stats.GetParam(StatType.MaxHP) == null)
        {
            hpBar.gameObject.SetActive(false);
            return;
        }

        //체력이 꽉 차 있으면 게이지 감추기
        hpBar.gameObject.SetActive(!battleUnit.isDie && battleUnit.curHP < battleUnit.maxHP);

        double a = battleUnit.curHP / battleUnit.maxHP;

        progressBar.fillAmount = (float)a;
    }

    void OnEnable()
    {
        if (progressBarOrigalWidth == 0)
        {
            progressBarOrigalWidth = progressBar.rectTransform.sizeDelta.x;
        }
    }

    void OnDisable()
    {
        if (battleUnit != null)
        {
            battleUnit.onChangedHP -= OnChangedHP;
        }
    }

    void OnBattleUnitDie(BattleUnit unit)
    {
        if (!hpBar)
            return;

        //죽으면 hp바 보여주지 않음
        hpBar.gameObject.SetActive(false);
    }

    void OnBattleUnitRevive(BattleUnit unit)
    {
        if (!hpBar)
            return;

        if (pivot != null)
            transform.position = pivot.transform.position;
                
        hpBar.gameObject.SetActive(true);
    }

    void OnChangedHP()
    {
        UpdateGuage();
    }   

}
