using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroTrainingSlot : MonoBehaviour {

    public string heroID
    {
        get { return heroTrainingData.heroID; }
    }
    public int slotNumber;

    HeroTrainingData _heroTrainingData = null;
    public HeroTrainingData heroTrainingData
    {
        get { return _heroTrainingData; }
        set
        {
            _heroTrainingData = value;

            if(value == null)
            {
                SetOffHeroSlot();
            }
        }
    }

    [SerializeField]
    GameObject heroTrainingInfoPanel;

    [SerializeField]
    Image heroImage;

    [SerializeField]
    Text textHeroName;

    [SerializeField]
    Image progressBar;

    [SerializeField]
    Text remainTimeText;

    [SerializeField]
    Text textTrainingStatName;

    [SerializeField]
    Text textCurrentTrainingValue;

    //[SerializeField]
    //Text textMaxTrainingValue;

    [SerializeField]
    Button buttonTrainingComplete;

    [SerializeField]
    Button buttonInstantComplete;

    float remainTime = 0f;
    float progressValue = 0f;
    float progressMaxValue;

    //영웅 교체 가능 여부
    bool isChangable = false;

    private void Awake()
    {
        progressMaxValue = progressBar.rectTransform.sizeDelta.x;
    }

    public void InitTrainingSlot()
    {
        heroTrainingInfoPanel.SetActive(true);
        remainTime = heroTrainingData.remainTime;
        progressValue = (heroTrainingData.trainingTime - heroTrainingData.remainTime) / heroTrainingData.trainingTime;

        InitUI();
    }

    void InitUI()
    {
        HeroData heroData = HeroManager.heroDataDic[heroTrainingData.heroID];
        textTrainingStatName.text = heroData.trainingDataList[heroTrainingData.trainingStat].paramName;
        textCurrentTrainingValue.text = (heroData.trainingDataList[heroTrainingData.trainingStat].training + 1) + "단계";
        //textMaxTrainingValue.text = heroData.trainingMax.ToString();
        
        remainTimeText.text = remainTime.ToStringTimeHMS();

        if (heroTrainingData.isTrainingMax == true)
        {
            buttonTrainingComplete.interactable = true;
            buttonInstantComplete.interactable = false;
        }   
        else
        {
            buttonTrainingComplete.interactable = false;
            buttonInstantComplete.interactable = true;
            isChangable = true;
        }
            

        textHeroName.text = heroData.heroName;
        AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.baseData.image);
    }

    void SetOffHeroSlot()
    {
        heroTrainingInfoPanel.SetActive(false);
    }

    public void OnClickSelectTrainingHero()
    {
        UIHeroTraining.Instance.slotNumber = slotNumber;
        UIHeroTraining.Instance.ShowSelectPanel();
    }

    public void OnClickChangeTrainingHero()
    {
        if (isChangable == true)
        {
            UIHeroTraining.Instance.slotNumber = slotNumber;
            UIHeroTraining.Instance.ShowSelectPanel();
        }
        else
        {
            UIPopupManager.ShowInstantPopup("훈련 완료전까지 영웅을 교체할 수 없습니다");
        }
    }

    private void Update()
    {
        if (heroTrainingData == null)
            return;
        else
            HeroManager.heroDataDic[heroTrainingData.heroID].isTraining = true;

        if (heroTrainingData.isTrainingMax == true)
        {
            buttonTrainingComplete.interactable = true;
            buttonInstantComplete.interactable = false;
            isChangable = false;
        }
            

        if(heroTrainingData.isTrainingStart == true)
        {
            isChangable = false;

            progressValue = (heroTrainingData.trainingTime - heroTrainingData.remainTime) / heroTrainingData.trainingTime;
            float progress = progressValue * progressMaxValue;
            progressBar.rectTransform.sizeDelta = new Vector2(progress, progressBar.rectTransform.sizeDelta.y);
            float remain = heroTrainingData.remainTime;
            remainTimeText.text = remain.ToStringTimeHMS();
        }
    }

    public void OnClickTrainingCompleteButton()
    {
        HeroTrainingManager.Instance.Training(slotNumber);
        heroTrainingData.isTrainingMax = false;
        buttonTrainingComplete.interactable = false;
        isChangable = true;
        HeroManager.heroDataDic[heroTrainingData.heroID].isTraining = false;

        HeroTrainingManager.Instance.heroTrainingDataList.Remove(heroTrainingData);
        heroTrainingData = null;
    }

    
}
