using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 배틀 그룹의 활성화/비활성화 상태를 체크해서 해당 파티클 또는 오브젝트 랜더러 변경하는 코드 </summary>
public class BattleGroupElement : MonoBehaviour {
    
    public BattleGroup battleGroup { get; private set; }

    SpriteRenderer[] spriteRenderers = null;
    MeshRenderer[] meshRenderers = null;
    Renderer[] renderers = null;
    Light[] lights = null;
    ParticleSystemRenderer[] particles = null;
    Canvas[] canvases = null;

    /// <summary> isActive상황에 보여줄 오브젝트를 미리 캐싱할껀지, 매번 검색할껀지 여부. false면 매번 새로 검색 </summary>
    public bool cacheRenderObjs = true;

    /// <summary> 배틀 그룹 세팅하는 부분 </summary>
    virtual public void SetBattleGroup(BattleGroup _battleGroup)
    { 

        if(_battleGroup != battleGroup)
        {
            if(battleGroup != null)
                battleGroup.onChangedActiveState -= OnChangeBattleGroupActiveState;

            if(_battleGroup != null)
                _battleGroup.onChangedActiveState += OnChangeBattleGroupActiveState;
        }

        battleGroup = _battleGroup;

        UpdateActiveState();
    }

    void OnEnable()
    {
        UpdateActiveState();
    }

    void OnDestroy()
    {
        if(battleGroup != null)
            battleGroup.onChangedActiveState -= OnChangeBattleGroupActiveState;
    }



    virtual protected void Awake()
    {
        OptionManager.onChangedLightEffect += OnChangedLightEffect;

        spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        lights = gameObject.GetComponentsInChildren<Light>();
        particles = gameObject.GetComponentsInChildren<ParticleSystemRenderer>();
        canvases = gameObject.GetComponentsInChildren<Canvas>();
        //Battle.onChangedBattleGroupActiveState += 

        if (Battle.Instance)
        {
            Battle.onChangedBattleGroup += OnChangedBattleGroup;
            Battle.onRemoveBattle += OnRemoveBattle;
            UpdateShader();
        }

        
    }

    void UpdateShader()
    {
        return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i])
                continue;

            Material[] materials = renderers[i].materials;

            for (int a = 0; a < materials.Length; a++)
            {
                if (materials[a].name.Contains("Sprites-Default"))
                    continue;

                if (materials[a].shader == OptionManager.defaultShader || materials[a].shader == OptionManager.vertectLitShader)
                {
                    Shader s = OptionManager.lightEffect ? OptionManager.vertectLitShader : OptionManager.defaultShader;

                    materials[a].shader = s;

                    //if (OptionManager.lightEffect)
                    //    materials[a].color = Color.white;

                    //materials[a].SetFloat("_ZWrite", 1f);
                }
            }
        }
    }

    void OnChangedLightEffect()
    {
        UpdateShader();
    }

    void OnRemoveBattle()
    {
        if(battleGroup == null || Battle.currentBattleGroup == null)
        {
            //Debug.Log("배틀그룹 없는 애들");
            return;
        }

        if (battleGroup.battleType != Battle.currentBattleGroup.battleType)
            return;

        SetBattleGroup(null);
        gameObject.SetActive(false);
    }

    void OnChangedBattleGroup(BattleGroup _battleGroup)
    {

        if (battleGroup != _battleGroup)
            return;

        UpdateActiveState();
        //SetBattleGroup(battleGroup);
    }

    void OnChangeBattleGroupActiveState(BattleGroup _battleGroup)
    {
        UpdateActiveState();
    }

    public void UpdateActiveState()
    {
        if (battleGroup != null)
            isActive = battleGroup.isRenderingActive;
        else
            isActive = false;
    }

    bool _isActive = false;
    /// <summary> 현재 랜더러 on/off 상황 </summary>
    public bool isActive
    {
        get { return _isActive; }
        set
        {
            if (!cacheRenderObjs)
            {
                spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
                meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                renderers = gameObject.GetComponentsInChildren<Renderer>();
                lights = gameObject.GetComponentsInChildren<Light>();
                particles = gameObject.GetComponentsInChildren<ParticleSystemRenderer>();
                canvases = gameObject.GetComponentsInChildren<Canvas>();
            }

            if (spriteRenderers != null)
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].enabled = value;
                }

            if (meshRenderers != null)
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    meshRenderers[i].enabled = value;
                }

            if (renderers != null)
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = value;
                }
            
            if (lights != null)
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].enabled = value;
                }
            
            if (particles != null)
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].enabled = value;
                }

            if (canvases != null)
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].enabled = value;
                }

            _isActive = value;

        }
    }
}
