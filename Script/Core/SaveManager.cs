using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;

public class SaveData
{

}

[Serializable]
public class BattleSaveDataStage
{
    public BattleSaveDataStage() { }
    public BattleSaveDataStage(BattleGroup battleGroup)
    {
        id = battleGroup.battleType.ToString();
        dungeonID = battleGroup.dungeonID;
        stage = battleGroup.stage;

        //List<string> list = new List<string>();
        for (int i = 0; i < battleGroup.originalMember.Count; i++)
        {
            BattleHero hero = battleGroup.originalMember[i];

            Dictionary<string, string> saveForm = new Dictionary<string, string>();
            saveForm.Add("invenID", hero.heroData.id);
            saveForm.Add("level", hero.heroData.level.ToString());
            saveForm.Add("exp", hero.heroData.exp.ToString());    //누적 경험치

            float time = Time.unscaledTime - hero.heroData.battleStartTime;
            //Debug.Log("저장 : " + hero.heroData.heroName + " 숙련도 시작 시간: " + hero.heroData.battleStartTime + ", 숙려 시간 : " + time);
            hero.heroData.proficiencyTime = time;

            battleHeroList.Add(saveForm);
        }

        battleTime = battleGroup.battleTime.ToString();

        totalExp = battleGroup.battleLevelUpController.totalExp.ToString();
    }

    public string id;
    public string dungeonID;
    public int stage;
    public string battleTime;    
    public List<Dictionary<string, string>> battleHeroList = new List<Dictionary<string, string>>();
    public string totalExp;
}

[Serializable]
public class BattleSaveDataArtifact
{
    public BattleSaveDataArtifact() { }
    public BattleSaveDataArtifact(BattleGroup battleGroup)
    {
        id = battleGroup.battleType.ToString();

        artifactPoint = battleGroup.artifactController.artifactPoint;

        
        for (int i = 0; i < battleGroup.artifactController.artifactList.Count; i++)
        {
            Artifact artifact = battleGroup.artifactController.artifactList[i];

            Dictionary<string, string> saveForm = new Dictionary<string, string>();
            saveForm.Add("artifactID", artifact.baseData.id);
            saveForm.Add("stack", artifact.stack.ToString());            
            artifactList.Add(saveForm);
        }
    }

    public string id;
    public double artifactPoint;
    public List<Dictionary<string, string>> artifactList = new List<Dictionary<string, string>>();
    //public string artifactListJsonData;
}


[Serializable]
public class BattleSaveData
{
    public BattleSaveData(BattleGroup battleGroup)
    {
        dungeonID = battleGroup.battleType.ToString();
        stage = battleGroup.stage.ToString();
        
        List<string> battleHeroList = new List<string>();
        for (int i = 0; i < battleGroup.originalMember.Count; i++)
        {
            BattleHero hero = battleGroup.originalMember[i];

            Dictionary<string, string> saveForm = new Dictionary<string, string>();
            saveForm.Add("heroDataID", hero.heroData.id/*.heroDataID*/);
            saveForm.Add("curHP", hero.curHP.ToString());
            saveForm.Add("heroLevel", hero.heroData.level.ToString());

            string data = JsonMapper.ToJson(saveForm);
            battleHeroList.Add(data);
        }        
        heroSaveJsonData = JsonMapper.ToJson(battleHeroList);

        battleTime = battleGroup.battleTime.ToString();
        totalDropItemCount = battleGroup.totalEnhanceStoneCount.ToString();

        totalExp = battleGroup.battleLevelUpController.totalExp.ToString();
        artifactPoint = battleGroup.artifactController.artifactPoint.ToString();

        List<string> artifactSaveDataList = new List<string>();
        for (int i = 0; i < battleGroup.artifactController.artifactList.Count; i++)
        {
            Artifact artifact = battleGroup.artifactController.artifactList[i];

            Dictionary<string, string> saveForm = new Dictionary<string, string>();
            saveForm.Add("artifactID", artifact.baseData.id);
            saveForm.Add("stack", artifact.stack.ToString());


            string data = JsonMapper.ToJson(saveForm);
            artifactSaveDataList.Add(data);
        }
        artifactListJsonData = JsonMapper.ToJson(artifactSaveDataList);
    }

