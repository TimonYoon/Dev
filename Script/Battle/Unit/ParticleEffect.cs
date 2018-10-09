using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour {

    public BattleHero hero;
    public GameObject particleRef;
    public ParticleSystem part;
    public string name;
    
    public void PlayParticleEffect()
    {
        GameObject particleObj = Battle.GetObjectInPool(name);
        if (!particleObj)
        {
            particleObj = GameObject.Instantiate(particleRef, hero.transform.position, Quaternion.identity, hero.transform.parent) as GameObject;
            particleObj.transform.localPosition = Vector3.zero;
            particleObj.name = name;
            //particleObj.SetActive(false);

            if (!particleObj.GetComponent<BattleGroupElement>())
                particleObj.AddComponent<BattleGroupElement>();

            if (Battle.Instance)
                Battle.AddObjectToPool(particleObj);
        }

        if (!particleObj)
            return;
        //캐릭터 위치에 따라 뒤집기
        if (hero)
        {
            bool isFlip = hero.flipX;
            float x = isFlip ? -1f : 1f;
            particleObj.transform.localScale = new Vector3(x, 1f, 1f);
        }

        
        particleObj.transform.SetParent(hero.transform.parent);

        particleObj.transform.position = hero.skeletonAnimation.transform.position;
        

        OrderController o = particleObj.GetComponent<OrderController>();
        if (o)
            o.parent = hero.GetComponent<OrderController>();
        //발사체 오브젝트 활성
        if (Battle.Instance)
            particleObj.GetComponent<BattleGroupElement>().SetBattleGroup(hero.battleGroup);

        particleObj.SetActive(true);

        ParticleSystem particle = particleObj.GetComponent<ParticleSystem>();
        
        if (particle)
        {
            particle.Play();
        }
            
    }
}
