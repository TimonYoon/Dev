using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 영웅 배치 때 영웅 능력치 표기 슬롯 </summary>
public class UIHeroAbilitySlot : MonoBehaviour {

    public string abilityID;

    public double abilityValue;

    [SerializeField]
    Text textName;

    [SerializeField]
    Text textValue;


    public void InitSlot(string name, double value, Color color)
    {
        gameObject.SetActive(true);
        abilityID = name;
        abilityValue = value;
        textName.text = name;
        textValue.text = value.ToStringABC();
        textValue.color = color;
    }

    private void OnDisable()
    {
        abilityValue = 0;
    }
}   
