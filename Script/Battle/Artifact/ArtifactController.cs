using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;
using B83.ExpressionParser;
using LitJson;
using System.IO;
using CodeStage.AntiCheat.ObscuredTypes;


/// <summary> 전투에서 획득한 유물 데이터 관리 </summary>
public class ArtifactController : MonoBehaviour
{
    //artifactPoint가 변경되었을 때 발생
    public SimpleDelegate onChangedArtifactPoint;

    public float attackPower { get; private set; }
    public float maxHp { get; private set; }

    /// <summary> 소유하고 있는 유물 리스트 </summary>
    public List<Artifact> artifactList = new List<Artifact>();
    
    public List<string> showArtifactIDList { get; private set; }

    ObscuredDouble _artifactPoint = 0;
    public ObscuredDouble artifactPoint
    {
        get { return _artifactPoint; }
        set
        {
            bool isChanged = _artifactPoint != value;

            _artifactPoint = value;

            if (isChanged && onChangedArtifactPoint != null)
                onChangedArtifactPoint();
        }
    }

    public ObscuredDouble cost
    {
        get
        {
            int count = artifactList.Sum(x=>x.stack);

            double d = 14d * System.Math.Pow(2.3d, count);
            if (d < 1000)
                d = System.Math.Round(d, 0);

            return d;
        }
    }


    public BattleGroup battleGroup { get; private set; }
        

    void Awake()
    {   
        showArtifactIDList = new List<string>();

        battleGroup = GetComponent<BattleGroup>();
        battleGroup.onChangedStage += OnChangedStage;
        battleGroup.onStopBattle += OnStopBattle;
        battleGroup.onRestartBattle += OnRestartBattle;

        battleGroup.redTeamList.onAdd += OnAddHero;
        battleGroup.redTeamList.onRemove += OnRemoveHero;

        //몬스터 리스트 추가, 삭제 될 때 콜백 등록
        battleGroup.blueTeamList.onAdd += OnAddMonster;
        battleGroup.blueTeamList.onRemove += OnRemoveMonster;
        battleGroup.blueTeamList.onClear += OnClearMonsterList;

        //영지 효과 적용을 위해 콜백 등록
        TerritoryManager.onAddPlace += UpdatePlaceModify;
        TerritoryManager.onChangedPlaceData += UpdatePlaceModify;
        UpdatePlaceModify();
    }

    double placeModifyIncreaseAmount = 0d;

    void UpdatePlaceModify()
    {
        //영지 효과 퀘스트 보상 증가
        placeModifyIncreaseAmount = 0f;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
            if (placeData.placeBaseData.type == "Battle_IncreaseArtifectPoint")
            {
                float a = 0f;
                float.TryParse(placeData.placeBaseData.formula, out a);
                placeModifyIncreaseAmount += a * 0.01f * placeData.placeLevel;
            }
        }

