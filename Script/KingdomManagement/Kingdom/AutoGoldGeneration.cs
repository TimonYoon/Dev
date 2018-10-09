using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;
using CodeStage.AntiCheat.ObscuredTypes;

public class AutoGoldGeneration : MonoBehaviour {

    public static AutoGoldGeneration Instance;

    DateTime startTime;

    // 남은 시간
    public float remainTime;

    // 쿨타임 (마다 골드 획득가능)
    public float coolTime = 30f;
    
    // 현재 획득한 골드량
    double currentGoldAmount;

    public GameObject goldIcon;
    public ParticleSystem coinParticle;

    public ParticleSystem particleSystem;
    public Image imageProgress;

    public SoundPlayer soundPlayerTakeCoins;


    public string saveKey { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        while (SceneLobby.Instance == false)
            yield return null;
        LocalSave.RegisterSaveCallBack(OnSave);
        OnLoad();

        TerritoryManager.onAddPlace += UpdatePlaceModify;
        TerritoryManager.onChangedPlaceData += UpdatePlaceModify;



        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        
        imageBGGoldIcon.gameObject.SetActive(currentGoldAmount > 0);
    }
    void OnChangedMenu(LobbyState state)
    {
        coinParticle.gameObject.SetActive(state == LobbyState.Territory);
    }

    void OnLoad()
    {
        if(PlayerPrefs.HasKey("taxCount"))
        {
            taxCount = PlayerPrefs.GetFloat("taxCount");
        }

        if(ObscuredPrefs.HasKey("currentTaxAmount"))
        {
            string data = ObscuredPrefs.GetString("currentTaxAmount");
            double taxAmount = 0;
            double.TryParse(data, out taxAmount);

            Tax(taxAmount);
        }
    }

    void OnSave()
    {
        //Debug.Log("세금 관련 저장");
        ObscuredPrefs.SetString("currentTaxAmount", currentGoldAmount.ToString());
        PlayerPrefs.SetFloat("taxCount", taxCount);

    }

    // 세금 획등량 증가
    double percentIncreaseTaxAmount;

    // 세금 2배 획득 확률
    float probabilityTaxDouble;
    // 세금 3배 획득 확률
    float probabilityTaxTriple;


    void UpdatePlaceModify()
    {
        percentIncreaseTaxAmount = 0;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData data = TerritoryManager.Instance.myPlaceList[i];
            if(data.placeBaseData.type == "Tax")
            {
                double d = 0;
                double.TryParse(data.placeBaseData.formula, out d);
                d *= data.placeLevel;
                percentIncreaseTaxAmount += d;
            }
        }


