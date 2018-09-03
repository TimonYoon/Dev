using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using B83.ExpressionParser;

public class SkillManager : MonoBehaviour {

    static public SkillManager Instance;

    List<SkillData> _skillDataList = new List<SkillData>();

    static public List<SkillData> skillDataList
    {
        get
        {
            if (!Instance)
                return null;

            return Instance._skillDataList;
        }
    }

    void Awake()
    {
        Instance = this;
    }

	void Start()
	{
		StartCoroutine(InitSkillData());
	}

	IEnumerator InitSkillData()
	{
        JsonData jData = null;
        yield return StartCoroutine(AssetLoader.LoadJsonData("json", "Skill.json", x => jData = x));
        if(jData == null)
        {
            Debug.LogWarning("Failed to load skill json data");
            yield break;
        }

        for (int i = 0; i < jData.Count; i++)
        {
            Debug.Log(jData[i]["id"]);

            SkillData skillData = new SkillData(jData[i]);

            _skillDataList.Add(skillData);
        }
	}
}
