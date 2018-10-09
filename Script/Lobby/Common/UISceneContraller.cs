using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> 로비 씬에서 각 메뉴 버튼마다 원하는 씬을 호출하기 위해 만들어졌다. 각 버튼마다 넣어서 사용하면 된다. </summary>
public class UISceneContraller : MonoBehaviour
{

    public delegate void SceneContrallerCallback(LobbyState state,SubMenuState subMenuState);
    public static SceneContrallerCallback onSceneContrallerCallBack;

    [SerializeField]
    string assetBundle;

    [SerializeField]
    string sceneName;

    [SerializeField]
    bool isAdditive = true;

    [SerializeField]
    LobbyState state;

    [SerializeField]
    SubMenuState subMenuState;

    Coroutine coroutine;

    Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnClick()
    {
        if (toggle != null)
        {
            if (toggle.isOn == false)
            {
                //Debug.Log(state.ToString() + "토글 꺼짐!");
                return;
            }
        }
        //Debug.Log(state.ToString() + " / " + toggle.isOn);

        if (coroutine != null)
            return;

        coroutine = StartCoroutine(SceneLoadCoroutine(assetBundle, sceneName, isAdditive));

    }


    IEnumerator SceneLoadCoroutine(
        string assetBundle,
        string sceneName,        
        bool isAdditive = true)
    {
        
        //씬 불러옴
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync(assetBundle, sceneName, isAdditive));
            //SceneLobby.Instance.SceneChange(state);

            scene = SceneManager.GetSceneByName(sceneName);

            while (!scene.isLoaded)
                yield return null;
        }
        if (onSceneContrallerCallBack != null)
            onSceneContrallerCallBack(state, subMenuState);
        

        coroutine = null;
    }

}
