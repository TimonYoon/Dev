using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class UIHeroEnhance : MonoBehaviour,IHeroInfoUpdateUI {

    [Header("강화")]
    public GameObject enhanceObject;
    public Button buttonEnhance;
    public Text textNeedEnhancePoint;
    public Image iconEnhance;

    public Button buttonTenEnhance;
    public Text textNeedTenEnhancePoint;
    public Image iconTenEnhance;

    public Button buttonMaxEnhance;
    public Text textNeedMaxEnhancePoint;
    public Image iconMaxEnhance;

    [Header("환생")]
    public GameObject rebirthObject;
    public Button buttonRebirth;
    public Text textNeedRebirthPoint;

    HeroData heroData { get { return UIHeroInfo.Instance.heroData; } }

    

    private void OnEnable()
    {
        Show();
    }

    void Show()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
       

        


        enhanceObject.gameObject.SetActive(!HeroEnhance.Instance.canRebirth);
        rebirthObject.gameObject.SetActive(HeroEnhance.Instance.canRebirth);

        //rebirthObject.gameObject.SetActive(heroData.rebirth > 0);

        //강화석 포인트 부족하면 강화 버튼 비활성
        buttonEnhance.interactable = HeroEnhance.Instance.canEnhance;
        //필요 강화 수치 표시
        textNeedEnhancePoint.text = HeroEnhance.Instance.needEnhancePoint.ToStringABC();

        buttonTenEnhance.interactable = HeroEnhance.Instance.canTenEnhance;
        textNeedTenEnhancePoint.text = HeroEnhance.Instance.needTenEnhancePoint.ToStringABC();

        buttonMaxEnhance.interactable = HeroEnhance.Instance.canMaxEnhance;
        textNeedMaxEnhancePoint.text = HeroEnhance.Instance.needMaxEnhancePoint.ToStringABC();

        //환생 관련 재화 포인트 부족하면 강화 버튼 비활성
        buttonRebirth.interactable = HeroEnhance.Instance.canRebirth;
        //환생시 필요 재료 수치 표시
        textNeedRebirthPoint.text = HeroEnhance.Instance.needRebirthPoint.ToStringABC();


        InitEnhancePoint();
    }


    [Header("강화시 요구 강화석 아이콘")]
    public Sprite enhancePointAIcon;
    public Sprite enhancePointBIcon;
    public Sprite enhancePointCIcon;
    public Sprite enhancePointDIcon;
    public Sprite enhancePointEIcon;


    void InitEnhancePoint()
    {
        Sprite sprite = null;
        ElementalType type = heroData.baseData.elementalType;

        switch (type)
        {
            case ElementalType.NotDefined:
                break;
            case ElementalType.Fire:
                sprite = enhancePointAIcon;
                break;
            case ElementalType.Water:
                sprite = enhancePointBIcon;
                break;
            case ElementalType.Earth:
                sprite = enhancePointCIcon;
                break;
            case ElementalType.Light:
                sprite = enhancePointDIcon;
                break;
            case ElementalType.Dark:
                sprite = enhancePointEIcon;
                break;
            default:
                break;
        }

        iconEnhance.sprite = sprite;
        iconTenEnhance.sprite = sprite;
        iconMaxEnhance.sprite = sprite;
    }

    /// <summary> 강화 버튼 눌렀을 때 </summary>
    public void OnPointerDownEnhance()
    {
        if (coroutineEnhanceUp != null)
            StopCoroutine(coroutineEnhanceUp);

        coroutineEnhanceUp = StartCoroutine(EnhanceUp());
    }

    /// <summary> 강화버튼을 땟다</summary>
    public void OnPointerUpEnhance()
    {
        if (coroutineEnhanceUp != null)
            StopCoroutine(coroutineEnhanceUp);
    }

    /// <summary> 환생 버튼 눌렀을 때 </summary>
    public void OnClickRebirth()
    {
        HeroEnhance.Instance.Rebirt();
        UpdateUI();
    }

    public void OnClickTenEnhance()
    {
        HeroEnhance.Instance.TenEnhance();
        UpdateUI();
    }


    public void OnClickMaxEnhance()
    {
        HeroEnhance.Instance.MaxEnhance();
        UpdateUI();
    }


    Coroutine coroutineEnhanceUp = null;
    IEnumerator EnhanceUp()
    {
        float interval = 0.5f;

        while (HeroEnhance.Instance.needEnhancePoint <= HeroEnhance.Instance.enhanceValue)
        {
            // 환생상태가 되면 강화 중지
            if (HeroEnhance.Instance.canRebirth || HeroEnhance.Instance.canEnhance == false)
            {
                yield break;
            }

            HeroEnhance.Instance.Enhance();
            UpdateUI();

            yield return new WaitForSecondsRealtime(interval);

            if (interval > 0.01f)
                interval *= 0.8f;

            if (interval < 0.01f)
                interval = 0.01f;
        }
        yield break;
    }

}
