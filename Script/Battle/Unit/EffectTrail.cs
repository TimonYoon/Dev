using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTrail : MonoBehaviour {

    public LineRenderer lineRenderer = null;
    public GameObject trailPoint;
    public BattleHero hero;
    bool isOn = false;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void OnRestartBattle(BattleGroup battle)
    {
        if (battle.battlePhase == BattleGroup.BattlePhase.FadeOut || battle.battlePhase == BattleGroup.BattlePhase.Finish)
            lineRenderer.enabled = false;
    }

    public void SetStartPoint()
    {
        hero.battleGroup.onChangedBattlePhase += OnRestartBattle;
        lineRenderer.SetPosition(0, trailPoint.transform.position);
    }

    public void OffEffect()
    {
        isOn = false;
        lineRenderer.enabled = false;
    }

    public void DrawTrail(bool isOn)
    {
        if (co != null)
        {   
            lineRenderer.enabled = false;
            StopCoroutine(co);
        }

        this.isOn = isOn;
        co = StartCoroutine(TrailRendering());
    }

    Coroutine co = null;
    IEnumerator TrailRendering()
    {
        lineRenderer.enabled = true;
        while(isOn && !hero.isDie)
        {
            lineRenderer.SetPosition(1, trailPoint.transform.position);
            yield return null;
        }
        lineRenderer.enabled = false;
        hero.battleGroup.onShowStage -= OnRestartBattle;
    }
}
