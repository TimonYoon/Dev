using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : BattleGroupElement {

    LineRenderer lineRenderer;

    public float duration = 1f;

    public Vector3 startPosition = Vector3.zero;

    public Vector3 endPosition = Vector3.zero;

    public float expandWidthTime = 0.2f;

    float startWidth;
    override protected void Awake()
    {
        base.Awake();

        lineRenderer = GetComponentInChildren<LineRenderer>();
        startWidth = lineRenderer.widthMultiplier;
    }

    void OnEnable()
    {
        Show();
    }

    float startTime;

    Coroutine coShow = null;
    public void Show()
    {
        if (coShow != null)
            StopCoroutine(coShow);

        coShow = StartCoroutine(ShowA());
    }
    

    IEnumerator ShowA()
    {
        startTime = Time.time;

        lineRenderer.SetPosition(1, startPosition);

        float width = 0f;
        if (duration < 0.2f)
            width = startWidth;

        float elapsedTime = 0f;
        while (Time.time < startTime + duration)
        {
            elapsedTime = Time.time - startTime;

            float a = elapsedTime / duration;

            Vector3 destPos = Vector3.Lerp(startPosition, endPosition, a);

            lineRenderer.SetPosition(1, destPos);

            if (width < startWidth && expandWidthTime > 0f)
                width += elapsedTime / expandWidthTime;
            else
                width = startWidth;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
