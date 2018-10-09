using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHeroPromote : MonoBehaviour {

    public static UIHeroPromote Instance;
    
    string beforeImageName = string.Empty;
    string afterImageName = string.Empty;
    string beforeHeroName = string.Empty;
    string afterHeroName = string.Empty;

    int beforeGrade;
    int afterGrade;

    [Header("등급별 표시")]
    public Image heroImage;
    public GameObject[] gradeArray;
    public Image bg;
    public Color[] colorArray;
    public bool showGradeColor = true;
    public Text textheroName;

    [SerializeField]
    Animator animator;

    [SerializeField]
    GameObject advancementAniPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(HeroData heroData)
    {
        beforeImageName = heroData.baseData.image;
        afterImageName = GameDataManager.heroBaseDataDic[heroData.baseData.promoteID].image;

        beforeHeroName = heroData.heroName;
        afterHeroName = GameDataManager.heroBaseDataDic[heroData.baseData.promoteID].name;

        beforeGrade = heroData.heroGrade;
        afterGrade = GameDataManager.heroBaseDataDic[heroData.baseData.promoteID].grade;
    }

    // 영웅 slot에 등급 만큼 별 이미지 표시
    void ShowGrade(int grade)
    {
        foreach (var i in gradeArray)
        {
            i.SetActive(false);
        }
        if (grade < 5 && grade > 0)
            gradeArray[grade - 1].SetActive(true);
        else
            gradeArray[0].SetActive(true);

        if (showGradeColor)
        {
            if (grade < 1 || grade > colorArray.Length)
                bg.color = Color.gray;

            bg.color = colorArray[grade - 1];
        }
    }

    public void InitImage(string imageName, Sprite sprite = null)
    {
        if (sprite == null)
            AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", imageName);
        else
            heroImage.sprite = sprite;
    }

    public void ShowBeforeHero()
    {
        ShowGrade(beforeGrade);
        InitImage(beforeImageName);
        textheroName.text = beforeHeroName;
    }

    public void ShowAfterHero()
    {
        ShowGrade(afterGrade);
        InitImage(afterImageName);
        textheroName.text = afterHeroName;
    }


    public IEnumerator ShowPromoteAnimation()
    {
        ShowBeforeHero();

        advancementAniPanel.SetActive(true);

        while (animator.GetCurrentAnimatorStateInfo(0).IsTag("Standby") == false)
            yield return null;
       
        animator.SetTrigger("On");

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        ShowAfterHero();
    }

    public void OnClickClosePromoteAniPanel()
    {
        advancementAniPanel.SetActive(false);
    }
}
