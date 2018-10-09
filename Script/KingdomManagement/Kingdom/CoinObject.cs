using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinObject : MonoBehaviour {


    public bool isEnd = false;
    public float speed = 350f;

    public Transform coin;

    public Text textAmount;
    public Image imageCoinIcon;
    //private void Start()
    //{
    //    Init(3800d);
    //}

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        fadeCoroutine = null;
        transform.position = Vector3.zero;
        imageCoinIcon.color = new Color(1, 1, 1, 1);
        textAmount.color = new Color(1, 1, 1, 1);
        target = null;
        //isEnd = true;
    }

    Vector3 genPivot;

    Vector3 goalPivot;

    Transform target;

    public void Init(double amount, Transform _target)
    {
        target = _target;
        transform.position = new Vector3(target.position.x - 5, target.position.y + 2, target.position.z);
        //goalPivot = new Vector3(target.position.x, target.position.y + 10,0);
        textAmount.text = "+" + amount.ToStringABC();
        isEnd = false;
        startTime = Time.time;

        goalPivot = transform.position + Vector3.up * 10;
    }

    float startTime;
    private void Update()
    {
        if (target != null)
        {
            //goalPivot = new Vector3(target.position.x -5, target.position.y + 10, target.position.z);

            

            coin.Rotate(Vector3.up * Time.deltaTime *speed);
            transform.position = Vector3.Lerp(transform.position, goalPivot, (Time.time- startTime)/3);

            float dis = Vector3.Distance(transform.position, goalPivot);
            if(dis <= 1f)
            {
                //Debug.Log(transform.localPosition + " / " + goalPivot);
                //gameObject.SetActive(false);

                if(fadeCoroutine == null)
                    fadeCoroutine = StartCoroutine(FadeText());
            }
        }
    }

    Coroutine fadeCoroutine = null;
    IEnumerator FadeText()
    {
        float startTime = Time.time;

        while(true)
        {
            float t = 1 - (Time.time - startTime);


            imageCoinIcon.color = new Color(1, 1, 1, t);
            textAmount.color = new Color(1, 1, 1, t);

            if (t < 0.1f)
            {
                gameObject.SetActive(false);
                yield break;
            }                

            yield return null;
        }
        
    }
}
