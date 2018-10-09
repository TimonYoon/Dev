using UnityEngine;
using System.Collections;

public class SceneTest : MonoBehaviour {
    	
	void Awake ()
    {
        if(!UserDataManager.Instance)
            UnityEngine.SceneManagement.SceneManager.LoadScene("PreLogin");

    }
}
