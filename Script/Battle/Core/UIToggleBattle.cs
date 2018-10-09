using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleBattle : MonoBehaviour {

    public int index;
    public string battleGroupID;
    
    public enum ClickActionType
    {
        ShowBattle,
        ShowPreparation
    }

    ClickActionType _clickActionType = ClickActionType.ShowBattle;
    public ClickActionType clickActionType
    {
        get { return _clickActionType; }
        set
        {
            if (_clickActionType == value)
                return;

            _clickActionType = value;

            toggledObjectShowBattle.SetActive(value == ClickActionType.ShowBattle);
            toggledObjectShowPreparation.SetActive(value == ClickActionType.ShowPreparation);
        }
    }

    public GameObject toggledObjectShowBattle;
    public GameObject toggledObjectShowPreparation;

    //###########################################################################
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        battleGroupID = string.Empty;
    }

    void Start()
    {
        Battle.onChangedBattleGroup -= OnChangedBattleGroup;
        Battle.battleGroupList.onAdd -= OnAddBattleGroupList;
        Battle.battleGroupList.onRemovePost -= OnRemovedBattleGroupList;

        Battle.onChangedBattleGroup += OnChangedBattleGroup;
        Battle.battleGroupList.onAdd += OnAddBattleGroupList;
        Battle.battleGroupList.onRemovePost += OnRemovedBattleGroupList;

        UpdateBattleGroupID();
    }

    //void OnDisable()
    //{
    //    Battle.onChangedBattleGroup -= OnChangedBattleGroup;
    //    Battle.battleGroupList.onAdd -= OnAddBattleGroupList;
    //    Battle.battleGroupList.onRemovePost -= OnRemovedBattleGroupList;
    //}

    void OnAddBattleGroupList(BattleGroup battleGroup)
    {
        UpdateBattleGroupID();
    }

    void OnRemovedBattleGroupList(BattleGroup battleGroup)
    {
        UpdateBattleGroupID();
    }

    void UpdateBattleGroupID()
    {
        if(index > Battle.battleGroupList.Count)
        {
            gameObject.SetActive(false);
            battleGroupID = string.Empty;
            return;
        }
        else if(index < Battle.battleGroupList.Count)
        {
            gameObject.SetActive(true);
            battleGroupID = Battle.battleGroupList[index].battleType;
            clickActionType = ClickActionType.ShowBattle;
        }
        else
        {
            gameObject.SetActive(true);
            for (int i = 0; i < Battle.Instance.battleIDList.Count; i++)
            {
                BattleGroup b = Battle.battleGroupList.Find(x => x.battleType == Battle.Instance.battleIDList[i]);
                if (b == null)
                {
                    UIToggleBattle t = UIBattle.Instance.uiToggleBattleList.Find(x => x.battleGroupID == Battle.Instance.battleIDList[i]);
                    if (t == null)
                    {
                        battleGroupID = Battle.Instance.battleIDList[i];
                        break;
                    }
                }
            }

            clickActionType = ClickActionType.ShowPreparation;
        }   
    }


    void OnChangedBattleGroup(BattleGroup battleGroup)
    {
        //int i = Battle.battleGroupList.FindIndex(x => x == Battle.currentBattleGroup);

        //if(i == index)
        if (Battle.currentBattleGroup.battleType == battleGroupID)
        {
            if (!toggle.isOn)
                toggle.isOn = true;
        }   
    }

    public Toggle toggle;


	
    public void OnToggleValueChanged(Toggle toggle)
    {
        if (!toggle.isOn)
            return;

        //int a = Battle.battleGroupList.FindIndex(x => x == Battle.currentBattleGroup);

        //if (a == index)
        if (Battle.currentBattleGroup.battleType == battleGroupID)
            return;

        //string battleID = string.Empty;// battleGroupID;
        //if(index < Battle.battleGroupList.Count)
        //    battleID = Battle.battleGroupList[index].id;
        //else
        //{
        //    for(int i = 0; i < Battle.Instance.battleIDList.Count; i++)
        //    {
        //        BattleGroup b = Battle.battleGroupList.Find(x => x.id == Battle.Instance.battleIDList[i]);
        //        if(b == null)
        //        {
        //            battleID = Battle.Instance.battleIDList[i];
        //            break;
        //        }
        //    }
        //}

        Battle.ShowBattle(battleGroupID);
    }
}
