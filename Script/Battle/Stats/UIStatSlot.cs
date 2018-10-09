using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIStatSlot : MonoBehaviour
{

    public Image imageIcon;

    public Text textName;

    public Text textValue;

    public int index;

    Stat stat;

    public void Init(Stat stat)
    {
        this.stat = stat;

        textName.text = stat.baseData.name;

        stat.onChangedValue += UpdateValue;

        index = stat.baseData.index;

        //transform.SetSiblingIndex(stat.baseData.index);

        UpdateValue();
    }

    void UpdateValue()
    {
        if(stat.baseData.expressionType == StatBaseData.ExpressionType.Value)
        {
            double value = stat.value;
            textValue.text = value.ToStringABC();
        }   
        else if(stat.baseData.expressionType == StatBaseData.ExpressionType.Percent)
            textValue.text = (stat.value * 0.01).ToString("0.00") + "%";
    }
}
