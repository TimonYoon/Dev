using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//영웅에 달까 배틀그룹에 달까 고민했는데. 배틀그룹에 달아서 통합관리하는것이 나을 것 같음
//왜냐하면 동시에 여기저기서 말풍선이 남발하면 안되니까 누군가 중앙에서 관리해주는게 나을 것 같음
/// <summary> 전투 중 대사 치는거(말풍선) 관리하고 컨트롤하는 클래스 </summary>
public class BattleSpeechController : MonoBehaviour {

    BattleGroup battleGroup = null;

    //옵션에서 말풍선 끄고/켜기 이런거 하려면 캔버스 따로 두는게 나을 것 같아서 따로 만듬
    /// <summary> 말풍선들이 모여 있는 캔버스 </summary>
    public Canvas canvas;


    //[Tooltip("말풍선 프리팹")]
    //public GameObject speechBubblePrefab;

    List<SpeechBubble> speechBubbleList = new List<SpeechBubble>();
    
    /// <summary> 최대로 동시에 띄울 수 있는 배고픔 메시지 수 </summary>
    [Tooltip("최대로 동시에 띄울 수 있는 배고픔 메시지 수")]
    public int maxHungerMessageCount = 2;

    /// <summary> 배고플 때 나올 문구 리스트. 이 중에 랜덤하게 표시됨 </summary>
    [Tooltip("배고플 때 나올 문구 리스트. 이 중에 랜덤하게 표시됨")]
    public List<string> hungerMessageList = new List<string>();

    void Awake()
    {
        battleGroup = GetComponentInChildren<BattleGroup>();

        if (!battleGroup)
            return;

        //콜백들 등록
        battleGroup.originalMember.onAdd += OnAddBattleHero;
        battleGroup.originalMember.onRemove += OnRemoveBattleHero;
        //battleGroup.onAddBattleHero += OnAddBattleHero;
        battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
        //battleGroup.onChangedActiveState += OnChangedActiveState;
    }

    void OnAddBattleHero(BattleHero _battleHero)
    {
        //몬스터 말풍선은 등록 암함. 임시
        if (_battleHero.team == BattleUnit.Team.Blue)
            return;

        if (!Battle.Instance || !Battle.speechBubblePrefab)
            return;

        GameObject go = Instantiate(Battle.speechBubblePrefab, canvas.transform, false);
        go.SetActive(false);

        SpeechBubble sb = go.GetComponentInChildren<SpeechBubble>();
        if(sb)
            speechBubbleList.Add(sb);
    }

    void OnRemoveBattleHero(BattleHero battleHero)
    {

    }

    //void OnChangedActiveState(BattleGroup _battleGroup)
    //{
    //    canvas.enabled = _battleGroup.isActive;
    //}

    void OnChangedBattlePhase(BattleGroup _battleGroup)
    {
        if(_battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut)
            HideAll();
    }
    	
	void Start ()
    {
        HideAll();
    }	
	
    //모든 말풍선 끄기
    void HideAll()
    {
        if (speechBubbleList == null || speechBubbleList.Count == 0)
            return;

        for (int i = 0; i < speechBubbleList.Count; i++)
        {
            speechBubbleList[i].Hide();
        }
    }
    
    BattleHero lastHero = null;

    BattleHero GetRandomHero()
    {
        //아군 중, 사망하지 않은 애들 리스트. 소환수는 제외
        List<BattleHero> heroList = battleGroup.redTeamList.FindAll(x => x.team == BattleUnit.Team.Red && !x.isDie && x.lifeTime == 0f);

        if (heroList == null || heroList.Count == 0)
            return null;

        int i = Random.Range(0, heroList.Count);

        //최근에 말했던애는 또 말하지 않음. (돌아가면서 말하게 하기 위함
        if (heroList.Count > 1 && heroList[i] == lastHero && currentActivatedSpeechBubbleCount > 0)
            return GetRandomHero();

        if (heroList[i].isDie)
            return GetRandomHero();

        return heroList[i];
    }

    /// <summary> 현재 활성화된 말풍선 수량 </summary>
    int currentActivatedSpeechBubbleCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < speechBubbleList.Count; i++)
            {
                if (speechBubbleList[i].isActive)
                    count++;
            }

            return count;
        }
    }

    /// <summary> 말풍선 띄우기 </summary>
    /// <param name="t"> 따라다닐 대상. 지정 안하면 작동 안 함 </param>
    /// <returns> 말풍선 띄우기에 성공하면 true 반환 </returns>
    bool ShowSpeechBubble(BattleUnit target, Transform t)
    {
        if (t == null)
            return false;
        
        if (currentActivatedSpeechBubbleCount >= maxHungerMessageCount)
            return false;

        SpeechBubble sb = speechBubbleList.Find(x => !x.isActive);
        if (!sb)
            return false;

        if (hungerMessageList == null || hungerMessageList.Count == 0)
            return false;

        int r = Random.Range(0, hungerMessageList.Count);
        string message = hungerMessageList[r];

        sb.gameObject.SetActive(true);
        sb.Show(target, t, message, 5f);
        return true;
    }
}
