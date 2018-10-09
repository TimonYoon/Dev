using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 전투중 획득한 유물들을 화면에 표현하는 클래스 </summary>
public class UIArtifact : MonoBehaviour {
    static public UIArtifact Instance;

    ArtifactController artifactController
    {
        get
        {
            if (!Battle.Instance)
                return null;

            if (!Battle.currentBattleGroup)
                return null;

            if (!Battle.currentBattleGroup.artifactController)
                return null;

            return Battle.currentBattleGroup.artifactController;
        }
    }

    double artifactPoint
    {
        get
        {
            if (!artifactController)
                return 0;

            return artifactController.artifactPoint; 
        }
    }

    //[SerializeField]
    public Text artifactPointText;

    public Text textCost;
         

    public GameObject artifactSlotPrefab;

    public GameObject _lootObjectPrefab;

    static public GameObject lootObjectPrefab
    {
        get { return Instance._lootObjectPrefab; }
    }

    //public GridLayoutGroup content;

    public LayoutGroup layoutGroup;

    public ScrollRect scrollRect;

    public Button buttonBuyArtifact;

    [System.NonSerialized]
    static public List<UIArtifactSlot> slotPool = new List<UIArtifactSlot>();

    void Awake()
    {
        Instance = this;

        //Battle.onChangedCurrentBattleGroupID += OnChangedCurrentBattleGroupID;
        //Battle.onChangedBattleGroupList += OnChangedBattleGroupList;
        Battle.onChangedBattleGroup += OnChangedBattleGroup;

    }

    void Start()
    {

        Battle.battleGroupList.onAdd += OnAddBattleGroup;
        Battle.battleGroupList.onRemove += OnRemoveBattleGroup;
    }
    
    //새로운 배틀그룹이 추가될 때
    void OnAddBattleGroup(BattleGroup battleGroup)
    {
        battleGroup.artifactController.onChangedArtifactList += OnArtifactAdd;
        battleGroup.artifactController.onChangedArtifactPoint += OnChangedArtifactPoint;
    }

    //기존 배틀그룹이 제거될 때
    void OnRemoveBattleGroup(BattleGroup battleGroup)
    {
        battleGroup.artifactController.onChangedArtifactList -= OnArtifactAdd;
        battleGroup.artifactController.onChangedArtifactPoint -= OnChangedArtifactPoint;
    }

    void OnChangedBattleGroup(BattleGroup _battleGroup)
    {
        if (this == null)
            return;

        UpdateArtifactPointText();

        StartCoroutine(UIInit());
    }
    
    public void OnClickArtifactButton()
    {
        if (artifactController.artifactPoint < artifactController.cost)
            return;

        Battle.currentBattleGroup.artifactController.ShowBuff();

        UIArtifactSelect.Show();
    }
    void OnArtifactAdd(BattleGroup _battleGroup)
    {
        if (!Battle.currentBattleGroup)
            return;

        if (Battle.currentBattleGroup != _battleGroup)
            return;

        StartCoroutine(UIInit());

        UpdateArtifactPointText();
    }

    void OnChangedArtifactPoint()
    {
        UpdateArtifactPointText();
    }

    void UpdateArtifactPointText()
    {
        //현재 유물 포인트
        if (artifactPointText)
            artifactPointText.text = artifactPoint.ToStringABC();

        //유물 구매 비용
        double cost = artifactController.cost;
        textCost.text = cost.ToStringABC();

        //유물 구매 버튼 활성/비활성 처리
        buttonBuyArtifact.interactable = artifactController.artifactPoint >= artifactController.cost;
    }

    IEnumerator UIInit()
    {
        if (!layoutGroup)
            yield break;

        while (!Battle.currentBattleGroup)
            yield return null;

        while (!Battle.currentBattleGroup.artifactController)
            yield return null;
        

        if (slotPool.Count < Battle.currentBattleGroup.artifactController.artifactList.Count)
        {
            // 슬롯 생성
            int count = Battle.currentBattleGroup.artifactController.artifactList.Count - slotPool.Count;
            for (int i = 0; i < count; i++)
            {
                UIArtifactSlot artifact = Instantiate(artifactSlotPrefab).GetComponent<UIArtifactSlot>();
                artifact.transform.SetParent(layoutGroup.transform, false);
                slotPool.Add(artifact);
            }
        }

        UIArtifactSelect.Close();
        ////UIArtifactSelect.Instance.InfoClose();

        // 슬롯에 데이터 세팅
        for (int i = 0; i < slotPool.Count; i++)
        {
            slotPool[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < Battle.currentBattleGroup.artifactController.artifactList.Count; i++)
        {
            slotPool[i].gameObject.SetActive(true);
            slotPool[i].SlotInit(Battle.currentBattleGroup.artifactController.artifactList[i]);
        }

        
        //그리드 사이즈 조절. 컨텐츠 사이즈 필터 못써서 취한 조치
        GridLayoutGroup grid = layoutGroup as GridLayoutGroup;
        if (grid)
        {
            int childCount = scrollRect.content.childCount;

            float y = grid.cellSize.y * childCount + grid.spacing.y * (childCount - 1);

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, y);
        }
    }

}
