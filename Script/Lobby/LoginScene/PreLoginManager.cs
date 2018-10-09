using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreLoginManager : MonoBehaviour {
    
	IEnumerator Start ()
    {
        while (!UILoginManager.isInitailized)
            yield return null;
        
        yield return StartCoroutine(InitLocalizationManager());

        yield return StartCoroutine(InitOptionManager());
    

        InitLoginScene();

    }
    
    IEnumerator InitLocalizationManager()
    {
        StartCoroutine(LocalizationManager.Init());

        while (!LocalizationManager.isInitializedPreLocalizingData)
            yield return null;
    }

    IEnumerator InitOptionManager()
    {
        GameObject go = new GameObject("OptionManager");
        OptionManager option = go.AddComponent<OptionManager>();
        DontDestroyOnLoad(go);

        while (OptionManager.isInitialized == false)
            yield return null;
    }

    void InitLoginScene()
    {
        SceneManager.LoadSceneAsync("Login", LoadSceneMode.Additive);
    }
}
