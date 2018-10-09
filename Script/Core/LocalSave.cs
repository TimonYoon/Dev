using UnityEngine;
using System.Collections;

public class LocalSave : MonoBehaviour {

    public static SimpleDelegate onSave;

    static public void RegisterSaveCallBack(SimpleDelegate reciever)
    {
        onSave += reciever;
    }
    float saveTime = 60f;
    float startTime = 0;
    private void Awake()
    {
        startTime = Time.unscaledTime;
    }
    void Update()
    {
        if (Time.unscaledTime - startTime >= saveTime)
        {
            // 세이브 시간마다 콜백 날림
            startTime = Time.unscaledTime;
            //Debug.Log("저장 콜백 보냄");
            if (onSave != null)
                onSave();
        }
    }
}
