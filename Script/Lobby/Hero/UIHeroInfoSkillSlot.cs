using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroInfoSkillSlot : MonoBehaviour {

    public Text textName;

    public Text textDescription;

    private void OnDisable()
    {
        heroData = null;
        territorySkillData = null;
        skillData = null;
    }

    HeroData _heroData;
    public HeroData heroData
    {
        get { return _heroData; }
        set
        {
            if (_heroData != null)
                _heroData.onChangedValue -= OnChangedHeroVale;

            _heroData = value;

            if (value != null)
                _heroData.onChangedValue += OnChangedHeroVale;

            //_heroData.on
        }
    }

    public SkillData skillData
    {
        set
        {
            if (value == null)
            {
                textName.text = string.Empty;
                textDescription.text = string.Empty;                
            }
            else
            {
                textName.text = value.name;
                textDescription.text = value.description;
            }

            gameObject.SetActive(value != null);
        }
    }

    TerritorySkillData _territorySkillData;
    public TerritorySkillData territorySkillData
    {
        get { return _territorySkillData; }
        set
        {
            _territorySkillData = value;
            UpdateDescription(value);
        }
    }

    void OnChangedHeroVale(PropertyInfo p)
    {
        if (p.Name == "enhance" || p.Name == "rebirth" || p.Name == "level")
            UpdateDescription(territorySkillData);

    }

    void UpdateDescription(TerritorySkillData _territorySkillData)
    {
        TerritorySkillData value = _territorySkillData;
        if (value == null)
        {
            textName.text = string.Empty;
            textDescription.text = string.Empty;
        }
        else
        {
            string name = value.name;
            string description = value.description;

            string item = value.fillterItem;
            string category = value.fillterCategory;
            if (GameDataManager.itemDic.ContainsKey(item))
            {
                item = GameDataManager.itemDic[item].name;
                name = name.Replace("[item]", "<color=#FFE57AFF>" + item + "</color>");
                description = description.Replace("[item]", "<color=#FFE57AFF>" + item + "</color>");
            }

            name = name.Replace("[category]", "<color=#FFE57AFF>" + category + "</color>");
            description = description.Replace("[category]", "<color=#FFE57AFF>" + category + "</color>");




            double level = (1 + (heroData.rebirth * 100) + heroData.enhance);
            int formula = 0;
            int.TryParse(value.formula, out formula);
            level *= formula;

            description = description.Replace("[formula]", "<color=#85B8FFFF>" + level.ToStringABC() + "</color>");


            textName.text = name;
            textDescription.text = description;
        }

        gameObject.SetActive(value != null);
    }



}
