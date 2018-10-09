using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaxObjectGold : MonoBehaviour {

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;

        if(coroutineSpawn == null)
        {
            coroutineSpawn = StartCoroutine(Spawn());
        }

        //if (coroutineMove != null)
        //{
        //    StopCoroutine(coroutineMove);
        //    coroutineMove = null;
        //}
        //coroutineMove = StartCoroutine(Move());
    }

    private void OnDisable()
    {
        if (coroutineMove != null)
        {
            StopCoroutine(coroutineMove);
            coroutineMove = null;
        }
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }
    IEnumerator Spawn()
    {
        Vector3 spawnPos = transform.position;
        transform.position = spawnPos;

        //좌우 이동
        float x = /*Random.Range(0, 2) == 0 ? Random.Range(-4f, -15f) :*/ Random.Range(10f, 30f);
        float speedX = Random.Range(0.5f, 1.5f);
        float randomY = Random.Range(-1f, 1f);

        //처음 위로 튀겨올라가는 정도
        float height = Random.Range(5f, 10f);

        //바운스 주기
        float frequency = Random.Range(2f, 3f);

        float startTime = Time.unscaledTime;
        int bounceCount = 0;
        while ((Time.unscaledTime - startTime) * 2 < 2f + 1f)
        {
            float elapsedTime = (Time.unscaledTime - startTime) *2;
            float y = Mathf.Sin(elapsedTime * frequency - bounceCount * Mathf.PI) * height;
            if (y < 0)
            {
                y *= -1f;
                bounceCount++;
            }

            //튕길 때 마다 속도 감소
            float bouncePower = Mathf.Pow(0.5f, bounceCount);
            if (bouncePower < 0.1f)
                bouncePower = 0f;

            y *= bouncePower;

            float posX = Mathf.Lerp(transform.position.x, spawnPos.x + x, speedX * Time.unscaledDeltaTime);

            transform.position = Vector3.Lerp(spawnPos, spawnPos/* + Vector3.right * x*/ + Vector3.up * (y + randomY), elapsedTime);

            transform.position = new Vector3(posX, transform.position.y, transform.position.z);

            yield return null;
        }

        coroutineSpawn = null;

        if (coroutineMove == null)
            coroutineMove = StartCoroutine(Move());
    }
    Vector3 orignalPoint;

    Coroutine coroutineSpawn = null;
    Coroutine coroutineMove = null;
    IEnumerator Move()
    {
        if (UIMoney.Instance == false)
        {
            gameObject.SetActive(false);
            yield break;
        }
        orignalPoint = transform.position;

        float waitTime = Random.Range(0,0.5f);
        //yield return new WaitForSeconds(waitTime);


        
        Vector3 direction = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        //Debug.Log("방향 : " + direction);
        //float time = Time.unscaledTime;

        //while (true)
        //{
        //    if (AutoGoldGeneration.Instance.texTime < (Time.time - time))
        //    {
        //        break;
        //    }
        //    transform.Translate(direction * (AutoGoldGeneration.Instance.texSpeed) * Time.deltaTime);


        //    yield return null;
        //}


        int a = Random.Range(-1, 1);
        int b = Random.Range(-1, 1);
        //transform.position = Vector2.MoveTowards(transform, )

        float speed = Random.Range(0.5f, 1.5f);
        float startTime = Time.unscaledTime;
        //float r = Random.Range(-0.5f, 0.5f);
        while (Time.unscaledTime < startTime + 2f)
        {
            float elapsedTime = (Time.unscaledTime - startTime) * 2;

            float t = elapsedTime / 2f;



            transform.position = Vector3.Lerp(transform.position, UIMoney.Instance.shopMoneyGoldText.transform.position, elapsedTime * speed);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, t);
            //transform.position = GetPointOnBezierCurve(orignalPoint, new Vector3(orignalPoint.x, orignalPoint.y + (a*AutoGoldGeneration.Instance.texSpeed), orignalPoint.z), new Vector3(UIMoney.Instance.shopMoneyGoldText.transform.position.x, UIMoney.Instance.shopMoneyGoldText.transform.position.y + (b* AutoGoldGeneration.Instance.texSpeed), UIMoney.Instance.shopMoneyGoldText.transform.position.z), UIMoney.Instance.shopMoneyGoldText.transform.position, elapsedTime * speed);


            //Vector3 center = (orignalPoint + UIMoney.Instance.shopMoneyGoldText.transform.position) * 0.5F;
            //center -= new Vector3(0, r, 0);
            //Vector3 riseRelCenter = orignalPoint - center;
            //Vector3 setRelCenter = UIMoney.Instance.shopMoneyGoldText.transform.position - center;

            //transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, t * speed * AutoGoldGeneration.Instance.moveSpeed);
            //transform.position += center;


            float distance = Vector2.Distance(transform.position, UIMoney.Instance.shopMoneyGoldText.transform.position);
            if (distance < 3f)
                break;


           
            yield return null;
        }
        AutoGoldGeneration.ScaleText();
        gameObject.SetActive(false);

    }

    Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 a = Lerp(p0, p1, t);
        Vector3 b = Lerp(p1, p2, t);
        Vector3 c = Lerp(p2, p3, t);
        Vector3 d = Lerp(a, b, t);
        Vector3 e = Lerp(b, c, t);
        Vector3 pointOnCurve = Lerp(d, e, t);

        return pointOnCurve;
    }
    //Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1)
    //{
    //    return ((1 - t) * p0) + ((t) * p1);
    //}

    //Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    //{
    //    Vector3 pa = BezierCurve(t, p0, p1);
    //    Vector3 pb = BezierCurve(t, p1, p2);
    //    return BezierCurve(t, pa, pb);
    //}
    Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return (1f - t) * a + t * b;
    }
}