    public string dungeonID { get;private set; }
    public string stage { get; private set; }
    public string heroSaveJsonData { get; private set; }
    public string battleTime { get; private set; }
    public string totalDropItemCount { get; private set; }

    public string totalExp { get; private set; }
    public string artifactPoint { get; private set; }
    public string artifactListJsonData { get; private set; }


    /*
         * 영웅 레벨
         * 경험치 보유량
         * 유물 리스트
         * 유물 포인트량
         * 
         */
}


[Serializable]
public class HeroProficiencySave
{
    public HeroProficiencySave() { }
    public HeroProficiencySave(BattleGroup battleGroup)
    {
        //HeroManager.heroDataList

        List<HeroData> heroList = HeroManager.heroDataList;

        for (int i = 0; i < heroList.Count; i++)
        {
            string id = heroList[i].id;
            float proficiencyTime = heroList[i].proficiencyTime;
            heroProficiencyTimeDic.Add(id, proficiencyTime.ToString());
            
        }

        //Debug.Log(heroProficiencyTimeDic.Count+ "개 = 저장 끝");        
    }

    public Dictionary<string, string> heroProficiencyTimeDic = new Dictionary<string, string>();
}

[Serializable]
public class ProfileSaveData
{
    public string id;
}
public enum SaveType
{
    Battle,
    Profile,
    Place,
    Building
}

/// <summary> 클라이언트에 세이브와 로드를 담당한다. </summary>
public class SaveLoadManager : MonoBehaviour
{
    /// <summary> 클라이언트에 저장하기 </summary>
    /// <param name="type"> 저장하고자 하는 타입 (중복불가,데이터가 덧붙여짐)</param>
    /// <param name="data"> 저장하고자 하는 데이터 List<T> or Data Class </param>
    /// <returns></returns>
    public static IEnumerator Save( SaveType type, object data )
    {
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using (Stream fileStream = File.Open(Application.persistentDataPath + "/" + type.ToString() + ".dat", FileMode.Create))
        {
            formatter.Serialize(fileStream, data);
            yield break;
        }
    }

    public static void Save(SaveType type, string id, object data)
    {
        string fileName = Application.persistentDataPath + "/" + type.ToString() + "_" + id + ".dat";

        //데이타 비어 있으면 해당 세이브 파일 삭제
        if(data == null)
        {
            File.Delete(fileName);
            return;
        }

        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using (Stream fileStream = File.Open(fileName, FileMode.Create))
        {
            formatter.Serialize(fileStream, data);
        }

        //Debug.Log("Save data. type: " + type + ", id: " + id);
    }

    public static object Load(SaveType type, string id)
    {
        string fileName = Application.persistentDataPath + "/" + type.ToString() + "_" + id + ".dat";

        if (!File.Exists(fileName))
            return null;

        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using (Stream fileStream = File.Open(fileName, FileMode.Open))
        {
            //안전장치. 문제 생기면 제거. 불러올 때 너무 사이즈가 작은 세이브 파일은 뭔가 이상한거?
            if (fileStream.Length < 20)
                return null;

            return formatter.Deserialize(fileStream);
        }
    }

    /// <summary> 클라이언트에 저장된 데이터 불러오기 </summary>
    /// <param name="type"> 저장시킨 타입 </param>
    /// <param name="data"> object형태로 콜백됨 저장시켰던 형태로 변환하여 사용하시오</param>
    /// <returns></returns>
    public static IEnumerator Load(SaveType type,Action<object> data)
    {
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        if (!File.Exists(Application.persistentDataPath + "/" + type.ToString() + ".dat"))
        {
            yield break;
        }
            
        using (Stream fileStream = File.Open(Application.persistentDataPath + "/" + type.ToString() + ".dat", FileMode.Open))
        {
            data(formatter.Deserialize(fileStream));
            yield break;
        }
    }
    public static void Clear(SaveType type,string id)
    {
        string fileName = Application.persistentDataPath + "/" + type.ToString() + "_" + id + ".dat";
        File.Delete(fileName);
    }

    /// <summary> 클라이언트에 저장된 데이터 지우기 </summary>
    public static IEnumerator Clear(SaveType type)
    {
        string fileName = Application.persistentDataPath + "/" + type.ToString() + ".dat";
        File.Delete(fileName);
        yield break;
    }    
}   
