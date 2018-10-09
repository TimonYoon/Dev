using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

//정렬 타입
public enum HeroSortingType
{
    Auto,
    AcquiredTime,   //습득순. 오래된게 위로
    AcquiredTimeDesc,   //최근 습득순
    Name,
    NameDesc,
    Grade,
    GradeDesc,
}

public class UISortHeroSlot : MonoBehaviour {
    
    static public SimpleDelegate onSort;
 
    string placeID;

    public List<UIHeroSlotContainer> heroContainerList;

    public HeroSortingType heroSortType = HeroSortingType.Auto;

    HeroSlotState currentState = HeroSlotState.Default;
    /// <summary> 인벤토리 영웅 정렬 함수 </summary>
    public void SortHeroList(HeroSortingType sortingType = HeroSortingType.Auto, HeroSlotState state = HeroSlotState.Inventory, string _placeId = null)
    {
        if (coroutineSort != null)
            return;

        UIHeroInventory.Instance.currentSortType = sortingType;
        currentState = state;
        placeID = UIDeployHeroInfo.Instance.currentPlaceID;
        coroutineSort = StartCoroutine(SortHeroListA(sortingType, state));
    }

    Coroutine coroutineSort = null;



    IEnumerator SortHeroListA(HeroSortingType sortingType = HeroSortingType.Auto, HeroSlotState state = HeroSlotState.Inventory)
    {
        heroSortType = sortingType;

        switch (heroSortType)
        {
            case HeroSortingType.AcquiredTime:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateAcquiredTime);
                break;

            case HeroSortingType.AcquiredTimeDesc:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegatAcquiredTimeDesc);
                break;

            case HeroSortingType.Name:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateName);
                break;

            case HeroSortingType.NameDesc:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateNameDesc);
                break;

            case HeroSortingType.Grade:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateGrade);
                break;

            case HeroSortingType.GradeDesc:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateGradeDesc);
                break;
            default:
                UIHeroInventory.heroSlotContainerList.Sort(SortDelegateAuto);
                break;
        }
        

        for (int i = 0; i < heroContainerList.Count; i++)
        {
            heroContainerList[i].transform.SetSiblingIndex(i);
        }

        //오브젝트 정렬 위해 한 프레임 기다려야 함
        yield return null;

        if (onSort != null)
            onSort();

        coroutineSort = null;
    }
    
    /// <summary> 정렬을 위해 데이터 비교하기 위한 함수 </summary>
    int SortDelegateAuto(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if(currentState == HeroSlotState.Training)
        {
            if (heroDataA.isTraining && !heroDataB.isTraining)
                return -1;
            else if (!heroDataA.isTraining && heroDataB.isTraining)
                return 1;
        }


        // 1차 정렬 새로 얻은 영웅을 위쪽으로 정렬  
        //########################################################################
        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        
        //2차 정렬. 무조건 전투 참여중인애가 제일 위로
        //########################################################################
        bool isUsingA = !string.IsNullOrEmpty(heroDataA.placeID) || !string.IsNullOrEmpty(heroDataA.battleGroupID);
        bool isUsingB = !string.IsNullOrEmpty(heroDataB.placeID) || !string.IsNullOrEmpty(heroDataB.battleGroupID);


        if (currentState == HeroSlotState.Territory && !string.IsNullOrEmpty(placeID))
        {
            if (heroDataA == null)
            {
                if (placeID == heroDataA.placeID && placeID != heroDataB.placeID)
                    return -1;
                else if (placeID != heroDataA.placeID && placeID == heroDataB.placeID)
                    return 1;
            }
        }


        if (isUsingA && !isUsingB)
            return -1;
        else if (!isUsingA && isUsingB)
            return 1;

        //3차 강화 높은 순 정렬
        //#########################################################################

        int enhanceA, enhanceB;

        enhanceA = (heroDataA.rebirth * 100) + heroDataA.enhance;
        enhanceB = (heroDataB.rebirth * 100) + heroDataB.enhance;

        if (enhanceA > enhanceB)
            return -1;
        else if (enhanceA < enhanceB)
            return 1;


        //4차 정렬 원하는 기준에 따른 정렬
        //##########################################################################

        int result;

        int gradeA = heroDataA.heroGrade;
        int gradeB = heroDataB.heroGrade;

        result = gradeB.CompareTo(gradeA);
        if(result == 0)
        {
            result = heroDataA.heroName.CompareTo(heroDataB.heroName);
        }

        return result;

    }

    int SortDelegateAcquiredTime(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int idA, idB;

        int.TryParse(heroDataA.id, out idA);
        int.TryParse(heroDataB.id, out idB);
        return idA.CompareTo(idB);
    }
    int SortDelegatAcquiredTimeDesc(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int idA, idB;

        int.TryParse(heroDataA.id, out idA);
        int.TryParse(heroDataB.id, out idB);
        return idB.CompareTo(idA);
    }
    int SortDelegateName(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int result;
        result = heroDataA.heroName.CompareTo(heroDataB.heroName);
        if(result == 0)
        {

            int enhanceA, enhanceB;

            enhanceA = (heroDataA.rebirth * 100) + heroDataA.enhance;
            enhanceB = (heroDataB.rebirth * 100) + heroDataB.enhance;

            if (enhanceA > enhanceB)
                return -1;
            else if (enhanceA < enhanceB)
                return 1;
            
        }

        return result;
    }
    int SortDelegateNameDesc(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int result;
        result = heroDataB.heroName.CompareTo(heroDataA.heroName);
        if (result == 0)
        {

            int enhanceA, enhanceB;

            enhanceA = (heroDataA.rebirth * 100) + heroDataA.enhance;
            enhanceB = (heroDataB.rebirth * 100) + heroDataB.enhance;

            if (enhanceA > enhanceB)
                return -1;
            else if (enhanceA < enhanceB)
                return 1;
        }

        return result;
    }
    int SortDelegateGrade(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int result;
        int gradeA = heroDataA.heroGrade;
        int gradeB = heroDataB.heroGrade;
        result = gradeA.CompareTo(gradeB);
        if (result == 0)
        {

            int enhanceA, enhanceB;

            enhanceA = (heroDataA.rebirth * 100) + heroDataA.enhance;
            enhanceB = (heroDataB.rebirth * 100) + heroDataB.enhance;

            if (enhanceA > enhanceB)
                return -1;
            else if (enhanceA < enhanceB)
                return 1;
        }

        if(result == 0)
        {
            result = heroDataA.heroName.CompareTo(heroDataB.heroName);
        }

        return result;
    }
    int SortDelegateGradeDesc(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];

        if (heroDataA.isChecked && !heroDataB.isChecked)
            return 1;
        else if (!heroDataA.isChecked && heroDataB.isChecked)
            return -1;

        int result;
        int gradeA = heroDataA.heroGrade;
        int gradeB = heroDataB.heroGrade;
        result = gradeB.CompareTo(gradeA);
        if (result == 0)
        {

            int enhanceA, enhanceB;

            enhanceA = (heroDataA.rebirth * 100) + heroDataA.enhance;
            enhanceB = (heroDataB.rebirth * 100) + heroDataB.enhance;

            if (enhanceA > enhanceB)
                return -1;
            else if (enhanceA < enhanceB)
                return 1;
        }

        if (result == 0)
        {
            result = heroDataA.heroName.CompareTo(heroDataB.heroName);
        }

        return result;
    }
}


