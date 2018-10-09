using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAdAlarmIcon : MonoBehaviour {

    RectTransform rectTransform;

	void Awake ()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    float randomTimeModify;
    void OnEnable()
    {
        randomTimeModify = Random.Range(-10f, 10f);
    }

    Vector2 startPos;

    public float moveAmountX;
    public float moveAmountY;
    public float speedX;
    public float speedY;

    void Update ()
    {
        rectTransform.anchoredPosition = startPos
            + Vector2.up * Mathf.Sin((Time.unscaledTime + randomTimeModify) * speedY) * moveAmountY
            + Vector2.right * Mathf.Cos((Time.unscaledTime + randomTimeModify) * speedX) * moveAmountX;

		
	}
}
