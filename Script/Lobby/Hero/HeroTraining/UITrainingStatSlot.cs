using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrainingStatSlot : MonoBehaviour {
    
    [SerializeField]
    Text textTrainingValue;

    [SerializeField]
    GameObject selectJewel;

    [SerializeField]
    Toggle toggle; // 

    [SerializeField]
    int statNumber;

    [SerializeField]
    Text textTrainingPoint;

    Color nonColor;

    Color upColor = Color.green;
    

    private void OnEnable()
    {
        nonColor = textTrainingValue.color;
    }

    private void OnDisable()
    {
        if (selectJewel.activeSelf)
        {
            toggle.isOn = false;
            selectJewel.SetActive(false);
        }
            
    }

    public void InitSlot()
    {
        if(!string.IsNullOrEmpty(UIHeroTraining.Instance.selectedTrainingHeroID))
        {
            selectJewel.SetActive(false);
            //toggle.isOn = false;
            textTrainingValue.color = nonColor;
            if (HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList.Count > statNumber)
                textTrainingValue.text = HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList[statNumber].training + "단계";
        }
    }

    /// <summary> 해당 슬롯 클릭했을 때 </summary>
    public void OnClickSelectButton()
    {
        if (string.IsNullOrEmpty(UIHeroTraining.Instance.selectedTrainingHeroID))
            return;

        if (HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList.Count <= statNumber)
            return;

        int count = 0;
        for (int i = 0; i < HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList.Count; i++)
        {
            count += HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList[i].training;
        }


        if (count < HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingMax)
        {
            if (toggle.isOn)
            {
                

                textTrainingPoint.text = (HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingMax - count - 1).ToString();


                int num = HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList[statNumber].training;
                num++;
                textTrainingValue.color = upColor;
                textTrainingValue.text = num + "단계";

                selectJewel.SetActive(true);

                if (onClickStatSelectButton != null)
                    onClickStatSelectButton(statNumber);
            }
            else
            {
                textTrainingValue.color = nonColor;
                textTrainingValue.text = HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList[statNumber].training + "단계";

                selectJewel.SetActive(false);
            }
        }
        else
        {
            UIPopupManager.ShowOKPopup("훈련 불가", "남은 훈련치가 없습니다", null);
            textTrainingValue.color = nonColor;
            textTrainingValue.text = HeroManager.heroDataDic[UIHeroTraining.Instance.selectedTrainingHeroID].trainingDataList[statNumber].training + "단계";

            selectJewel.SetActive(false);
        }
    }

    public delegate void OnClickStatSelectButton(int statNumber);
    public static OnClickStatSelectButton onClickStatSelectButton;

    ///// <summary> 토글 변경 </summary>
    //public void IsOn()
    //{
    //    if (toggle.isOn)
    //        toggle.isOn = false;
    //    else if (!toggle.isOn)
    //        toggle.isOn = true;
    //}
}
