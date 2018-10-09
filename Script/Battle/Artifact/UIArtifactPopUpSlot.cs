using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPopUpSlot : MonoBehaviour {


    public Image artifactImage;
    public Text artifactName;
    public Text artifactMessage;
    public GameObject particleObject;

    public AnimationCurve animDisolve;
    
    public Material materialDissolve;

    public float desolveThreshold
    {
        get { return 0f; }
        set { }
    }

    Button button;

    ArtifactBaseData artifactBaseData;

    CanvasRenderer[] canvasRenderers;

    //################################################################

    void Awake()
    {
        button = GetComponent<Button>();

        canvasRenderers = GetComponentsInChildren<CanvasRenderer>();

        materialDissolve = Instantiate(materialDissolve);

        Battle.onChangedBattleGroupActiveState += OnChangedBattleGroupActiveState;

        //artifactImage.material = Instantiate(artifactImage.material);

    }

    void OnChangedBattleGroupActiveState()
    {
        //연출이 진행 중인데 또 선택됨 콜백 날라오면 연출 다 꺼버리고 바로 비활성
        if (coroutineShowResult != null)
        {
            StopCoroutine(coroutineShowResult);
            coroutineShowResult = null;
        }

        RectTransform t = GetComponent<RectTransform>();
        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;

        gameObject.SetActive(false);        
    }

    void OnEnable()
    {
        coroutineShow = StartCoroutine(Show());

    }

    Coroutine coroutineShow = null;

    IEnumerator Show()
    {
        float resultTime = 1.5f;
        float threshold = 1f;
        float alpha = 0f;
        float elapsedTime = 0f;

        artifactImage.material = materialDissolve;
        artifactImage.material.SetFloat("_Threshold", threshold);

        for (int i = 0; i < canvasRenderers.Length; i++)
            canvasRenderers[i].SetAlpha(alpha);

        //디졸브 연출+서서히 사라짐
        while (elapsedTime < resultTime)
        {
            float a = elapsedTime / resultTime;
            float thresholdAmount = animDisolve.Evaluate(a);
            threshold = 1f - thresholdAmount;
            alpha = thresholdAmount;
            artifactImage.material.SetFloat("_Threshold", threshold);

            for (int i = 0; i < canvasRenderers.Length; i++)
                canvasRenderers[i].SetAlpha(alpha);

            yield return null;

            elapsedTime += Time.unscaledDeltaTime;
        }
    }

    public void SlotInit(ArtifactBaseData data)
    {
        particleObject.SetActive(false);
        artifactBaseData = data;
        artifactName.text = data.name;
        artifactName.gameObject.SetActive(true);

        AssetLoader.AssignImage(artifactImage, "sprite/artifact", "Atlas_Artifact", artifactBaseData.icon, null);

        ArtifactController artifactController = Battle.currentBattleGroup.artifactController;

        Artifact artifact = artifactController.artifactList.Find(x => x.baseData.id == data.id);

        //현재 스택 상태에 따라 패러미터 변화량 보여줌
        int currentStack = 0;

        if (artifact != null)
            currentStack = artifact.stack;

        float power = 0;
        float.TryParse(data.formula, out power);

        string powerBefore = (power * currentStack).ToString();

        string powerAfter = (power * (currentStack + 1)).ToString();

        string description = data.message.Replace("[formula]", "<color=#00ff00ff>" + powerAfter + "</color>");
        if (artifact != null)
            description = data.message.Replace("[formula]", "<color=#00ff00ff>" + powerBefore + " → " + powerAfter + "</color>");

        artifactMessage.text = description;
        artifactMessage.gameObject.SetActive(true);

        artifactImage.material.SetFloat("_Threshold", 0f);

        //버튼 활성화
        button.interactable = true;

        //캔버스 랜더러들 알파 다 켜기
        for (int i = 0; i < canvasRenderers.Length; i++)
        {
            canvasRenderers[i].SetAlpha(1f);
        }

        //유물 선택되었을 때 콜백 등록
        UIArtifactSelect.onSelected += OnSelected;

        //게임 오브젝트 활성화
        gameObject.SetActive(true);
    }

    /// <summary> 유물을 클릭했을 때 발생. button 컴퍼넌트에의해 실행 </summary>
    public void OnClick(UIArtifactPopUpSlot uiArtifactPopUpSlot)
    {
        if (!Battle.currentBattleGroup)
            return;

        //유물 선택 처리
        UIArtifactSelect.Select(this);
    }


    Coroutine coroutineShowResult = null;

    /// <summary> 3개 중 하나의 유물 슬롯이 선택되면 발생. 선택된 유물이 이 슬롯이 아닐 수 있음 </summary>
    void OnSelected(UIArtifactPopUpSlot slot)
    {
        if(coroutineShow != null)
        {
            StopCoroutine(coroutineShow);
            coroutineShow = null;
        }

        //연출이 진행 중인데 또 선택됨 콜백 날라오면 연출 다 꺼버리고 바로 비활성
        if(coroutineShowResult != null)
        {
            StopCoroutine(coroutineShowResult);
            coroutineShowResult = null;

            RectTransform t = GetComponent<RectTransform>();
            t.localScale = Vector3.one;
            t.anchoredPosition = Vector2.zero;

            gameObject.SetActive(false);
            return;
        }

        //버튼 비활성 처리
        button.interactable = false;

        //설명 문구 끔
        artifactMessage.gameObject.SetActive(false);

        //선택 안 된 애들은 이름도 끔
        //if(this != slot)
        artifactName.gameObject.SetActive(false);


        artifactImage.material = null;

        //캔버스 랜더러들 알파 다 켜기
        for (int i = 0; i < canvasRenderers.Length; i++)
        {
            canvasRenderers[i].SetAlpha(1f);
        }

        //선택 결과에 따른 연출 재생
        if (this == slot)
        {
            //유물 추가
            Battle.currentBattleGroup.artifactController.AddArtifact(artifactBaseData);
            coroutineShowResult = StartCoroutine(ShowResult_Selected());
        }
        else
            coroutineShowResult = StartCoroutine(ShowResult_NotSelected());

    }

    void OnDisable()
    {
        //유물 선택되었을 때 콜백 해제. 이 시점 부터 얘는 기능적으로 작동하는 애가 아니라 그냥 보여주기 용으로만 사용될 뿐
        UIArtifactSelect.onSelected -= OnSelected;
    }
        
    IEnumerator ShowResult_NotSelected()
    {
        RectTransform t = GetComponent<RectTransform>();

        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;

        float resultTime = 0.3f;

        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.Rebind();
            animator.SetBool("isSelected", false);

            animator.SetTrigger("select");
        }

        float threshold = 0f;
        float alpha = 1f;
        float elapsedTime = 0f;

        //디졸브 연출+서서히 사라짐
        while (elapsedTime < resultTime)
        {
            float a = Time.unscaledDeltaTime / resultTime;
            float thresholdAmount = animDisolve.Evaluate(a);
            threshold += thresholdAmount;
            alpha -= Time.unscaledDeltaTime / resultTime;
            artifactImage.material.SetFloat("_Threshold", threshold);

            for (int i = 0; i < canvasRenderers.Length; i++)
                canvasRenderers[i].SetAlpha(alpha);

            yield return null;

            elapsedTime += Time.unscaledDeltaTime;
        }

        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;
        
        if (coroutineShowResult != null)
            coroutineShowResult = null;

        gameObject.SetActive(false);
    }

    /// <summary> 선택 됐을 때 연출 </summary>
    IEnumerator ShowResult_Selected()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.enabled = false;
            //animator.Rebind();
            //animator.SetBool("isSelected", true);

            //animator.SetTrigger("select");
        }

        RectTransform t = GetComponent<RectTransform>();

        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;

        //추가된 유물 슬롯
        UIArtifactSlot slot = UIArtifact.slotPool.Find(x => x.artifact.baseData.id == artifactBaseData.id);

        while (!slot)
            yield return null;

        //한 프레임 기다려줘야 스크롤 갱신됨
        yield return null;


        //파티클 재생
        //particleObject.SetActive(true);

        //방금 획득한 유물 리스트에 포커스 맞추기 & 유물 슬롯 위치 받아오기
        RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
        float slotHeight = slotRectTransform.sizeDelta.y;
        float posY = slotRectTransform.anchoredPosition.y;

        Vector2 a = UIArtifact.Instance.scrollRect.content.anchoredPosition;
        UIArtifact.Instance.scrollRect.content.anchoredPosition = new Vector2(a.x, -posY - slotHeight * 0.5f);

        //t.SetParent(slot.transform);

        //UIArtifact.Instance.scrollRect.viewport.GetComponent<Mask>().

        //artifactImage.SetClipRect(UIArtifact.Instance.scrollRect.viewport.rect, true);

        Canvas canvas = GetComponentInParent<Canvas>();
        
        //화면 중앙으로 확대되면서 이동
        float elapsedTime = 0f;
        while (elapsedTime < 0.4f)
        {
            Vector3 destPos = canvas.GetComponent<RectTransform>().anchoredPosition;            
            t.position = Vector2.Lerp(transform.position, destPos, 5f * Time.unscaledDeltaTime);

            Vector3 s = Vector3.Lerp(transform.localScale, Vector3.one * 2f, 3f * Time.unscaledDeltaTime);
            t.localScale = s;
            
            yield return null;

            elapsedTime += Time.unscaledDeltaTime;
        }



        //파티클 재생
        //particleObject.SetActive(true);



        float destScale = slot.relicsImage.GetComponent<RectTransform>().sizeDelta.y / GetComponent<RectTransform>().sizeDelta.y;

        //Vector3 destPos = slot.relicsImage.transform.position;

        //해당 유물 슬롯 위치로 날아가는 연출
        float resultTime = 1.5f;
        elapsedTime = 0f;
        while (elapsedTime < resultTime)
        {
            Vector3 destPos = slot.relicsImage.transform.position;
            t.position = Vector2.Lerp(transform.position, destPos, 8f * Time.unscaledDeltaTime/* elapsedTime*/);

            Vector3 s = Vector3.Lerp(transform.localScale, Vector3.one * destScale, elapsedTime);
            t.localScale = s;

            if(elapsedTime > 0.1f && t.parent != slot.transform)
            {
                t.SetParent(slot.transform);
                artifactImage.RecalculateMasking();
            }                

            yield return null;

            elapsedTime += Time.unscaledDeltaTime;
        }

        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;

        if (coroutineShowResult != null)
            coroutineShowResult = null;

        gameObject.SetActive(false);

        yield break;
    }

    /// <summary> 선택 안 됐을 때 연출 </summary>
    IEnumerator ShowResult(bool isSelected)
    {
        RectTransform t = GetComponent<RectTransform>();

        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;


        float resultTime = 1.5f;

        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.SetBool("isSelected", isSelected);
            
            animator.SetTrigger("select");
        }

        if (isSelected)
        {
            //파티클 재생
            particleObject.SetActive(true);
            //ParticleSystem particle = particleObject.GetComponentInChildren<ParticleSystem>();
            Battle.currentBattleGroup.artifactController.AddArtifact(artifactBaseData);

            ////연출 시간 동안 대기
            //yield return new WaitForSecondsRealtime(resultTime);

            ////오브젝트 비활성
            //gameObject.SetActive(false);
        }
        
        float threshold = 0f;
        float alpha = 1f;

        float startTime = Time.unscaledTime;
        float elapsedTime = 0f;


        UIArtifactSlot slot = UIArtifact.slotPool.Find(x => x.artifact.baseData.id == artifactBaseData.id);

        while (!slot)
            yield return null;

        yield return null;

        while (UIArtifact.Instance.scrollRect.velocity.magnitude > 0.1f)
            yield return null;

        RectTransform slotRectTransform = slot.GetComponent<RectTransform>();

        artifactImage.SetClipRect(UIArtifact.Instance.scrollRect.viewport.rect, true);

        if (isSelected)
        {
            float slotHeight = slotRectTransform.sizeDelta.y;
            float posY = slotRectTransform.anchoredPosition.y;

            Vector2 a = UIArtifact.Instance.scrollRect.content.anchoredPosition;

            UIArtifact.Instance.scrollRect.content.anchoredPosition = new Vector2(a.x, -posY - slotHeight * 0.5f);

            Debug.Log(UIArtifact.Instance.scrollRect.normalizedPosition + ", " + UIArtifact.Instance.scrollRect.content.childCount);

            UIArtifact.Instance.scrollRect.CalculateLayoutInputVertical();
        }

        float destScale = slot.relicsImage.GetComponent<RectTransform>().sizeDelta.y / GetComponent<RectTransform>().sizeDelta.y;

        //Vector3 destPos = slot.relicsImage.transform.position;

        //디졸브 연출+서서히 사라짐
        while (elapsedTime < resultTime)
        {
            if (isSelected)
            {

                Vector3 destPos = slot.relicsImage.transform.position;

                artifactImage.RecalculateMasking();

                t.position = Vector2.Lerp(transform.position, destPos, elapsedTime);

                Vector3 s = Vector3.Lerp(transform.localScale, Vector3.one * destScale, elapsedTime);
                t.localScale = s;
            }

            if (!isSelected)
            {
                float a = Time.unscaledDeltaTime / 0.3f;
                float thresholdAmount = animDisolve.Evaluate(a);
                threshold += thresholdAmount;
                alpha -= Time.unscaledDeltaTime / 0.3f;
                artifactImage.material.SetFloat("_Threshold", threshold);

                for (int i = 0; i < canvasRenderers.Length; i++)
                    canvasRenderers[i].SetAlpha(alpha);
            }

                
            
            yield return null;

            elapsedTime += Time.unscaledDeltaTime;
        }
                
        t.localScale = Vector3.one;
        t.anchoredPosition = Vector2.zero;

        gameObject.SetActive(false);

        yield break;
    }

}
