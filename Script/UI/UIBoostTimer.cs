using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBoostTimer : MonoBehaviour {

    public static UIBoostTimer Instance;

    [SerializeField]
    GameObject boostTimerPanel;

    [SerializeField]
    GameObject boostSpeedDouble;

    [SerializeField]
    GameObject boostSpeedTriple;

    [SerializeField]
    Text boostRemainTime;

    bool isActive = false;

    Color empty = new Color(1, 1, 1, 0);
    Color origin;

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start () {
        origin = boostRemainTime.color;
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        StartBoostTimer();

    }

    void OnChangedMenu(LobbyState menu)
    {
        if((menu == LobbyState.Territory || menu == LobbyState.Battle) && isActive)
        {
            boostTimerPanel.SetActive(true);
        }
        else
        {
            boostTimerPanel.SetActive(false);
        }
    }

    Coroutine timerCoroutine = null;
    public void StartBoostTimer()
    {
        if (!OptionManager.Instance)
            return;
        

        if (OptionManager.boostSpeed > 1f)
        {
            ActiveTimer();
            boostSpeedDouble.SetActive(false);
            boostSpeedTriple.SetActive(false);

            if (OptionManager.boostSpeed == 2f)
            {
                boostSpeedDouble.SetActive(true);
            }
            else if (OptionManager.boostSpeed >= 3f)
            {
                boostSpeedTriple.SetActive(true);
            }

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                boostRemainTime.color = origin;
            }
            if(blinkerCoroutine != null)
            {
                StopCoroutine(blinkerCoroutine);
            }

            timerCoroutine = StartCoroutine(BoostTimer());
            isActive = true;
        }
        else
        {
            boostTimerPanel.SetActive(false);
        }
    }

    void ActiveTimer()
    {
        if (SceneLobby.currentState != LobbyState.Shop)
            boostTimerPanel.SetActive(true);
    }
    
    bool isBlink = false;
	IEnumerator BoostTimer()
    {
        string hour = "";
        string min = "";
        string sec = "";

        while (OptionManager.boostRemainTime > 0)
        {
            float remain = OptionManager.boostRemainTime;
            boostRemainTime.text = remain.ToStringTime();
            yield return null;
            //yield return new WaitForSecondsRealtime(0.25f);

            //if (OptionManager.boostRemainTime >= 86400f)
            //{
            //    if ((OptionManager.boostRemainTime / 86400) < 10)
            //    {
            //        min = Mathf.Floor(OptionManager.boostRemainTime / 86400).ToString("0");
            //    }
            //    else
            //    {
            //        min = Mathf.Floor(OptionManager.boostRemainTime / 86400).ToString("00");
            //    }

            //    boostRemainTime.text = min + " Days";
            //}
            //else if (OptionManager.boostRemainTime < 86400f && OptionManager.boostRemainTime >= 3600f)
            //{
            //    hour = Mathf.Floor(OptionManager.boostRemainTime / 3600).ToString("00");
            //    min = "";
            //    sec = "";

            //    boostRemainTime.text = hour + " Hours";
            //}
            //else
            //{
            //    min = Mathf.Floor(OptionManager.boostRemainTime / 60).ToString("00");
            //    sec = Mathf.Floor(OptionManager.boostRemainTime % 60).ToString("00");

            //    boostRemainTime.text = min + ":" + sec;
            //}

            if (OptionManager.boostRemainTime < 30f && !isBlink)
            {
                isBlink = true;
                blinkerCoroutine = StartCoroutine(TimerBlinker(OptionManager.boostRemainTime));
            }
        }

        boostTimerPanel.SetActive(false);
        isActive = false;
        yield return null;

    }

    Coroutine blinkerCoroutine = null;
    IEnumerator TimerBlinker(float remainTime)
    {
        while(remainTime > 0)
        {
            boostRemainTime.color = empty;
            yield return new WaitForSecondsRealtime(0.5f);

            boostRemainTime.color = origin;
            yield return new WaitForSecondsRealtime(0.5f);
            remainTime -= 1f;
        }
        boostRemainTime.color = origin;
        isBlink = false;
        blinkerCoroutine = null;
    }
}
