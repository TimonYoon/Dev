using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayDungeonMonsterSlot : MonoBehaviour {

    public GameObject heroSlotPanel;

    public Image imageHero;

    public GameObject monsterCountPanel;

    public Text textMonsterCount;

    public Image gradeFrame;

    public List<Sprite> gradeSpriteList = new List<Sprite>();

    public void InitSlot(HeroBaseData heroData = null, int monstarCount = 0)
    {
        heroSlotPanel.SetActive(heroData != null);

        if (heroData != null)
        {
            AssetLoader.AssignImage(imageHero, "sprite/hero", "Atlas_HeroImage", heroData.image);

            monsterCountPanel.SetActive(false);
            if (monstarCount > 0)
            {
                monsterCountPanel.SetActive(true);
                textMonsterCount.text = "X " + monstarCount.ToString();
            }
            

            Sprite frame = null;
            for (int i = 0; i < gradeSpriteList.Count; i++)
            {
                frame = gradeSpriteList[i];
                if (i == (heroData.grade - 1))
                    break;

            }
            if (frame != null)
                gradeFrame.sprite = frame;
        }

    }
}
