using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BackGroundMove : MonoBehaviour {
    
    /// <summary> 전투 카메라 스크립트 </summary>
    public BattleMoveCamera battleCamera;
    

    /// <summary> 이동 가중치 </summary>
    //[Header("이동 가중치"), Range(0f, 2f)]
    public float moveWeight = 1f;

    
    float frontDis;

    public Transform root;

    public int index = 0;

    public float offsetMin = 0f;
    public float offsetMax = 0f;

    void OnEnable()
    {
        CalculateBound();
    }

    void CalculateBound()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].GetComponent<ParticleSystem>())
                continue;

            if (i == 0)
            {
                xMin = renderers[i].bounds.min.x;
                xMax = renderers[i].bounds.max.x;
                continue;
            }

            xMin = Mathf.Min(xMin, renderers[i].bounds.min.x);
            xMax = Mathf.Max(xMax, renderers[i].bounds.max.x);
        }

        center = (xMin + xMax) * 0.5f;
        width = xMax - xMin;

        centerDiff = center - transform.position.x;
    }

    float centerDiff = 0f;

    public Vector3 startPos;

    public BattleGroup battleGroup;

    private void Awake()
    {
        if (battleGroup == null)
            battleGroup = GetComponentInParent<BattleGroup>();
    }

    void Start()
    {
        startPos = transform.position;
        BattleGroupElement be = GetComponent<BattleGroupElement>();

        be.SetBattleGroup(battleGroup);
    }

    float xMin = 0f;
    float xMax = 0f;
    public float center { get; set; }
    public float width { get; set; }

    public int cacheCount = 4;

    void Update()
    {   
        frontDis = battleCamera.b * /*-(bVector.x - aVector.x) **/  /*battleCamera.t  **/ (moveWeight);
        transform.position = new Vector3(frontDis, 0, 0) + startPos;
    }
}
