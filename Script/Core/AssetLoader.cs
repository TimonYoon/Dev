using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using System.IO;
using LitJson;
using UnityEngine.U2D;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class AssetLoader : MonoBehaviour
{
    public static AssetLoader Instance;


    string url
    {
        get
        {
            return "http://cola.funmagic-cdn.com/Project_L_Dev_" + SceneLogin.clientVersion +"/";//+SceneLogin.clientVersion;
        }
    }


    public AssetBundleManifest manifest { get; set; }

    [System.Serializable]
    public struct AssetBundleHash
    {
        public string assetBundle;
        public Hash128 hash;
    }

    public List<AssetBundleHash> assetBundleHashes = new List<AssetBundleHash>();

    void Awake()
    {
        Instance = this;
        SpriteAtlasManager.atlasRequested += RequestAtlas;
        //DontDestroyOnLoad(gameObject);
    }

    void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {
        var sa = Resources.Load<SpriteAtlas>(tag);
        callback(sa);
    }
    public bool isInitialized { get; set; }

    private void Start()
    {
        //StartCoroutine(Initialize());
    }

    public IEnumerator Initialize(bool isForceInit = false)
    {
        Debug.Log("에셋 로더 초기화 시작");
        if (isInitialized && !isForceInit)
            yield break;

        

        isInitialized = false;

#if UNITY_EDITOR
        //Debug.Log("Init Asset loader");
        if (AssetBundleManager.SimulateAssetBundleInEditor)
        {
            AssetBundleManager.SetDevelopmentAssetBundleServer();
            AssetBundleManager.SetSourceAssetBundleDirectory("");
        }
        else
#endif
        {
            //Debug.Log(url + " url 사용");
            AssetBundleManager.SetSourceAssetBundleURL(url);
        }

        var request = AssetBundleManager.Initialize();

        if (request != null)
            yield return StartCoroutine(request);

        AssetBundleManager.logMode = AssetBundleManager.LogMode.JustErrors;

        AssetBundleManifest manifest = AssetBundleManager.m_AssetBundleManifest;
        if (!manifest)
        {
            isInitialized = true;
            //Debug.Log("에셋 로더 초기화 결과 : " + isInitialized);
            yield break;
        }

        string[] ss = manifest.GetAllAssetBundles();

        Debug.Log("Assetbundle count: " + ss.Length);
        assetBundleHashes.Clear();
        for (int i = 0; i < ss.Length; i++)
        {
            //Debug.Log(ss[i] + ", " + manifest.GetAssetBundleHash(ss[i]).ToString());

            AssetBundleHash a = new AssetBundleHash();
            a.assetBundle = ss[i];
            a.hash = manifest.GetAssetBundleHash(ss[i]);
            assetBundleHashes.Add(a);
        }

        isInitialized = true;

        //Debug.Log("에셋 로더 초기화2 : " + isInitialized);
    }

    static public GameObject InstantiateGameObject(string assetBundleName, string assetName)
    {
        //GameObject prefab = null;

        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            return null;
        }

        if (!Instance.isInitialized)
            return null;

            Instance.StartCoroutine(Instance.Initialize());

        string errorString;
        LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out errorString);
        //Debug.Log(errorString);
        if (loadedAssetBundle != null)
        {
            Object request = loadedAssetBundle.m_AssetBundle.LoadAsset(assetName, typeof(GameObject));

            GameObject go = Instantiate(request as GameObject);

            return go;
        }

        //return null;

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));

        if (r == null)
            return null;

        GameObject obj = r.GetAsset<GameObject>();
        
        return Instantiate(obj);        
    }

    /// <summary>
    /// 어셋번들에서 게임오브젝트(프리팹) 불러오기
    /// </summary>
    /// <param name="assetBundleName">어셋번들 이름</param>
    /// <param name="assetName">프리팹 이름</param>
    /// <param name="result">Instantiate한 게임오브젝트 반환함. 람다식으로 잘 참조해서 쓸 것</param>
    /// <returns></returns>
    public IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName, System.Action<GameObject> result)
    {
        //GameObject prefab = null;

        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        if (!isInitialized)
            yield return StartCoroutine(Initialize());

        string errorString;
        LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out errorString);
        //Debug.Log(errorString);
        if (loadedAssetBundle != null)
        {
            AssetBundleRequest request = loadedAssetBundle.m_AssetBundle.LoadAssetAsync(assetName, typeof(GameObject));
            while (!request.isDone)
                yield return null;

            GameObject go = Instantiate(request.asset as GameObject);

            result(go);
            yield break;
        }

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));


        if (r == null)
        {
            result(null);
            yield break;
        }

        yield return StartCoroutine(r);

        GameObject obj = r.GetAsset<GameObject>();

        //yield return StartCoroutine(LoadGameObjectAsync(assetBundleName, assetName, x => prefab = x));

        if (obj != null)
            result(GameObject.Instantiate(obj));
        else
            result(null);
    }

    public IEnumerator LoadGameObjectAsync(string assetBundleName, string assetName, System.Action<GameObject> result)
    {
        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        if (!isInitialized)
            yield return StartCoroutine(Initialize());

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));

        if (r == null)
        {
            result(null);
            yield break;
        }

        yield return StartCoroutine(r);
        
        GameObject obj = r.GetAsset<GameObject>();

        result(obj);
    }

    public IEnumerator LoadTexture(string assetBundleName, string assetName, System.Action<Texture> result)
    {
        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        if (!isInitialized)
            yield return StartCoroutine(Initialize());

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Texture));

        if (r == null)
        {
            result(null);
            yield break;
        }

        yield return StartCoroutine(r);

        Texture texture = r.GetAsset<Texture>();

        //Debug.Log(texture);

        if (!texture)
        {
            result(null);
            yield break;
        }

        result(texture);
    }

    static public Dictionary<string, UnityEngine.U2D.SpriteAtlas> cachedAtlasDic = new Dictionary<string, UnityEngine.U2D.SpriteAtlas>();

    static public void AssignImage(Image image, string assetBundleName, string atlasName, string spriteName, System.Action<string> result = null)
    {
        Instance.StartCoroutine(Instance.AssignImageA(image, assetBundleName, atlasName, spriteName, result));
    }

    IEnumerator AssignImageA(Image image, string assetBundleName, string atlasName, string spriteName, System.Action<string> result)
    {   
        string resultString = "Finish";

        Sprite sprite = null;
        if(!string.IsNullOrEmpty(spriteName))
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadSprite(assetBundleName, atlasName, spriteName, x => sprite = x));
        }
  

        //이미지 설정 잘못 된 경우
        if (!sprite)
        {
            resultString = "Error: Cannot find sprite [" + spriteName + "] in atlas [" + atlasName + "]";

            yield return StartCoroutine(AssetLoader.Instance.LoadSprite("sprite/hero", "Atlas_HeroImage", "NoImage", x => sprite = x));
        }

        image.preserveAspect = true;
        sprite.name = spriteName;
        image.sprite = sprite;

        //결과 콜백
        if(result != null)
            result(resultString);
    }

    public IEnumerator LoadSprite(string assetBundleName, string atlasName, string spriteName, System.Action<Sprite> result)
    {
        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(spriteName) || string.IsNullOrEmpty(atlasName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        if (!isInitialized)
            yield return StartCoroutine(Initialize());

        SpriteAtlas atlas = null;

        if(cachedAtlasDic.ContainsKey(atlasName))
            atlas = cachedAtlasDic[atlasName];

        if (!atlas)
        {
            
            AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, atlasName, typeof(SpriteAtlas));


            if (r == null)
            {
                result(null);
                yield break;
            }

            yield return StartCoroutine(r);

            //Debug.Log(r.IsDone());
            
            atlas = r.GetAsset<SpriteAtlas>();
            
            if (atlas != null)
            {
                if (!cachedAtlasDic.ContainsKey(atlasName))
                    cachedAtlasDic.Add(atlasName, atlas);

                //Debug.Log("스프라이트 수량 " + atlas.spriteCount);
            }   

        }

        Sprite sprite = null;
        if (atlas != null)
            sprite = atlas.GetSprite(spriteName);

        //SpriteAtlasManager.atlasRequested += 

        //
        //AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromFileAsync(path);
        //yield return bundleReq;

        //AssetBundle bundle = bundleReq.assetBundle;

        //if (bundle == null)
        //{
        //    Debug.LogError("Failed to load Asset Bundle at " + path);
        //    yield break;
        //}

        //AssetBundleRequest atlasReq = bundle.LoadAssetAsync<SpriteAtlas>(atlasName);
        //yield return atlasReq;
        //SpriteAtlas atlas = (SpriteAtlas)atlasReq.asset as SpriteAtlas;
        //Sprite[] sprites = new Sprite[
        //callback(bundle, );

        ////
        //AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Sprite));


        //yield return StartCoroutine(r);

        //Sprite sprite = r.GetAsset<Sprite>();

        //Debug.Log(texture);

        if (!sprite)
        {
            result(null);
            yield break;
        }

        result(sprite);
    }

    public static IEnumerator LoadJsonDataG(string assetBundleName, string assetName, System.Action<JsonData> result)
    {
        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        //if (!isInitialized)
        //    yield return StartCoroutine(Initialize());

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Object));

        if (r == null)
        {
            result(null);
            yield break;
        }

        //yield return StartCoroutine(r);

        TextAsset txt = r.GetAsset<TextAsset>();

        if (!txt)
        {
            result(null);
            yield break;
        }

        JsonReader jReader = new JsonReader(txt.text);
        JsonData jData = JsonMapper.ToObject(jReader);

        result(jData);
    }

    static List<TextAsset> textAssetList = new List<TextAsset>();
    static public Dictionary<string, JsonData> preCachedJsonData = new Dictionary<string, JsonData>();

    static public IEnumerator LoadJsonData(string assetBundleName, string assetName, System.Action<JsonData> result)
    {
        if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
        {
            Debug.LogWarning("assetBundleName or assetName is empty");
            result(null);
            yield break;
        }

        if (!Instance.isInitialized)
            yield return Instance.StartCoroutine(Instance.Initialize());

        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(Object));

        if (r == null)
        {
            result(null);
            yield break;
        }

        yield return Instance.StartCoroutine(r);

        TextAsset txt = r.GetAsset<TextAsset>();

        if (!txt)
        {
            result(null);
            yield break;
        }

        JsonReader jReader = new JsonReader(txt.text);
        JsonData jData = JsonMapper.ToObject(jReader);

        result(jData);
    }

    public IEnumerator DownLoadAsset(string assetBundleName, Hash128 hash)
    {
        bool isFinsish = false;

        while (!isFinsish)
        {
            yield return null;
            
            bool isCached = Caching.IsVersionCached(assetBundleName, hash);
            Debug.Log("캐싱된 데이터 인가? : " + isCached);
            if (isCached)
                AssetBundleManager.UnloadAssetBundle(assetBundleName);
            //break;

            //Todo:인터넷 연결 상태 체크


            //yield return StartCoroutine(WebServerConnectManager.Instance.WaitToConnectInternet());

            WWW download = null;
 #if UNITY_EDITOR
            download = WWW.LoadFromCacheOrDownload(url + "Android/" + assetBundleName, hash, 0);
#endif


#if UNITY_ANDROID
            download = WWW.LoadFromCacheOrDownload(url + "Android/" + assetBundleName, hash, 0);
            //UnityEngine.Networking.UnityWebRequest.
#endif

#if UNITY_IOS
            download = WWW.LoadFromCacheOrDownload(url + "iOS/" + assetBundleName, hash, 0);
#endif

            if (download == null)
            {
                Debug.LogError("download null");
                download.Dispose();
                download = null;
                continue;
            }

            if (!string.IsNullOrEmpty(download.error))
            {
                Debug.LogError(download.error);
                download.Dispose();
                download = null;
                continue;
            }


#if UNITY_EDITOR
            Debug.Log("download : " + assetBundleName);
#endif
            float startTime = Time.time;
            float lastProgress = 0f;
            bool isTimeOut = false;
            while (!download.isDone && !isTimeOut)
            {
                if (lastProgress != download.progress)
                    startTime = Time.time;

                isTimeOut = Time.time > startTime + 30f && lastProgress == download.progress;

                yield return null;

                //Debug.Log(download.isDone + ", " + isTimeOut + ", " + lastProgress + ", " + download.progress);
                lastProgress = download.progress;
            }

            if (!download.isDone)
            {
                Debug.LogError(assetBundleName + "download is not done : " + assetBundleName);
                download.Dispose();
                download = null;
                break;
            }


            if (download.assetBundle == null)
            {
                Debug.LogError("download assetbundle : null");
                download.Dispose();
                download = null;
                break;
            }

            download.assetBundle.Unload(false);

            download.Dispose();
            download = null;
            isFinsish = true;

            yield break;
        }


    }

    public long GetDownloadSizeOfBundles(List<AssetLoader.AssetBundleHash> assetBundlesToDownload)
    {
        int size = 0;

        for (int i = 0; i < assetBundlesToDownload.Count; i++)
        {
#if UNITY_ANDROID
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url + "Android/" + assetBundlesToDownload[i].assetBundle);
#endif
#if UNITY_IOS
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url + "iOS/" + assetBundlesToDownload[i].assetBundle);
#endif
            req.Method = "HEAD";
            req.Timeout = 10000;

            int ContentLength;
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    size += ContentLength;
                    //Debug.Log(assetBundlesToDownload[i].assetBundle + ", size: " + ContentLength);
                }

                resp.Close();
            }
            req.Abort();
            req = null;
            //System.GC.Collect();
        }

        return size;
    }
    //float lastCheckTime = 0f;
    public IEnumerator GetDownloadSizeOfBundlesA(List<AssetLoader.AssetBundleHash> assetBundlesToDownload, System.Action<long> result)
    {
        int size = 0;
        int count = assetBundlesToDownload.Count;
        //SceneLogIn.Instance.labelCheckAsset.text = "Calculate download size";
        for (int i = 0; i < count; i++)
        {
#if UNITY_ANDROID
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url + "Android/" + assetBundlesToDownload[i].assetBundle);
#endif
#if UNITY_IOS
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url + "iOS/" + assetBundlesToDownload[i].assetBundle);
#endif
            req.Method = "HEAD";
            req.Timeout = 10000;

            int ContentLength;
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    size += ContentLength;
                    //Debug.Log(assetBundlesToDownload[i].assetBundle + ", size: " + ContentLength);
                }

                resp.Close();
            }
            req.Abort();
            req = null;
            //System.GC.Collect();

            //SceneLogIn.Instance.labelCheckAsset.text = "Calculate download size";

            //if (Time.time > lastCheckTime + 1f)
            //{
            //    SceneLogIn.Instance.labelCheckAsset.text = "Calculate download size (" + i + "/" + count + ")";
            //    SceneLogIn.Instance.labelCheckAsset.Update();

            //    lastCheckTime = Time.time;
            //    yield return new WaitForSeconds(0.1f);
            //}   
        }

        result(size);

        //return size;

        yield break;
    }

    public IEnumerator LoadLevelAsync(string bundleName, string sceneName, bool isAdditive)
    {
        AssetBundleLoadOperation async = AssetBundleManager.LoadLevelAsync(bundleName, sceneName, isAdditive);

        while (!async.IsDone())
            yield return null;

        yield break;
    }
}
