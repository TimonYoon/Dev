using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 유물 선택지 구현용 클래스 </summary>
public class UIArtifactSelect : MonoBehaviour
{
    static UIArtifactSelect Instance;

    /// <summary> 유물 선택 관련 최상위 부모 오브젝트 </summary>
    [Header("유물 선택 오브젝트. 최상위 부모 오브젝트 지정")]
    public GameObject objArtifactSelect;

    /// <summary> 유물 선택 화면에서 보여지는 뒷배경. (어두운 텍스쳐) </summary>
    [Header("배경 오브젝트. 어두운 화면, 유물을 선택하세요 레이블")]
    public GameObject objBG;

    public GameObject objTitle;

    /// <summary> 연출 제어용 애니메이터 </summary>
    public Animator animator;

    /// <summary> 유물 선택 슬롯들. 0~2 까지 총 3개 </summary>
    [Header("유물 선택 슬롯들. 좌측, 중앙, 우측의 슬롯을 설정. 순서는 중요치 않음")]
    public List<UIArtifactPopUpSlot> artifactPopUpSlotList;

    public GameObject uiArtifactPopUpSlotPrefab;

    public List<Transform> spawnPivots;

    List<UIArtifactPopUpSlot> pool = new List<UIArtifactPopUpSlot>();

    UIArtifactPopUpSlot SpawnPopUpSlot(Transform pivot)
    {
        UIArtifactPopUpSlot artifactPopUpSlot = null;

        artifactPopUpSlot = pool.Find(x => x != null && !x.gameObject.activeSelf);

        //풀에 없으면 새로 생성
        if(!artifactPopUpSlot)
        {
            GameObject go = Instantiate(uiArtifactPopUpSlotPrefab, pivot);
            artifactPopUpSlot = go.GetComponent<UIArtifactPopUpSlot>();
            go.SetActive(false);
            pool.Add(artifactPopUpSlot);            
        }

        RectTransform r = artifactPopUpSlot.GetComponent<RectTransform>();

        artifactPopUpSlot.transform.SetParent(pivot);

        r.localPosition = Vector3.zero;
        r.localScale = Vector3.one;
        r.SetAsLastSibling();

        return artifactPopUpSlot;
    }
    

    //#############################################################################
    void Awake()
    {
        Instance = this;

        objArtifactSelect.SetActive(false);
        objBG.SetActive(false);
        objTitle.SetActive(false);
    }

    /// <summary> 유물 3가지 화면에 보여줌 </summary>
    static public void Show()
    {
        Instance.objArtifactSelect.SetActive(true);
        Instance.objBG.SetActive(true);
        Instance.objTitle.SetActive(true);

        for (int i = 0; i < Battle.currentBattleGroup.artifactController.showArtifactIDList.Count; i++)
        {
            string id = Battle.currentBattleGroup.artifactController.showArtifactIDList[i];
            UIArtifactPopUpSlot slot = Instance.SpawnPopUpSlot(Instance.spawnPivots[i]);

            slot.SlotInit(GameDataManager.ArtifactBaseDataDic[id]);
        }
    }

    public delegate void UIArtifactPopUpSlotDeleget(UIArtifactPopUpSlot slot);
    static public UIArtifactPopUpSlotDeleget onSelected;

    /// <summary> 3가지 선택지 중에 하나를 선택했을 때 </summary>
    static public void Select(UIArtifactPopUpSlot selectedSlot)
    {
        Instance.objBG.SetActive(false);
        Instance.objTitle.SetActive(false);

        if (onSelected != null)
            onSelected(selectedSlot);
    }

    /// <summary> 유물 선택 과정 닫기 </summary>
    static public void Close()
    {
        Instance.objBG.SetActive(false);
        Instance.objTitle.SetActive(false);
    }


}
