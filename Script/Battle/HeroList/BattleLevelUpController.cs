using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary> 전투중 발생하는 레벨업을 컨트럴하는 클래스 </summary>
public class BattleLevelUpController : MonoBehaviour {

    public delegate void BattleGroupDelegate(BattleGroup battleGroup);

    public BattleGroupDelegate onChangedTotalExp;

    public BattleGroup battleGroup { get; private set; }

    double _totalExp = 0;
    public double totalExp
    {
        get
        {
            return _totalExp;
        }
        set
        {
            bool isChanged = _totalExp != value;

            _totalExp = value;

            //값이 변경된 경우 콜백
            if (isChanged && onChangedTotalExp != null)
                onChangedTotalExp(battleGroup);

        }
    }    

    public List<BattleHero> heroList
    {
        get
        {
            if (battleGroup == null)
                return null;

            if (battleGroup.originalMember == null)
                return null;

            return battleGroup.originalMember;
        }
    }

    void Awake()
    {
        battleGroup = GetComponent<BattleGroup>();
        //battleGroup.onChangedStage += OnChangedStage;
        battleGroup.onRestartBattle += OnRestartBattle;
        battleGroup.onLoadData += OnLoadData;

        battleGroup.originalMember.onRemove += OnRemoveHero;

        //몬스터 리스트 추가, 삭제 될 때 콜백 등록
        battleGroup.blueTeamList.onAdd += OnAddMonster;
        battleGroup.blueTeamList.onRemove += OnRemoveMonster;
        battleGroup.blueTeamList.onClear += OnClearMonsterList;

        for (int i = 0; i < UIBattleLevelUp.battleHeroSlotList.Count; i++)
        {
            UIBattleLevelUp.battleHeroSlotList[i].onClickLevelUp -= OnClickLevelUp;
            UIBattleLevelUp.battleHeroSlotList[i].onClickLevelUp += OnClickLevelUp;
        }

        //균등 레벨업 버튼 콜백
        UIBattleLevelUp.onClickLevelUpEvenly += OnClickLevelUpEvenly;
    }

    void OnRemoveHero(BattleHero hero)
    {
        double d = 200d * System.Math.Pow(1.2d, hero.heroData.level - 1) - 200d;

        totalExp += d;
        hero.heroData.level = 1;
        hero.heroData.exp = 0;
    }

    void OnAddMonster(BattleHero monster)
    {
        //Debug.Log(battleGroup.id + " - add monster : " + monster.name);

        monster.onDie += OnDieMonster;
    }

    void OnRemoveMonster(BattleHero monster)
    {
        //Debug.Log(battleGroup.id + " - remove monster : " + monster.name);

        monster.onDie -= OnDieMonster;
    }

    void OnClearMonsterList()
    {
        //Debug.Log(battleGroup.id + " - clear monster");

        for (int i =0; i < battleGroup.blueTeamList.Count; i++)
        {
            battleGroup.blueTeamList[i].onDie -= OnDieMonster;
        }
    }

    public bool showDebug = false;

    //몬스터 사망하면 경험치 증가
    void OnDieMonster(BattleUnit monster)
    {
        //유물에 의한 보정. (적 처치시 경험치 증가)
        float increaseRate = 0f;
        Artifact artifactExp = battleGroup.artifactController.artifactList.Find(x => x.baseData.type == "IncreaseExp");
        if(artifactExp != null)
        {
            float a = 0f;
            float.TryParse(artifactExp.baseData.formula, out a);
            increaseRate = a * artifactExp.stack * 0.01f;   //formula가 10 이렇게 되어 있음. (10%를 의미)
        }

        float increaseByPlace = 0f;
        for(int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
            if(placeData.placeBaseData.type == "Battle_IncreaseExpRatio")
            {
                float a = 0f;
                float.TryParse(placeData.placeBaseData.formula, out a);
                increaseByPlace += a * placeData.placeLevel * 0.01f;
            }
        }

        double exp = monster.power * 5 * (1 + increaseRate) * (1 + increaseByPlace);
        
        if(monster is BattleHero)
        {
            BattleHero m = monster as BattleHero;
            if (m.isBoss)
            {
                //보스는 8분할 해서 줌. (경험치 구슬 8개 떨굼)
                exp = m.power * 5f * 0.125f;
                for(int i = 0; i < 8; i++)
                {
                    SpawnLootObject(monster, exp);
                }
            }
            else
                SpawnLootObject(monster, exp);

        }


        if (showDebug)
            Debug.Log("[" + battleGroup.battleType + "] " + monster.name + ". level: " + monster.heroData.level + " - die. exp : " + exp);

        //totalExp += exp;
    }

