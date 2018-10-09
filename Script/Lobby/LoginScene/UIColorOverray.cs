using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIColorOverray : MonoBehaviour {

    //나무관련
    Color dayTreeColor = new Color(53 / 255.0f, 83 / 255.0f, 52 / 255.0f);
    Color nightTreeColor = new Color(56 / 255.0f, 52 / 255.0f, 55 / 255.0f);
    Color treeColor;

    //러프 시간관련
    float term = 0f;
    float progress = 0f;

    //러프된 색상
    public static Color lerpedTreeColor = new Color(53 / 255.0f, 83 / 255.0f, 52 / 255.0f);
    public static Color lerpedCloudColor = new Color(1f, 1f, 1f);

    //구름 관련
    Color dayCloudColor = new Color(1f, 1f, 1f);
    Color nightCloudColor = new Color(255 / 255.0f, 237 / 255.0f, 226 / 255.0f);
    Color cloudColor;

    IEnumerator ColorOverray()
    {
        treeColor = lerpedTreeColor;
        cloudColor = lerpedCloudColor;
        float t = 60f + Time.unscaledTime;
        term = 0f;
        while (Time.unscaledTime <= t)
        {
            term = t - Time.unscaledTime;
            progress = (60f - term) / 60f;
            lerpedTreeColor = Color.Lerp(treeColor, destTreeColor, progress);
            lerpedCloudColor = Color.Lerp(cloudColor, destCloudColor, progress);
            yield return null;
        }

        coColorOverray = null;
        yield break;
    }


    Coroutine coColorOverray = null;
    Color destTreeColor;
    Color destCloudColor;

    //애니메이션 이벤트 함수
    public void TurnNightColor()
    {
        destTreeColor = nightTreeColor;
        destCloudColor = nightCloudColor;

        if (coColorOverray != null)
            StopCoroutine(coColorOverray);

        coColorOverray = StartCoroutine(ColorOverray());
    }
    public void TurnDayColor()
    {
        destTreeColor = dayTreeColor;
        destCloudColor = dayCloudColor;

        if (coColorOverray != null)
            StopCoroutine(coColorOverray);

        coColorOverray = StartCoroutine(ColorOverray());
    }
}
