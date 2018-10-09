using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIMoneySlot : MonoBehaviour
{
    [Header("재화 아이콘")]
    public Image imageIcon;

    [Header("재화 값이 표시되는 곳")]
    public Text textValue;

    [Header("재화 종류")]
    public MoneyType moneyType = MoneyType.none;

    [Header("유저가 소유하고 있는 재화임? 기면 자동 갱신")]
    public bool isUserMoney = false;

    void Start()
    {
        if (!MoneyManager.isInitialized)
            return;
            
        if (isUserMoney)
        {
            Init(MoneyManager.GetMoney(moneyType));

            MoneyManager.RegisterOnChangedValueCallback(moneyType, OnChangedMoneyValue);

            UpdateValue();
        }       
    }

    MoneyManager.Money money;

    /// <summary> 재화 정보 초기화 </summary>
    /// <param name="money"></param>
    public void Init(MoneyManager.Money money)
    {
        this.money = money;

        if (money == null)
            return;

        UpdateValue();

        //Debug.Log (GameDataManager.moneyBaseDataDic.ContainsKey(money.id)) ;

        //Todo: 아이콘 세팅
    }

    void OnChangedMoneyValue()
    {
        UpdateValue();
    }

    void UpdateValue()
    {
        if (money == null)
            return;

        if (textValue)
        {
            if(money.type == MoneyType.gold 
                || moneyType == MoneyType.enhancePointA
                || moneyType == MoneyType.enhancePointB
                || moneyType == MoneyType.enhancePointC
                || moneyType == MoneyType.enhancePointD
                || moneyType == MoneyType.enhancePointE)
            {
                double d = money.value;

                textValue.text = d.ToStringABC();
            }
            else
            {
                double i = money.value;
                textValue.text = i.ToStringComma();
            }
                
        }
            
    }
}
