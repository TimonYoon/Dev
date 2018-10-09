using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvPHeroSlot : MonoBehaviour {

    public Text textRank;
    public Image imageHero;
    public Text textHeroName;
    public Image imageGradePanel;
    public Image imageGradeFrame;
    public List<Sprite> heroGradeFrameList = new List<Sprite>();
    public Text textEnhance;
    public Text textRebirth;
    public Text textDamge;
    public Image fillAmount;
    Color gradeColor;
    List<string> colorHashList = new List<string>();

    public BattleHero battleHero { get; private set; }
    void Awake()
    {
        colorHashList.Add("#767676FF");
        colorHashList.Add("#77A929FF");
        colorHashList.Add("#40899CFF");
        colorHashList.Add("#FF4D4DFF");
    }

    private void OnDisable()
    {
        battleHero.onChangedCumulativeDamage -= OnChangedCumulativeDamage;
        battleHero = null;
    }
    public void InitSlot(int rank, BattleHero _battleHero)
    {
        battleHero = _battleHero;
        HeroData heroData = battleHero.heroData;
        battleHero.onChangedCumulativeDamage += OnChangedCumulativeDamage;

        AssetLoader.AssignImage(imageHero, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);
        textHeroName.text = heroData.heroName;

        Sprite sprite = null;
        for (int i = 0; i < heroGradeFrameList.Count; i++)
        {
            if (i == (heroData.heroGrade - 1))
            {
                ColorUtility.TryParseHtmlString(colorHashList[i], out gradeColor);
                sprite = heroGradeFrameList[i];
                break;
            }
        }
        if (sprite != null)
        {
            imageGradeFrame.sprite = sprite;
            imageGradePanel.color = gradeColor;
        }
           
        string enhance = heroData.enhance == 0 ? "" : heroData.enhance.ToString();
        string rebirth = heroData.rebirth == 0 ? "" : heroData.rebirth.ToString();
        textEnhance.text = enhance;
        textRebirth.text = rebirth;

        textRank.text = rank + "위";
        textDamge.text = "0";
        fillAmount.fillAmount = 0;
    }

    void OnChangedCumulativeDamage()
    {
        double cumulativeDamage = battleHero.cumulativeDamage;
        textDamge.text = cumulativeDamage.ToStringABC();
    }

    public void UpdateSlot(int rank, float value)
    {
        textRank.text = rank + "위";
        fillAmount.fillAmount = value;
    }

   
}
