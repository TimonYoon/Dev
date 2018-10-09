using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIDamageText : MonoBehaviour {

    public Text text;

    string _value;
    public string value
    {
        get { return _value; }
        set
        {
            _value = value;
            text.text = value;
        }
    }
    public Color color = Color.white;
    public float speed = 3f;
    public float lifeTime = 2f;
    public AnimationCurve xScaleAnimationCurve;
    public AnimationCurve yScaleAnimationCurve;

    void Awake()
    {
        text = GetComponent<Text>();

        OptionManager.Instance.onChangedDamageEffect += OnChangedDamageEffectSetting;
    }

    /// <summary> 옵션창에서 설정 바뀌었을 때 </summary>
    void OnChangedDamageEffectSetting()
    {
        if (!OptionManager.Instance.isOnDamageEffect)
        {
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                gameObject.SetActive(false);
            }

        }
    }

    void OnDisable()
    {
        // 제자리 이동
        transform.localPosition = Vector3.zero;
        text.color = color;
    }

    public void Show()
    {
        coroutine = StartCoroutine(EffectCoroutine());
    }

    Coroutine coroutine = null;
    public float high = 10f;
    IEnumerator EffectCoroutine()
    {
        text.color = color;
        //transform.localScale = new Vector3(1, 1, 1);
        Vector3 startPoint = transform.localPosition;
        Vector3 startScale = transform.localScale;
        float startTime = Time.time;

        float destTime = high / speed;

        //float lastTime = Time.time;

        //사이즈 변경
        while (Time.time < startTime + lifeTime)
        {
            //float deltaTime = Time.time - lastTime;
            
            transform.Translate(Vector3.up * Time.deltaTime * speed);
            float t = ( Time.time - startTime) / 1f;
            transform.localScale = new Vector3(startScale.x * xScaleAnimationCurve.Evaluate(t), startScale.y * yScaleAnimationCurve.Evaluate(t), 1f);

            
            //lastTime = Time.time;
            yield return null;
        }

        //페이드 아웃
        float _time = 1f;
        text.color = color;
        while (text.color.a > 0)
        {
            
            transform.Translate(Vector3.up * Time.deltaTime * speed);
            _time -= Time.deltaTime;
            text.color = new Color(color.r, color.g, color.b, _time);
            yield return null;
        }

        //종료
        gameObject.SetActive(false);
        transform.localScale = new Vector3(1, 1, 1);

        coroutine = null;

    }
}
