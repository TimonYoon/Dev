using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 완료표시 </summary>
public class UIComplete : MonoBehaviour {

    [SerializeField]
    RectTransform icon;

    //[SerializeField]
    //List<GameObject> bgList;

    [SerializeField]
    AnimationCurve curve;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(ShowCoroutine());
        }
    }



    /// <summary> 초기화 과정 </summary>
    public void InitComplete(bool isDone)
    {
        gameObject.SetActive(isDone);
    }


    /// <summary> 출석 연출 </summary>
    public void Show()
    {
        StartCoroutine(ShowCoroutine());
    }



    public IEnumerator ShowCoroutine()
    {
        float startTime = Time.unscaledTime;
        float t = 0;
        icon.gameObject.SetActive(true);
        while (t <1)
        {
            t= Time.unscaledTime - startTime;
            icon.localScale =new Vector3(curve.Evaluate(t), curve.Evaluate(t),1);
            yield return null;


        }
        //for (int i = 0; i < bgList.Count; i++)
        //{
        //    bgList[i].SetActive(true);
        //}
        
    }

}
