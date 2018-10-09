using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//Todo: 하는 일이 늘어났으니 클래스 이름 바꿔야 할 듯
/// <summary> 파티클 종료 되면 게임 오브젝트 비활성 시켜주는 녀석 => 배틀그룹 상태에 따라 보여지는 것도 관리함 </summary>
public class SelfDestroyParticle : MonoBehaviour {

    //List<ParticleSystemRenderer> particleRenderers;
    //List<SpriteRenderer> spriteRenderers;
    //List<Canvas> canvas;
    ParticleSystem particle;
    //List<Light> lights;

    //BattleGroup _battleGroup = null;
    ///// <summary> 소속된 배틀 그룹 </summary>
    //public BattleGroup battleGroup
    //{
    //    get
    //    {
    //        if (_battleGroup != null)
    //            return _battleGroup;
            
    //        if (transform.parent == null)
    //            return null;

    //        //배틀그룹 찾기. battleGroup 찾을 때 까지, root 전까지 상위 hierachy에서 검색
    //        Transform t = transform;
    //        while (t != null && transform.parent != transform.root && !_battleGroup)
    //        {
    //            t = t.parent;

    //            if (t != null && t.GetComponent<BattleGroup>())
    //            {
    //                _battleGroup = t.GetComponent<BattleGroup>();
    //                return _battleGroup;
    //            }
    //        }

    //        return null;
    //    }
    //}


    void Awake ()
    {
        particle = GetComponentInChildren<ParticleSystem>();        
        //particleRenderers = new List<ParticleSystemRenderer>(GetComponentsInChildren<ParticleSystemRenderer>());
        //spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        //canvas = new List<Canvas>(GetComponentsInChildren<Canvas>());
        //lights = new List<Light>(GetComponentsInChildren<Light>());
    }

    //void OnEnable()
    //{
    //    if (battleGroup != null)
    //        battleGroup.onChangedActiveState += UpdateVisible;

    //    UpdateVisible(battleGroup);
    //}

    //void OnDisable()
    //{
    //    if (battleGroup != null)
    //        battleGroup.onChangedActiveState -= UpdateVisible;

    //    UpdateVisible(battleGroup);
    //}
    
    //void Start()
    //{
    //    if (battleGroup != null)
    //        battleGroup.onChangedActiveState += UpdateVisible;

    //    UpdateVisible(battleGroup);
    //}
    	
    ////배틀 그룹 활성/비활성 상태에 따라서 파티클 render 끄고 켜기
    //void UpdateVisible(BattleGroup _battleGroup)
    //{
    //    if (!battleGroup)
    //        return;
        
    //    //모든 파티클 랜더러 활성/비활성
    //    if (particleRenderers != null)
    //    {
    //        for (int i = 0; i < particleRenderers.Count; i++)
    //        {
    //            particleRenderers[i].enabled = battleGroup.isActive;
    //        }   
    //    }
        
    //    // 모든 스프라이트 렌더러 활성/비활성   
    //    if(spriteRenderers != null)
    //    {
    //        for (int i = 0; i < spriteRenderers.Count; i++)
    //        {
    //            spriteRenderers[i].enabled = battleGroup.isActive;
    //        }
    //    }

    //    if (canvas != null)
    //    {
    //        for (int i = 0; i < canvas.Count; i++)
    //        {
    //            canvas[i].enabled = battleGroup.isActive;
    //        }
    //    }

    //    //모든 라이트 활성/비활성
    //    if(lights != null)
    //        for(int i = 0; i < lights.Count; i++)
    //        {
    //            if (battleGroup.isActive)
    //                lights[i].cullingMask = 1 << LayerMask.NameToLayer("Battle");
    //            else
    //                lights[i].cullingMask = 0;// LayerMask.NameToLayer("Battle");
    //        }            
    //}

	void Update ()
    {
        //파티클 플레이 끝나면 게임오브젝트 비활성
        if (particle && particle.isStopped)
            gameObject.SetActive(false);
	}    
}
;