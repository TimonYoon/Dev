using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi;

public class GoogleManager : MonoBehaviour {

    public static GoogleManager Instance;

    public string googleID = null;

    public bool isInitialized { get; private set; }

#if UNITY_ANDROID

    static PlayGamesClientConfiguration _GPGConfig;

#endif

    void Awake()
    {
        Instance = this;

        isInitialized = false;

        DontDestroyOnLoad(this);
    }

    void Start()
    {

//#if UNITY_ANDROID

        // Create client configuration
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();

        // Enable debugging output (recommended)
        PlayGamesPlatform.DebugLogEnabled = true;

        // Initialize and activate the platform
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

//#endif        
	}

    public void LogIn()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                googleID = Social.localUser.id;
                Debug.Log("google login success");
                Debug.Log("google id : " + Social.localUser.id);
                Debug.Log("google name : " + Social.localUser.userName);
                isInitialized = true;
            }
            else
            {
                Debug.Log("google login failed : " + Social.Active);
                Debug.Log("google id : " + Social.localUser.id);
            }
            GooglePlayGames.OurUtils.PlayGamesHelperObject.RunOnGameThread(() => {
                Debug.Log("email : " + ((PlayGamesLocalUser)Social.localUser).Email);
            });

        });
    }

    public void LogOut()
    {
        googleID = null;
        ((PlayGamesPlatform)Social.Active).SignOut();
    }
	
}
