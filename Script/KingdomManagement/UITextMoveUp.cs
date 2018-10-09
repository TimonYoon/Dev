using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UITextMoveUp : MonoBehaviour {

    Text textCount;
    Color color;
    public float speed = 3f;
    public float lifeTime = 2f;
    public float high = 10f;
    public AnimationCurve xScaleAnimationCurve;
    public AnimationCurve yScaleAnimationCurve;
    Vector3 startScale;
    Vector3 startPoint;
    private void Awake()
    {
        startScale = transform.localScale;
        textCount = GetComponent<Text>();
        startPoint = transform.position;
        color = textCount.color;
    }

    private void OnDisable()
    {
        // 제자리 이동
        transform.localScale = startScale;
        textCount.color = color;
    }

    public void Show(string _text)
    {
        gameObject.SetActive(true);
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        textCount.text = _text;
        coroutine = StartCoroutine(EffectCoroutine());
    }
    Coroutine coroutine;
    
    IEnumerator EffectCoroutine()
    {
        transform.localScale = startScale;
        textCount.color = color;
        transform.position = startPoint;
        float startTime = Time.time;

        float destTime = high / speed;

        //사이즈 변경
        while (Time.time < startTime + lifeTime)
        {
            transform.Translate(Vector3.up * Time.deltaTime* speed);
            float t = (Time.time - startTime) / 1f;
            transform.localScale = new Vector3(startScale.x * xScaleAnimationCurve.Evaluate(t), startScale.y * yScaleAnimationCurve.Evaluate(t), 1f);

            yield return null;
        }

        //페이드 아웃
        float _time = 1f;
        
        while (textCount.color.a > 0)
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed);
            _time -= Time.deltaTime;
            textCount.color = new Color(color.r, color.g, color.b, _time);
            yield return null;
        }

        coroutine = null;
        gameObject.SetActive(false);
        yield break;

    }
}
