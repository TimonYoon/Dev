using UnityEngine;
using System.Collections;

public class OrderController : MonoBehaviour {

    int order;

    const float scaleModifyWeight = 0.2f;

    int shadowOrderOffset = -500;

    /// <summary>
    /// order 상속 받을 녀석
    /// </summary>
    [Tooltip("order 상속 받을 녀석. 없으면 y축에 따라 자동 계산")]
    public OrderController parent = null;
    
    public int orderOffset = 0;

    Renderer[] renderers;

    ParticleSystemRenderer[] particleRenderers;

    Renderer shadowRenderer;

    BattleGroupElement be = null;
    BoxCollider unitArea
    {
        get
        {
            if (be == null || be.battleGroup == null)
                return null;

            return null;
        }
    }

    Vector3 startSize;
        
    public float scaleRatio { get; private set; }

    float startSizeAmount = 1f;

    [System.NonSerialized]
    public float bossModify = 1f;

    int _lastOrder = -1;
    public int lastOrder { get { return _lastOrder; } private set { _lastOrder = value; } }
    //################################################################################
    void Start ()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.name == "Shadow")
            {
                shadowRenderer = renderers[i];
                break;
            }   
        }

        be = GetComponentInParent<BattleGroupElement>();

        startSize = transform.localScale;
        //startSizeAmount = transform.localScale.y;        
    }

    void OnEnable()
    {
        UpdateScale();
    }

    float lastPosY;

    public void UpdateScale()
    {

        float posY = transform.position.y;
        BoxCollider unitArea = this.unitArea;



        if (parent)
            order = parent.order;
        else
        {
            if (unitArea != null)
                order = (int)((unitArea.bounds.center.y - posY) * 100f) + 1000;
            else
                order = 0;
        }


        //order = (int)(transform.localPosition.y * -100f);

        if (order != lastOrder)
        {
            if (renderers != null)
                for (int i = 0; i < renderers.Length; i++)
                {
                    //그림자의 경우 일정량 만큼 뒤로 보냄. 예외처리
                    if (renderers[i] == shadowRenderer)
                        renderers[i].sortingOrder = order + orderOffset + shadowOrderOffset;
                    else
                        renderers[i].sortingOrder = order + orderOffset;
                }

            if (particleRenderers != null)
                for (int i = 0; i < particleRenderers.Length; i++)
                {
                    particleRenderers[i].sortingOrder = order + orderOffset;
                }

        }

        lastOrder = order;

        if (unitArea == null)
            return;

        float yDiff = unitArea.bounds.center.y - posY;  //내 위치가 기준선 보다 위에 있으면 -, 아래에 있으면 +

        float a = yDiff / (unitArea.bounds.center.y - unitArea.bounds.min.y);

        scaleRatio = a * scaleModifyWeight;

        float finalScale = startSizeAmount * (1f + scaleRatio) * bossModify;
        float x = transform.localScale.x > 0f ? 1f : -1f;

        transform.localScale = new Vector3(finalScale * x, finalScale, finalScale);
        //transform.localScale = startSize * (1f + scaleRatio);

        lastPosY = posY;
    }

	void Update ()
    {
        float posY = transform.position.y;
        if (lastPosY == posY)
            return;

        UpdateScale();
    }
}
