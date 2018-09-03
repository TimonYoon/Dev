using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRange : MonoBehaviour
{
    //public SkillBase skill;
        
    //public List<BattleHero> units = new List<BattleHero>();

    //public Dictionary<int, BattleHero> battleHeroDic = new Dictionary<int, BattleHero>();

    BattleUnit owner;

    public new Collider2D collider2D;

    Vector2 originalPos;

    void Awake()
    {
        collider2D = GetComponent<Collider2D>();
        originalPos = transform.localPosition;


        owner = GetComponentInParent<BattleUnit>();
        if (owner is BattleHero)
        {
            BattleHero h = owner as BattleHero;
            h.onChangedFlip += OnChangedFlip;

            CheckAndUpdateFlip();
        }
    }

    void CheckAndUpdateFlip()
    {
        BattleHero h = owner as BattleHero;
        if (h)
        {
            float x = h.flipX ? -1f : 1f;

            transform.localPosition = new Vector2(originalPos.x * x, transform.localPosition.y);
        }

    }

    void OnChangedFlip()
    {
        CheckAndUpdateFlip();
    }

    //void OnTriggerEnter2D(Collider2D col)
    //{
    //    if (!owner)
    //        return;

    //    if (col.CompareTag("AttackRange"))
    //        return;

    //    BattleHero unit = col.GetComponentInParent<BattleHero>();
    //    if (unit && unit.battleGroup == owner.battleGroup)
    //    {
    //        //units.Add(unit);
    //        if(!battleHeroDic.ContainsKey(unit.GetHashCode()))
    //            battleHeroDic.Add(unit.GetHashCode(), unit);
    //    }
    //        //Debug.Log("Enter " + unit.gameObject.name);
    //}

    //void OnTriggerExit2D(Collider2D col)
    //{
    //    if (!owner)
    //        return;

    //    if (col.CompareTag("AttackRange"))
    //        return;

    //    BattleHero unit = col.GetComponentInParent<BattleHero>();
    //    if (unit && unit.battleGroup == owner.battleGroup)
    //    {
    //        //units.Remove(unit);

    //        if (battleHeroDic.ContainsKey(unit.GetHashCode()))
    //            battleHeroDic.Remove(unit.GetHashCode());
    //    }

    //    //Debug.Log("Exit " + col.gameObject.name);
    //}

    //void OnTriggerStay2D(Collider2D col)
    //{


    //    //Debug.Log("Stay " + col.gameObject.name);
    //}
}
