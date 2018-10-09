using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 말풍선 오브젝트 </summary>
public class SpeechBubble : MonoBehaviour
{
    //대사 보여지는 곳
    Text text;

    BattleUnit targetUnit;

    /// <summary> 따라다닐 대상 </summary>
    Transform targetTransform;

    //##########################################################
    void Awake()
    {
        text = GetComponentInChildren<Text>();
    }


    public bool isActive { get; private set; }

    /// <summary> 말풍선 띄우기 </summary>
    /// <param name="_target"> 따라다닐 대상 </param>
    /// <param name="message"> 말풍선에 띄울 문구 </param>
    /// <param name="lifeTime"> 말풍선 수명 </param>
    public void Show(BattleUnit unit, Transform _target, string message, float lifeTime = 3f)
    {
        targetUnit = unit;
        targetTransform = _target;
        text.text = message;

        if (isActive)
            return;

        StartCoroutine(ShowA(unit, _target, message, lifeTime));
    }

    IEnumerator ShowA(BattleUnit unit, Transform _target, string message, float lifeTime = 3f)
    {
        isActive = true;

        text.text = message;
        gameObject.SetActive(true);

        float startTime = Time.time;

        Vector3 offset = Vector3.right * unit.collider.radius * 0.8f + Vector3.up * unit.collider.radius * 0.8f;

        while (Time.time < startTime + lifeTime/* && !unit.isDie*/)
        {
            if (targetTransform)
            {
                transform.position = targetTransform.position + offset;
            }

            if (unit.isDie)
            {
                break;
                //Hide();
            }   

            yield return null;
        }

        Hide();

        //isActive = false;

        yield break;
    }

    /// <summary> 말풍선 끄기. 수명이 남아있더라도 강제로 끔 </summary>
    public void Hide()
    {
        isActive = false;
        text.text = "";
        gameObject.SetActive(false);
    }    

 //   void Update ()
 //   {
 //       if (!target)
 //           return;

 //       transform.position = target.position;
		
	//}
}
