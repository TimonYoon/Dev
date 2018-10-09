using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IMailReceive
{
    void Receive();
}

public class UIMail : MonoBehaviour {

    public static UIMail Instance;

    void Awake()
    {
        Instance = this;    
    }


    /// <summary> 메일 슬롯 프리팹 </summary>
    public GameObject mailSlotPrefab;

    /// <summary> 메일 스크롤 그리드 레이아웃 그룹 </summary>
    public GridLayoutGroup mailScrollViewContent; // 메일 슬롯프리팹 생성위치

    /// <summary> scroll content rect </summary>
    RectTransform mailContentRect;

    /// <summary> 메일 슬롯 풀 </summary>
    public List<UIMailSlot> mailSlotPool { get; private set; } // 모두 받기 기능을 사용할 때 필요한 리스트
    List<Transform> objectList = new List<Transform>();

    
    public Button allReceiveButton;

    List<UIMailSlotContainer> mailSlotContainerList = new List<UIMailSlotContainer>();

    List<string> mailIDList = new List<string>();
    /// <summary> 일괄 획득 버튼 누름</summary>
    public void OnClickAllReceiveButton()
    {

        for (int i = 0; i < mailSlotContainerList.Count; i++)
        {
            if (mailSlotContainerList[i].mailData.itemType == "money")
            {
                mailIDList.Add(mailSlotContainerList[i].mailData.mailID);
                mailSlotContainerList[i].Receive();
            }
        }

        int deleteMailCount = mailIDList.Count;

        allReceiveButton.interactable = false;

        StartCoroutine(PoolRemover(mailIDList));

        MailManager.Instance.AllReceiveItem();

        int count = mailScrollViewContent.transform.childCount - deleteMailCount;
        SizeControl(count);
    }

    IEnumerator PoolRemover(List<string> mailList)
    {
        for (int i = 0; i < mailList.Count; i++)
        {
            for (int j = 0; j < mailSlotPool.Count; j++)
            {
                if (mailSlotPool[j].mailID == mailList[i])
                {
                    GameObject go = mailSlotPool[j].gameObject;
                    mailSlotPool.RemoveAt(j);
                    Destroy(go);
                }
            }
        }
        mailIDList.Clear();
        yield return null;
    }

    /// <summary> 우편 첨부 획득 </summary>
    public void OnClickReceiveButton(string mailID)
    {
        UIMailSlotContainer slot = mailSlotContainerList.Find(x => x.mailData.mailID == mailID);

        if(slot.mailData.itemType == "buff")
        {
            OptionManager.Instance.ApplyBoostByMail(slot.mailData.itemID, System.Convert.ToSingle(slot.mailData.itemAmount));
        }

        MailManager.Instance.ReceiveItme(mailID);

        mailSlotContainerList.Remove(slot);
        mailSlotPool.RemoveAt(mailSlotPool.FindIndex(x => x.mailID == mailID));
        slot.Receive();//.SetActive(false);

        mailIDList.Add(mailID);
        StartCoroutine(PoolRemover(mailIDList));

        int count = mailSlotContainerList.Count;
        SizeControl(count);
    }
 
    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.SubMenu && SceneLobby.currentSubMenuState != SubMenuState.Mail)
            Close();
    }

    /// <summary> 컨테이너 프리팹 </summary>
    public GameObject contaninerPrefab;
    
    IEnumerator Start()
    {
        allReceiveButton.interactable = false;


        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        mailContentRect = mailScrollViewContent.GetComponent<RectTransform>();
        mailSlotContainerList = new List<UIMailSlotContainer>();
        mailSlotPool = new List<UIMailSlot>();

        // 콜백 등록
        MailManager.onInitMailData += OnInitMailData;

        // 메일 갱신
        StartCoroutine(MailManager.MailDataInitCoroutine());

    }

    /// <summary> 메일 UI</summary>
    void OnInitMailData(List<MailData> data = null)
    {
        if (data.Count == 0)
            return;

        int count = data.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(contaninerPrefab);
            go.transform.SetParent(mailScrollViewContent.transform, false);
            
            UIMailSlotContainer mailSlotContainer = go.GetComponent<UIMailSlotContainer>();
            mailSlotContainer.SlotInit(data[i]);
            mailSlotContainerList.Add(mailSlotContainer);
            
        }

        allReceiveButton.interactable = false;
        
        for (int i = 0; i < MailManager.Instance.mailDataList.Count; i++)
        {
            if (MailManager.Instance.mailDataList[i].itemType == "money")
            {
                allReceiveButton.interactable = true;
                break;
            }   
        }


        SizeControl(count);
    }


    /// <summary> 미사용 중인 slot을 리턴해준다. </summary>
    public UIMailSlot GetMailSlotFromPool()
    {
        UIMailSlot uIMailSlot = mailSlotPool.Find(x => !x.gameObject.activeSelf);

        if(uIMailSlot == null)
        {
            GameObject go = Instantiate(mailSlotPrefab);
            uIMailSlot = go.GetComponent<UIMailSlot>();
            mailSlotPool.Add(uIMailSlot);
        }
        return uIMailSlot;
    }

    

    
 


    //public delegate void ResetMailListCallback();
    ///// <summary> 메일 리스트 UI 리셋 버튼 누름 콜백 </summary>
    //public ResetMailListCallback onResetMailListCallback;


    ///// <summary> 메일 리스트를 갱신한다. </summary>
    //public void OnClickResetMailListButton()
    //{
    //    if (onResetMailListCallback != null)
    //        onResetMailListCallback();

    //    MailManager.Instance.MailDataInit();
    //}
    

    /// <summary> Scroll content size conrtrol </summary>
    void SizeControl(float count)
    {
        if (mailContentRect == null)
            mailContentRect = mailScrollViewContent.GetComponent<RectTransform>();
        
        double quotient = System.Math.Ceiling((double)count);

        float sizeDeltaY = (mailScrollViewContent.cellSize.y + mailScrollViewContent.spacing.y) * ((int)quotient);

        mailContentRect.sizeDelta = new Vector2(mailContentRect.sizeDelta.x, sizeDeltaY);
    }


    public void OnClickCloseButton()
    {
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        //Close();
    }
    void Close()
    {
        //MailManager.Instance.MailDataInit();
        MailManager.onInitMailData -= OnInitMailData;
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        SceneManager.UnloadSceneAsync("Mail");
    }
}