    void SpawnLootObject(BattleUnit monster, double exp)
    {
        GameObject go = Battle.GetObjectInPool(UIBattleLevelUp.lootObjectExpPrefab.name);
        if (!go)
        {
            go = Instantiate(UIBattleLevelUp.lootObjectExpPrefab, battleGroup.canvasObject.transform);
            go.name = UIBattleLevelUp.lootObjectExpPrefab.name;
            Battle.AddObjectToPool(go);
        }

        LootObjectBase lootObject = go.GetComponent<LootObjectBase>();
        lootObject.gameObject.SetActive(true);
        lootObject.Init(battleGroup, exp, monster.transform.position);
        //go.transform.position = monster.transform.position;
    }

    void OnLoadData(BattleGroup _battleGroup, BattleSaveDataStage data)
    {
        if (battleGroup != _battleGroup)
            return;

        //Debug.Log(data.dungeonID + ", exp: " + data.totalExp);
        double result = 0;
        if (double.TryParse(data.totalExp, out result))
        {
            totalExp = result;
        }
    }
    
    public double expLevelUpEvenly
    {
        get
        {
            if (battleGroup.originalMember == null || battleGroup.originalMember.Count == 0)
                return 0;

            int minLevel = 0;
            battleGroup.originalMember.Min(x => minLevel = x.heroData.level);
            double requiredExp = 0d;
            for (int i = 0; i < battleGroup.originalMember.Count; i++)
            {
                BattleHero hero = battleGroup.originalMember[i];
                if (hero.heroData.level == minLevel)
                    requiredExp += hero.LevelUpExpValue;
            }

            return requiredExp;
        }
    }

    void OnClickLevelUpEvenly()
    {
        if (battleGroup != Battle.currentBattleGroup)
            return;

        //가장 낮은 레벨
        int minLevel = 0;
        battleGroup.originalMember.Min(x => minLevel = x.heroData.level);
        
        //필요한 경험치 총량
        double requiredExp = 0d;
        for(int i = 0; i < battleGroup.originalMember.Count; i++)
        {
            BattleHero hero = battleGroup.originalMember[i];
            if (hero.heroData.level == minLevel)
                requiredExp += hero.LevelUpExpValue;
        }


        //Debug.Log(minLevel + ", count: " + heroCount + ", required exp: " + requiredExp + ", totalExp" + totalExp);

        //균등하게 올리기 위해 필요한 경험치가 전체 경험치 보다 많으면 실행 안 됨
        if (requiredExp > totalExp)
            return;
        
        //레벨 젤 낮은 애들만 레벨 올림
        for (int i = 0; i < battleGroup.originalMember.Count; i++)
        {
            BattleHero hero = battleGroup.originalMember[i];
            if (hero.heroData.level == minLevel)
            {
                //totalExp -= hero.LevelUpExpValue;
                hero.LevelUp(hero.LevelUpExpValue);
            }
        }

        totalExp -= requiredExp;

    }

    void OnClickLevelUp(BattleHero battleHero)
    {
        if (heroList == null)
            return;

        if (battleGroup != Battle.currentBattleGroup)
            return;

        if (battleHero.LevelUpExpValue > totalExp)
        {
            Debug.Log("Not enough exp");
            return;
        }
        
        totalExp -= battleHero.LevelUpExpValue;
        battleHero.LevelUp(battleHero.LevelUpExpValue);
    }
    
    void OnRestartBattle(BattleGroup _battleGroup)
    {
        if (battleGroup != _battleGroup)
            return;

        totalExp = 0;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (battleGroup != Battle.currentBattleGroup)
            return;

        //치트키
        if (Input.GetKeyDown(KeyCode.F9))
            totalExp += 10000000;

        if (Input.GetKeyDown(KeyCode.F11))
            OnClickLevelUpEvenly();
    }
#endif
}
