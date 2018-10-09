using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTeam : MonoBehaviour {

    public CustomList<BattleHero> actorList = new CustomList<BattleHero>();

    public SimpleDelegate onChangedTeamActorDieCount;

    /// <summary> 팀원 수 </summary>
    public int teamMemberCount { get; private set; }

    int _deadTeamMemberCount = 0;
    /// <summary> 죽은 팀원의 수 </summary>
    public int deadTeamMemberCount
    {
        get { return _deadTeamMemberCount; }
        protected set
        {
            bool isChack = _deadTeamMemberCount != value;
            _deadTeamMemberCount = value;

            if (isChack && onChangedTeamActorDieCount != null)
            {
                onChangedTeamActorDieCount();
            }
        }
    }
    /// <summary> 전원 사망 </summary>
    public bool isAllDie { get { return teamMemberCount == deadTeamMemberCount; } }

    public void Init(BattleGroup battleGroup, BattleUnit.Team team = BattleUnit.Team.Red, List<HeroData> heroList = null)
    {
        StartCoroutine(InitCoroutine(battleGroup, team, heroList));
    }

    public IEnumerator InitCoroutine(BattleGroup battleGroup, BattleUnit.Team team = BattleUnit.Team.Red, List<HeroData> heroList = null)
    {
        while(actorList.Count > 0)
        {
            actorList[0].onDie -= OnDie;
            actorList[0].Despawn();
            yield return null;
        }
        deadTeamMemberCount = 0;
        teamMemberCount = 0;

        if (heroList == null)
            yield break;
        teamMemberCount = heroList.Count;

        for (int i = 0; i < teamMemberCount; i++)
        {
            BattleHero battleHero = null;
            yield return StartCoroutine(actorPool.Instance.GetActor(heroList[i].heroID, x => battleHero = x));

            if (!battleHero)
                continue;

            battleHero.team = team;
            if (battleGroup.battleType == BattleGroup.BattleType.PvP)
                battleHero.InitPvP(battleGroup, heroList[i]);
            else if (battleGroup.battleType == BattleGroup.BattleType.Normal)
                battleHero.Init(battleGroup, heroList[i]);

            battleHero.gameObject.SetActive(true);
            battleHero.ReGen();
            battleHero.onDie += OnDie;

            actorList.Add(battleHero);

        }


        //스테이지 시작 trigger인 버프 적용하기
        for (int i = 0; i < actorList.Count; i++)
        {
            BattleHero hero = actorList[i];
            for (int a = 0; a < hero.buffController.buffList.Count; a++)
            {
                Buff buff = hero.buffController.buffList[a];
                if (buff.baseData.trigger != "OnStartStage")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = hero;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = hero;

                if (target && !target.isDie)
                    target.buffController.AttachBuff(hero, buff.baseData.triggerBuff, 1, buff);
            }
        }
    }

    void OnDie(BattleUnit unit)
    {
        deadTeamMemberCount++;
    }
}
