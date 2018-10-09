using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

/// <summary> 로그인 화면 연출 </summary>
public class UILoginManager : MonoBehaviour
{
    static UILoginManager Instance;
    
    public Image backgroundPanel;
    public RectTransform ground;
    public Image groundImage;
    public float moveOffset = 0.01f;

    public static bool isInitailized = false;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {        
        yield return new WaitForSeconds(2f);

        StartCoroutine(LoadingManager.FadeInScreen());

        StartCoroutine(MoveGround());

        StartCoroutine(FirtstSpawneObject(trees, treeSpawner1, treeSpawner2, closeMinTreeSpeed, closeMaxTreeSpeed, farMinTreeSpeed, farMaxTreeSpeed, firstTreeSpawnTime, treeDestroyTime));

        StartCoroutine(FirtstSpawneObject(clouds, cloudSpawner1, cloudSpawner2, closeMinCloudSpeed, closeMaxCloudSpeed, farMinCloudSpeed, farMaxCloudSpeed, firstcloudSpawnTime, cloudDestroyTime));

        StartCoroutine(SpawneObject(trees, treeSpawner1, treeSpawner2, closeMinTreeSpeed, closeMaxTreeSpeed, farMinTreeSpeed, farMaxTreeSpeed, closeTreeRespawnTime, farTreeRespawnTime, treeDestroyTime));

        StartCoroutine(SpawneObject(clouds, cloudSpawner1, cloudSpawner2, closeMinCloudSpeed, closeMaxCloudSpeed, farMinCloudSpeed, farMaxCloudSpeed, closeCloudRespawnTime, farCloudRespawnTime, cloudDestroyTime));

        //StartCoroutine(RunHeroes());
        StartCoroutine(MoveHeroes());

        isInitailized = true;

        while (SceneLogin.Instance == null)
            yield return null;

        SceneLogin.Instance.onFadeOutStart += FadeOutStart;
    }
    
    IEnumerator MoveGround()
    {
        Vector2 offset = new Vector2(0.0f, 0.0f);

        float i = 0f;
        
        while (true)
        {
            offset = new Vector2(i, 0.0f);

            groundImage.material.SetTextureOffset("_MainTex", offset);

            yield return new WaitForEndOfFrame();

            if(i == 1.0f)
            {
                i = 0f;
            }
            else
            {
                i += moveOffset;
            }
        }

    }

    [Header("나무 관련")]
    public Rigidbody2D[] trees = new Rigidbody2D[2]; //먼 나무와 가까운 나무의 rigidbody를 담을 레퍼런스
    public float firstTreeSpawnTime;
    public float closeTreeRespawnTime; //가까운 나무 리스폰 타임
    public float farTreeRespawnTime; //먼 나무 리스폰 타임
    //public float minTimeBetweenTreeSpawn; //랜덤으로 생성하기 위한 시간의 min값
    //public float maxTimeBetweenTreeSpawn; //랜덤으로 생성하기 위한 시간의 max값
    public float closeMinTreeSpeed; //가까운 나무의 스피드의 min값
    public float closeMaxTreeSpeed; //가까운 나무의 스피드의 max값
    public float farMinTreeSpeed; //먼 나무의 스피드의 min값
    public float farMaxTreeSpeed; //먼 나무의 스피드의 max값
    public float treeDestroyTime; //나무 소멸까지 시간

    public RectTransform treeSpawner1;
    public RectTransform treeSpawner2;

    [Header("구름 관련")]
    public Rigidbody2D[] clouds;
    public float firstcloudSpawnTime;
    public float closeCloudRespawnTime; //가까운 구름 리스폰 타임
    public float farCloudRespawnTime; //먼 나무 리스폰 타임
    //public float minTimeBetweenCloudSpawn; //랜덤으로 생성하기 위한 시간의 min값
    //public float maxTimeBetweenCloudSpawn; //랜덤으로 생성하기 위한 시간의 max값
    public float cloudMinPosY; //생성 좌표 Y값 랜덤 범위 수치
    public float cloudMaxPosY; //생성 좌표 Y값 랜덤 범위 수치
    public float closeMinCloudSpeed; //가까운 구름의 스피드의 min값
    public float closeMaxCloudSpeed; //가까운 구름의 스피드의 max값
    public float farMinCloudSpeed; //먼 구름의 스피드의 min값
    public float farMaxCloudSpeed; //먼 구름의 스피드의 max값
    public float cloudDestroyTime; //구름 소멸까지 시간
    public RectTransform cloudSpawner1;
    public RectTransform cloudSpawner2;

    
    float time;
    IEnumerator SpawneObject(Rigidbody2D[] objects, RectTransform spawner1, RectTransform spawner2, float closeMinSpeed, float closeMaxSpeed, float farMinSpeed, float farMaxSpeed, float closeRespawnTime, float farRespawnTime, float destroyTime)
    {
        
        float closeTime = Time.unscaledTime + closeRespawnTime;
        float farTime = Time.unscaledTime + farRespawnTime;

        while (true)
        {
            if (closeTime <= Time.unscaledTime)
            {
                closeTime = Time.unscaledTime + closeRespawnTime;


                Rigidbody2D instance = Instantiate(objects[0], transform.position, Quaternion.identity) as Rigidbody2D;
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (objects == clouds)
                {
                    rect.localScale = new Vector3(100, 100, 1);
                    float posY = Random.Range(cloudMinPosY, cloudMaxPosY);
                    rect.localPosition = new Vector3(0, posY);
                }
                else
                {
                    rect.localScale = new Vector3(1, 1, 1);
                    rect.localPosition = new Vector3(0, 0);
                }

                float speed = 0;
                
                instance.transform.SetParent(spawner1, false);
                speed = Random.Range(closeMinSpeed, closeMaxSpeed);

                instance.velocity = new Vector2(-1 * speed * Time.unscaledDeltaTime, 0f);

                Destroy(instance.gameObject, destroyTime);
                
            }

            if (farTime <= Time.time)
            {
                farTime = Time.time + farRespawnTime;


                Rigidbody2D instance = Instantiate(objects[1], transform.position, Quaternion.identity) as Rigidbody2D;
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (objects == clouds)
                {
                    rect.localScale = new Vector3(100, 100, 1);
                    float posY = Random.Range(cloudMinPosY, cloudMaxPosY);
                    rect.localPosition = new Vector3(0, posY);
                }
                else
                {
                    rect.localScale = new Vector3(1, 1, 1);
                    rect.localPosition = new Vector3(0, 0);
                }


                float speed = 0;
                
                instance.transform.SetParent(spawner2, false);
                speed = Random.Range(farMinSpeed, farMaxSpeed);
                
                instance.velocity = new Vector2(-1 * speed * Time.unscaledDeltaTime, 0f);

                Destroy(instance.gameObject, destroyTime);
            }

            yield return null;
        }
       
    }

