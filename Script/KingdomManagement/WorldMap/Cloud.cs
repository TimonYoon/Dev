using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour {

    float goalPointX;
    float origialPointx;
    float dis;

    RectTransform rect;
    private void Awake()
    {
        rect = gameObject.GetComponent<RectTransform>();
        origialPointx = rect.position.x;
    }
    void Start()
    {
        ResetCloud();
    }

    bool isReset;
    void Update ()
    {
		if(Input.GetKeyDown(KeyCode.R))
        {
            isReset = !isReset;
        }

        if(isReset == false)
        {
            if (coroutine != null)
                return;
            coroutine = StartCoroutine(Show());
        }
        else
        {
            ResetCloud();
        }
	}
    Coroutine coroutine;
    float delayTime;

    IEnumerator Show()
    {
        yield return new WaitForSeconds((dis / 15f));
        //Debug.Log("dis : " + dis);
        yield return new WaitForSeconds(delayTime);
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(goalPointX, transform.position.y), CloudController.Instance.speed * Time.deltaTime);
            yield return null;
        }
        
    }
    float dir;
    void ResetCloud()
    {
        
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        dis = Vector2.Distance(new Vector2(rect.position.x, 0), new Vector2(CloudController.Instance.centerPoint.position.x, 0));
        dir = origialPointx - CloudController.Instance.centerPoint.position.x < CloudController.Instance.centerPoint.position.x ? -1f : 1f;
        goalPointX = origialPointx + (CloudController.Instance.movingPointX * dir);
        delayTime = Random.Range(CloudController.Instance.minStartDelay, CloudController.Instance.maxStartDelay);

        rect.position = new Vector2(origialPointx, rect.position.y);
    }
}