        probabilityTaxDouble = 0;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData data = TerritoryManager.Instance.myPlaceList[i];
            if (data.placeBaseData.type == "ProbabilityTaxDouble")
            {
                float f = 0;
                float.TryParse(data.placeBaseData.formula, out f);
                f *= data.placeLevel;
                probabilityTaxDouble += f;
            }
        }

        probabilityTaxTriple = 0;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData data = TerritoryManager.Instance.myPlaceList[i];
            if (data.placeBaseData.type == "ProbabilityTaxTriple")
            {
                float f = 0;
                float.TryParse(data.placeBaseData.formula, out f);
                f *= data.placeLevel;
                probabilityTaxTriple += f;
            }
        }
    }

    public Image imageBGGoldIcon;
    public Image imageGoldIcon;
    public List<Sprite> spriteGoldList;

    
    public GameObject coinObjectPrefab;
    List<CoinObject> coinObjectPool = new List<CoinObject>();

    public Transform coinParent;

    /// <summary> 골드 획득 </summary>
    public void TakeTax(Transform target, double taxAmount)
    {
        if (target == null)
            target = coinParent;

        taxAmount *= (1d + (percentIncreaseTaxAmount * 0.01d));

        // 주민 머리위 골드 연출
        CoinObject coin = CreatCoinObject();
        coin.Init(taxAmount, target);
        coin.gameObject.SetActive(true);

        
        
        Tax(taxAmount);
    }

    float normalProbability = 100f;
    float doubleProbability = 10f;
    float tripleProbability = 5f;
    int Probability()
    {
        int result = 1;

        float total = 0;
        float n = normalProbability;
        float d = n + doubleProbability * (1 + (probabilityTaxDouble * 0.01f));
        //Debug.Log("두배 확률" + doubleProvavility * (1 + (probabilityTaxDouble * 0.01f)));
        float t = d + tripleProbability * (1 + (probabilityTaxTriple * 0.01f));
        //Debug.Log("세배 확률" + tripleProvavility * (1 + (probabilityTaxTriple * 0.01f)));
        total = t;
        

        float randomValue = UnityEngine.Random.Range(0, total);

        if(0 <= randomValue && randomValue <= n)
        {
            result = 1;
            Debug.Log("일반 세금");
        }
        else if(n < randomValue && randomValue <= d)
        {
            result = 2;
            Debug.Log("두배 세금");
        }
        else if(d < randomValue && randomValue <= t)
        {
            result = 3;
            Debug.Log("세배 세금");
        }

        return result;
    }

    // to do : 일정량 이상 세금을 모은 후 획득하도록 하기 위한 count
    float taxCount;
    float taxMaxCount = 10;

    void Tax(double taxAmount)
    {
        currentGoldAmount += taxAmount;

        if (taxMaxCount > taxCount)
            taxCount++;

        imageBGGoldIcon.gameObject.SetActive(currentGoldAmount > 0);

        imageGoldIcon.fillAmount = taxCount / taxMaxCount;
        // 골드 획득 연출
        if (coroutineScaleImage != null)
        {
            StopCoroutine(coroutineScaleImage);
            coroutineScaleImage = null;
        }
        coroutineScaleImage = StartCoroutine(ScaleImage());


        if(currentGoldAmount > 0)
        {
            int index = (int)(Math.Floor(Math.Log10(currentGoldAmount)) / 3);

            if (index < spriteGoldList.Count)
            {
                if (imageGoldIcon.sprite != spriteGoldList[index])
                {
                    imageBGGoldIcon.sprite = spriteGoldList[index];
                    imageGoldIcon.sprite = spriteGoldList[index];
                }
            }
            else
            {
                imageGoldIcon.sprite = spriteGoldList[spriteGoldList.Count - 1];
                imageBGGoldIcon.sprite = spriteGoldList[spriteGoldList.Count - 1];
            }
        }
    }
    Coroutine coroutineScaleImage;
    IEnumerator ScaleImage()
    {

        imageBGGoldIcon.transform.localScale = new Vector2(2f, 1.5f);
        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            imageBGGoldIcon.transform.localScale = Vector3.Lerp(imageBGGoldIcon.transform.localScale, Vector3.one, 6f * Time.deltaTime);

            if (imageBGGoldIcon.transform.localScale.x < 0.01f)
                break;

            yield return null;
        }

        imageBGGoldIcon.transform.localScale = Vector3.one;

        coroutineScaleImage = null;
    }

    CoinObject CreatCoinObject()
    {
        CoinObject coin = null;
        for (int i = 0; i < coinObjectPool.Count; i++)
        {
            if(coinObjectPool[i].gameObject.activeSelf == false)
            {
                coin = coinObjectPool[i];
                break;
            }
        }
        if(coin == null)
        {
            GameObject go = Instantiate(coinObjectPrefab);
            go.transform.SetParent(coinParent, false);
            coin = go.GetComponent<CoinObject>();
            coinObjectPool.Add(coin);
        }

        return coin;
    }

    /// <summary> 분할된 금액 </summary>
    public double dividedAmount;

    double coinAmount;

    Coroutine spawnCoroutine;
    public void OnClickGold()
    {
        if(imageGoldIcon.fillAmount < 1)
        {
            UIPopupManager.ShowInstantPopup("세금이 충분히 모이지 않았습니다.");
            return;
        }

        if (coroutineScaleImage != null)
        {
            StopCoroutine(coroutineScaleImage);
            coroutineScaleImage = null;
        }
        coroutineScaleImage = StartCoroutine(ScaleImage());

        if (currentGoldAmount > 0)
        {
            if (spawnCoroutine != null)
                return;
            coinAmount = currentGoldAmount * Probability();
            currentGoldAmount = 0;
            taxCount = 0;
            OnSave();

            imageBGGoldIcon.gameObject.SetActive(false);

            spawnCoroutine = StartCoroutine(Spawn());

            imageBGGoldIcon.sprite = spriteGoldList[0];
            imageGoldIcon.sprite = spriteGoldList[0];

            //particleSystem.gameObject.SetActive(true);

            slot = UIMoney.Instance.shopMoneyGoldText.gameObject.AddComponent<UIMoneySlot>();
            //slot.isUserMoney = true;
            //slot.moneyType = MoneyType.gold;
            money = new MoneyManager.Money();
            MoneyManager.Money m = MoneyManager.GetMoney(MoneyType.gold);
            money.id = m.id;
            money.value =m.value;
            money.type = m.type;

            slot.textValue = UIMoney.Instance.shopMoneyGoldText;
            slot.Init(money);
            CoinObject coin = CreatCoinObject();
            coin.Init(coinAmount, coinParent);
            coin.gameObject.SetActive(true);

            //사운드 재생
            soundPlayerTakeCoins.Play();
        }
        //AutoGoldGenStart();
    }

    static public void ScaleText()
    {
        if (Instance.coroutineScaleText != null)
        {
            Instance.StopCoroutine(Instance.coroutineScaleText);
            Instance.coroutineScaleText = null;
        }

        Instance.coroutineScaleText = Instance.StartCoroutine(Instance.ScaleTextA());
    }

    Coroutine coroutineScaleText = null;
    UIMoneySlot slot;
    MoneyManager.Money money;

    IEnumerator ScaleTextA()
    {
        Text text = UIMoney.Instance.shopMoneyGoldText;// slot.textValue;
        money.value += dividedAmount;
        slot.Init(money);
        text.transform.localScale = new Vector2(1.4f, 1.2f);






        //double d = double.Parse(text.text);
        //////Debug.Log(d.ToString() + " / "+d.ToStringComma() );
        //d += dividedAmount;
        //text.text = d.ToStringComma();
        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, Vector3.one, 6f * Time.deltaTime);

            if (text.transform.localScale.x < 0.01f)
                break;

            yield return null;
        }

        text.transform.localScale = Vector3.one;

        coroutineScaleText = null;
    }
    public GameObject taxObjectPrefab;
    public Transform taxObjectGoldParent;

    List<TaxObjectGold> objectGoldPool = new List<TaxObjectGold>();

    public int spawnCount = 5;
    IEnumerator Spawn()
    {
        
        int count = Mathf.Clamp((int)(coinAmount / 10), 1, 30);
        spawnCount = count;

        dividedAmount = System.Math.Floor(coinAmount / spawnCount);
        for (int i = 0; i < spawnCount; i++)
        {
            TaxObjectGold objectGold = CreatTaxObjectGold();
            objectGold.gameObject.SetActive(true);
        }

        int enableCount = objectGoldPool.Count;
        while(enableCount > 0)
        {
            enableCount = 0;
            for (int i = 0; i < objectGoldPool.Count; i++)
            {
                if(objectGoldPool[i].gameObject.activeSelf)
                {
                    enableCount++;
                }
            }
            yield return null;
        }
        

        User.Tax(coinAmount);

        //MoneyManager.GetMoney(MoneyType.gold).value += coinAmount;
        //실제 서버 통신하는 부분
        //Todo: 일단 막아둠.. 
        //MoneyManager.SendMoneyToServer("gold", coinAmount.ToString());

        Destroy(slot);
        money = null;
        spawnCoroutine = null;


    }

    public float texSpeed = 30;
    public float texSpeed2 =1;
    public float texTime = 1;
    TaxObjectGold CreatTaxObjectGold()
    {
        TaxObjectGold taxObject = null;

        for (int i = 0; i < objectGoldPool.Count; i++)
        {
            if(objectGoldPool[i].gameObject.activeSelf == false)
            {
                taxObject = objectGoldPool[i];
                break;
            }
        }

        if(taxObject == null)
        {
            GameObject go = Instantiate(taxObjectPrefab);
            go.transform.SetParent(taxObjectGoldParent, false);
            taxObject = go.GetComponent<TaxObjectGold>();
            objectGoldPool.Add(taxObject);
        }

        return taxObject;
    }




}