        //Debug.Log(placeModifyIncreaseAmount);
    }


    bool isInitialized = false;
    IEnumerator Start()
    {
        while (!battleGroup.isInitialized)
            yield return null;

        string fileName = Application.persistentDataPath + "/" + battleGroup.battleType + "_" + User.Instance.userID + "_Artifact.dat";

        if (!File.Exists(fileName))
            yield break;

        string json = File.ReadAllText(fileName);

        //Debug.Log("[Load]" + fileName + ", " + json);

        JsonData jsonData = JsonMapper.ToObject(json);
        
        //습득한 유물 리스트 로딩
        //if (!string.IsNullOrEmpty(data.artifactListJsonData))
        {
            JsonData jsonArtifactList = jsonData["artifactList"];
            for (int i = 0; i < jsonArtifactList.Count; i++)
            {
                JsonData _json = jsonArtifactList[i];

                string artifactID = _json["artifactID"].ToString();
                int stack = _json["stack"].ToInt();
                for (int k = 0; k < stack; k++)
                {
                    AddArtifact(GameDataManager.ArtifactBaseDataDic[artifactID], false);
                }
            }
        }

        //유물포인트 로딩
        artifactPoint = jsonData["artifactPoint"].ToDouble();

        isInitialized = true;
    }
    
    void OnAddMonster(BattleHero monster)
    {
        monster.onDie += OnDieMonster;
    }

    void OnRemoveMonster(BattleHero monster)
    {
        monster.onDie -= OnDieMonster;
    }

    void OnClearMonsterList()
    {
        for (int i = 0; i < battleGroup.blueTeamList.Count; i++)
        {
            battleGroup.blueTeamList[i].onDie -= OnDieMonster;
        }
    }

    void OnAddHero(BattleHero hero)
    {
        for (int i = 0; i < artifactList.Count; i++)
        {
            hero.buffController.AttachBuff(hero, artifactList[i].baseData.buffID);
        }
    }

    void OnRemoveHero(BattleHero hero)
    {

    }

    //몬스터 사망하면 유물 떨어짐
    void OnDieMonster(BattleUnit monster)
    {
        //유물은 보스만 떨굼        
        if (!monster.isBoss)
            return;



        //획득량 계산. 관련 유물 스택당 획득량 10% 증가        
        var artifact = artifactList.Find(x => x.baseData.type == "IncreaseArtifact");
        float artifactModify = artifact != null ? artifact.stack * 0.1f : 0f;

        //최종 획득량
        double artifactPoint = monster.power * 5d * (1f + artifactModify) + (1f + placeModifyIncreaseAmount);

        //Debug.Log(placeModifyIncreaseAmount);

        //Debug.Log(artifactPoint + ", " + artifactModify);

        //1층 기준 보스 rating이 3. 유물 포인트 1이 나오겎므 하기 위해 3으로 나눔
        artifactPoint = monster.power * 0.34f;

        //정수로 변환하다면 변환. (반올림처리)
        if (artifactPoint < int.MaxValue)
            artifactPoint = System.Math.Round(artifactPoint, 0);

        //for (int i = 0; i < 1; i++)
        {
            SpawnLootObject(monster, artifactPoint);
        }
        
        //if (showDebug)
            //Debug.Log("[" + battleGroup.id + "] " + monster.name + ". level: " + monster.heroData.level + " - die. artifact : " + artifact);
    }

    void SpawnLootObject(BattleUnit monster, double point)
    {
        GameObject go = Battle.GetObjectInPool(UIArtifact.lootObjectPrefab.name);
        if (!go)
        {
            go = Instantiate(UIArtifact.lootObjectPrefab, battleGroup.canvasObject.transform);
            go.name = UIArtifact.lootObjectPrefab.name;
            Battle.AddObjectToPool(go);
        }

        LootObjectBase lootObject = go.GetComponent<LootObjectBase>();
        lootObject.gameObject.SetActive(true);
        lootObject.Init(battleGroup, point, monster.transform.position);
    }

    void OnStopBattle(BattleGroup _battleGroup)
    {
        if (battleGroup != _battleGroup)
            return;

        for (int i = 0; i < artifactList.Count; i++)
        {
            if (artifactList[i].baseData.type == "hero")
            {
                for (int j = 0; j < battleGroup.redTeamList.Count; j++)
                {
                    battleGroup.redTeamList[j].buffController.DetachBuff(artifactList[i].baseData.buffID);
                }
            }
        }

        artifactList.Clear();
        artifactPoint = 0;

        Battle.SaveArtifactInfo(battleGroup);
    }

    void OnRestartBattle(BattleGroup _battleGroup)
    {
        if (battleGroup != _battleGroup)
            return;

        //Debug.Log("OnRestart battle");

        artifactPoint = 0;
        for (int i = 0; i < artifactList.Count; i++)
        {
            if (artifactList[i].baseData.type == "hero")
            {
                for (int j = 0; j < battleGroup.redTeamList.Count; j++)
                {                    
                    battleGroup.redTeamList[j].buffController.DetachBuff(artifactList[i].baseData.buffID);
                }
            }
        }

        artifactList.Clear();

        if (onChangedArtifactList != null)
            onChangedArtifactList(battleGroup);

        showArtifactIDList.Clear();
    }


    /// <summary> 해당 공식을 계산한다. </summary
    public float Parse(Artifact artifact , string formula)
    {
        if(string.IsNullOrEmpty(formula))
        {
            return 0f;
        }

        formula = formula.Replace(" ", string.Empty);
        formula = formula.Replace("stack", artifact.stack.ToString());


        return GetParamValue(artifact, formula);

    }

    ExpressionParser parser = new ExpressionParser();

    float GetParamValue(Artifact artifact, string paramString)
    {
        //selft, target이런거 제외            
        //self. target. party. global. (타입 구분은 미정)
        paramString = paramString.Replace("owner.", "");
        paramString = paramString.Replace("self.", "");
        paramString = paramString.Replace("target.", "");
        paramString = paramString.Replace("master.", "");
        paramString = paramString.Trim();

        Expression exp = parser.EvaluateExpression(paramString);

        List<string> keys = exp.Parameters.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            string propertyName = keys[i];

            System.Type type = artifact.GetType().BaseType;
            //패러미터 못 찾을 경우 기본 값은 1. (주로 곱셈 연산이 많을 것 같아서? 해보고 이상하면 0으로..)
            float value = 1f;

            PropertyInfo p = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (p == null)
            {
                Debug.LogWarning("Cannot find property : " + artifact + propertyName);
                //continue;
            }
            else
            {
                float.TryParse(p.GetValue(artifact, null).ToString(), out value);
                //Debug.Log(unit.name + ", " + p.Name + ": " + value);
            }

            exp.Parameters[keys[i]].Value = value;
        }

        //Debug.Log(unit.name + ", " + paramString + " => " + exp.Value);        

        return (float)exp.Value;
    }
    
    void OnChangedStage(BattleGroup _battleGroup)
    {
        if (isInitialized)
            Battle.SaveArtifactInfo(battleGroup);
    }
    
    /// <summary> 버프 전체에서 3개 랜덤하게 보여줌 </summary>
    public void ShowBuff()
    {
        if (showArtifactIDList.Count > 0)
            return;

        int count = 3;

        //유물 리스트
        List<ArtifactBaseData> artifactBaseDataList = GameDataManager.ArtifactBaseDataDic.Values.ToList();

        //사용 가능한 애들만 보여줌
        artifactBaseDataList = artifactBaseDataList.FindAll(x => x.enable);

        for (int i = 0; i < count; )
        {
            ArtifactBaseData artifactBaseData = null;            
            while (artifactBaseData == null)
            {
                //최대 스택이면 선택지에서 제외
                int index = Random.Range(0, artifactBaseDataList.Count);
                ArtifactBaseData data = artifactBaseDataList[index];
                Artifact a = artifactList.Find(x => x.baseData == data);
                if (data.maxStack > 0 && a != null && a.stack >= data.maxStack)
                    continue;
                
                artifactBaseData = data;
            }

            if (showArtifactIDList.Find(x => x == artifactBaseData.id) == null)
            {
                showArtifactIDList.Add(artifactBaseData.id);
                i++;
            }
        }
    }

    public delegate void OnArtifactAdd(BattleGroup battleGroup);

    /// <summary> artifactList에 뭔가 추가되거나 제거 초기화 또는 리스트 내의 내용이 변경되었을 때 발생 </summary>
    public OnArtifactAdd onChangedArtifactList;

    /// <summary> 유물 획득(추가) </summary>
    public void AddArtifact(ArtifactBaseData data, bool forceSave = true)
    {
        Artifact artifact = null;
        if (artifactList == null)
            return;

        bool isAdd = false;
        if (artifactList.Count <= 0)
            isAdd = true;

        else
        {
            isAdd = true;
            for (int i = 0; i < artifactList.Count; i++)
            {               
                if(artifactList[i].baseData.id == data.id)
                {
                    if (artifact != null)
                        continue;
                    artifact = artifactList[i];
                    isAdd = false;                    
                }
            }
        }

        //아티펙트 포인트 감소       
        artifactPoint -= cost;
        
        //목록에 추가
        if (isAdd)
        {
            artifact = new Artifact(data);
            artifactList.Add(artifact);            
        }

        artifact.stack++;
        if (artifact.baseData.type == "hero")
        {
            for (int i = 0; i < battleGroup.redTeamList.Count; i++)
            {
                battleGroup.redTeamList[i].buffController.AttachBuff(battleGroup.redTeamList[i], artifact.baseData.buffID);
            }
        }

        //유물 선택지 리스트 초기화
        showArtifactIDList.Clear();

        //유물 선택창 닫기
        UIArtifactSelect.Close();

        if (onChangedArtifactList != null)
            onChangedArtifactList(battleGroup);

        //유물 선택 후 강제 저장 함
        if(forceSave)
            Battle.SaveArtifactInfo(battleGroup);
    }    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if(battleGroup == Battle.currentBattleGroup)
                artifactPoint = 1e100;
        }

    }
}

