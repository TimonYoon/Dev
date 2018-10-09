using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIKingdomInfo : MonoBehaviour {

    Canvas territoryCanvas;

    //public GameObject territoryPanel;

    public GameObject productionSlotPrefab;

    public RectTransform productionSlotParent;

    public Image imageTerritoryExpProgress;

    /// <summary> 왕국 레벨 </summary>
    public Text textLevel;

    /// <summary> 왕국?군주?왕? 이름. 실상은 유저 닉네임 </summary>
    public Text textNickname;

    /// <summary> 왕국 레벨업 버튼 </summary>
    public Button buttonLevelUp;

    GridLayoutGroup gridLayoutGroup;

    List<UIProductionSlot> productionSlotList = new List<UIProductionSlot>();
      

    private void Awake()
    {
        territoryCanvas = GetComponent<Canvas>();
        territoryCanvas.enabled = false;
        //territoryPanel.SetActive(false);
        gridLayoutGroup = productionSlotParent.GetComponent<GridLayoutGroup>();
    }
  
    IEnumerator Start()
    {

        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        while (!TerritoryManager.Instance.isInitialized)
            yield return null;

        //productionSlotList = new List<UIProductionSlot>(productionSlotParent.GetComponentsInChildren<UIProductionSlot>());

        while (!ProductManager.Instance.isInitialized)
            yield return null;

        for (int i = 0; i < ProductManager.Instance.productionLineDataList.Count; i++)
        {
            UIProductionSlot slot = CreateProductionSlot();
            slot.InitSlot(ProductManager.Instance.productionLineDataList[i]);
            //if(i == 0)
            //{
            //    slot.ApplyProduct(ProductManager.Instance.productList.Find(x => x.id == "food_001"));
            //}
            //else
            //    slot.ApplyProduct();
        }


        while (KingdomManagement.Storage.isInitialized == false)
            yield return null;

        //productionSlotList[0]

        SizeControl(productionSlotList.Count);
        

        while (!User.Instance)
            yield return null;
        
        User.Instance.onChangedUserData += OnChangedUserData;
        User.onChangedExp += OnChangedUserExp;
        User.onChangedLevel += OnChangedUserLevel;

        //왕국 기본 정보(레벨, 만족도, 닉네임?) 표시
        UpdateUserInfo();

        //만족도 게이지 갱신
        UpdateExpGauge();
    }

    /// <summary> 유저 경험치가 변경되었을 때. 경험치는 시민 만족도로 쓰고 있음 </summary>
    void OnChangedUserExp()
    {
        UpdateExpGauge();
    }

    /// <summary> 유저 레벨이 변경되었을 때 </summary>
    void OnChangedUserLevel()
    {
        textLevel.text = User.Instance.userLevel.ToString();
    }

    /// <summary> 왕국 레벨, 경험치(시민 만족도) 내용 갱신 </summary>
    void UpdateUserInfo()
    {
        //Debug.Log("유저 레벨 텍스트 변경");
        textLevel.text = User.Instance.userLevel.ToString();
        textNickname.text = "왕국이름 정하러 가기";
        if (!string.IsNullOrEmpty(User.Instance.nickname))
        {
            textNickname.text = User.Instance.nickname;
        }
    }

    public void OnClickNickName()
    {
        SceneLobby.Instance.ShowShop(ShopType.Buff);
    }
    /// <summary> 경험치(시민 만족도) 게이지 갱신. 레벨업 버튼 활성/비활성도 여기서 함 </summary>
    void UpdateExpGauge()
    {
        float a = (float)(User.currentExp / User.requiredExp);

        imageTerritoryExpProgress.fillAmount = a;

        buttonLevelUp.gameObject.SetActive(a >= 1f);
    }
    
    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Territory)
        {
            OnClickCloseButton();
            return;
        }
        Show();
    }

    void Show()
    {
        territoryCanvas.enabled = true;
        //territoryPanel.SetActive(true);
    }

    void OnChangedUserData()
    {
        UpdateUserInfo();
    }      

    public void OnClickCloseButton()
    {
        territoryCanvas.enabled = false;
        //territoryPanel.SetActive(false);
    }

    
    UIProductionSlot CreateProductionSlot()
    {
        UIProductionSlot slot = null;

        for (int i = 0; i < productionSlotList.Count; i++)
        {
            if(productionSlotList[i].gameObject.activeSelf == false)
            {
                slot = productionSlotList[i];
                break;
            }
        }

        if(slot == null)
        {
            GameObject go = Instantiate(productionSlotPrefab, productionSlotParent, false);
            slot = go.GetComponent<UIProductionSlot>();
            productionSlotList.Add(slot);
        }

        return slot;
    }

    void SizeControl(float count)
    {
        productionSlotParent.sizeDelta = new Vector2(0, (gridLayoutGroup.spacing.y + gridLayoutGroup.cellSize.y) * count);
    }

    public void OnClickLevelUp()
    {
        User.LevelUp(LevelUpEffect);
    }

    public GameObject kingdomLevelupEffect;
    void LevelUpEffect(bool result)
    {
        if(result)
        {
            kingdomLevelupEffect.SetActive(false);
            kingdomLevelupEffect.SetActive(true);
            //Todo: 레벨업 연출
        }
    }
}


