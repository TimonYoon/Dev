using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroSkillInfo : MonoBehaviour {

    /// <summary> 스킬 슬롯 풀 </summary>
    List<UIHeroInfoSkillSlot> skillSlotPool = new List<UIHeroInfoSkillSlot>();

    /// <summary> 스킬 설명 프리팹 </summary>
    [SerializeField]
    GameObject skillSlotPrefab;

    [SerializeField]
    GridLayoutGroup gridLayoutGroup;

    RectTransform rectContent;

    private void Awake()
    {
        rectContent = gridLayoutGroup.GetComponent<RectTransform>();
    }

    void Start()
    {
        //gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Show();
    }

    void Show()
    {
        for (int i = 0; i < skillSlotPool.Count; i++)
        {
            skillSlotPool[i].gameObject.SetActive(false);
        }

        if (UIHeroInfo.Instance == null || UIHeroInfo.Instance.heroData == null)
            return;

        HeroData heroData = UIHeroInfo.Instance.heroData;
        //스킬 목록 갱신
        if(heroData.skillDataList.Count >0)
        {
            for (int i = 0; i < heroData.skillDataList.Count; i++)
            {
                UIHeroInfoSkillSlot slot = CreateSkillSlot();
                slot.skillData = heroData.skillDataList[i];
            }
        }
        else if(heroData.baseData.territorySkillDataList.Count > 0)
        {
            for (int i = 0; i < heroData.baseData.territorySkillDataList.Count; i++)
            {
                UIHeroInfoSkillSlot slot = CreateSkillSlot();
                slot.heroData = heroData;
                slot.territorySkillData = heroData.baseData.territorySkillDataList[i];
            }
        }

        

        int count = heroData.skillDataList.Count;
        SizeControl(count);
    }

    /// <summary> 슬롯 생성 or 비활성화 슬롯 제공</summary>
    UIHeroInfoSkillSlot CreateSkillSlot()
    {
        UIHeroInfoSkillSlot slot = null;

        for (int i = 0; i < skillSlotPool.Count; i++)
        {
            if(skillSlotPool[i].gameObject.activeSelf == false)
            {
                slot = skillSlotPool[i];
            }
        }

        if(slot == null)
        {
            GameObject go = Instantiate(skillSlotPrefab);
            go.transform.SetParent(rectContent, false);
            slot = go.GetComponent<UIHeroInfoSkillSlot>();
            skillSlotPool.Add(slot);
        }

        return slot;

    }
    /// <summary> mail content size 조절 </summary>
    void SizeControl(float count)
    {
        rectContent.sizeDelta = new Vector2(0, (gridLayoutGroup.spacing.y + gridLayoutGroup.cellSize.y) * count);
    }

}