    IEnumerator FirtstSpawneObject(Rigidbody2D[] objects, RectTransform spawner1, RectTransform spawner2, float closeMinSpeed, float closeMaxSpeed, float farMinSpeed, float farMaxSpeed, float firstSpawnTime, float destroyTime)
    {
        int num = Random.Range(0, 2);

        yield return new WaitForSeconds(firstSpawnTime);

        Rigidbody2D instance = Instantiate(objects[num], transform.position, Quaternion.identity) as Rigidbody2D;
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (objects == clouds)
        {
            rect.localScale = new Vector3(100, 100, 1);
            float posY = Random.Range(cloudMinPosY, cloudMaxPosY);
            rect.localPosition = new Vector3(0, posY);
        }
        else
        {
            rect.localScale = new Vector3(1, 1, 1);
            rect.localPosition = new Vector3(0, 0);
        }


        float speed = 0;
        if (num == 0)
        {
            instance.transform.SetParent(spawner1, false);
            speed = Random.Range(closeMinSpeed, closeMaxSpeed);

        }
        else
        {
            instance.transform.SetParent(spawner2, false);
            speed = Random.Range(farMinSpeed, farMaxSpeed);

        }

        instance.velocity = new Vector2(-1 * speed * Time.unscaledDeltaTime, 0f);

        Destroy(instance.gameObject, destroyTime);


        yield return null;

    }

    [Header("영웅 관련")]
    public Transform heroSpawner;
    public GameObject[] heroPrefabs;
    public List<MoveHero> heroes = new List<MoveHero>();
    public float moveTermTime;
    int num = 0;
    IEnumerator MoveHeroes()
    {
        float startTime = Time.unscaledTime + moveTermTime;

        while (true)
        {
            if (startTime <= Time.unscaledTime)
            {
                startTime = Time.unscaledTime + moveTermTime;

                int i = 0;

                while(true)
                {
                    i = Random.Range(0, heroes.Count);

                    if (num == i)
                        continue;
                    else
                        break;
                }

                num = i;

                if (heroes[num] == null)
                    break;

                if (heroes[num].isStart != true)
                {
                    //heroes[num].gameObject.SetActive(true);
                    heroes[num].isStart = true;
                    heroes[num].Run();
                }
                    

            }

            yield return null;
        }
    }

    //IEnumerator RunHeroes()
    //{
    //    int i = Random.Range(0, heroPrefabs.Length);
    //    GameObject go = null;
    //    if(num == i)
    //    {
    //        StartCoroutine(RunHeroes());
    //        yield break;
    //    }
    //    else
    //    {
    //        num = i;
    //        go = Instantiate(heroPrefabs[num], heroSpawner.transform);
    //    }

    //    if(num == 0 || num ==3 || num == 4 || num == 15)
    //    {
    //        go.transform.localPosition = new Vector3(0, 100f);
    //    }

    //    go.transform.localScale = new Vector3(32f, 32f);
    //    if(num < 7)
    //    {
    //        go.AddComponent<MoveHero>().speed = 1.5f;
    //    }
    //    else if(num < 14)
    //    {
    //        go.AddComponent<MoveHero>().speed = 1f;
    //    }
    //    else
    //    {
    //        go.AddComponent<MoveHero>().speed = 0.5f;
    //    }


    //    yield return new WaitForSeconds(5.0f);

    //    StartCoroutine(RunHeroes());
    //    yield return null;
    //}

    bool _isFinished = false;
    public static bool isFinished
    {
        get { return Instance._isFinished; }
        private set
        {
            Instance._isFinished = value;
        }
    }
    void FadeOutStart()
    {
        isFinished = false;
        StartCoroutine(FadeOutBackGround());
    }

    
    IEnumerator FadeOutBackGround()
    {
        //yield return new WaitForSeconds(1f);

        //float a = 0f;
        float time = 1f;
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < time)
        {
            float e = Time.unscaledTime - startTime;
            float a = e * 1f / time;
            //Color color = backgroundPanel.color;
            //color.a = a;
            backgroundPanel.color = new Color(0f, 0f, 0f, a);// color;

            //a += Time.unscaledDeltaTime;

            yield return null;
        }

        isFinished = true;
    }
}

