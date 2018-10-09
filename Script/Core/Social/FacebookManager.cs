using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{

    public static FacebookManager Instance;

    public string UserID = null;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this);

        if (!FB.IsInitialized)
            FB.Init(Initialize, OnHideUnity);
    }

    void Initialize()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the facebook SDK");
        }
    }

    public void AuthCallbackLogIn(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            var aToken = AccessToken.CurrentAccessToken;

            Debug.Log(aToken.UserId);

            foreach (string param in aToken.Permissions)
            {
                Debug.Log(param);
            }
        }
        else
        {
            Debug.Log("user cancelled login");
        }
    }
    void SetInit()
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("FB is logged IN");
        }
        else
        {
            Debug.Log("FB is no logged in");
        }

        DealWithFBMenus(FB.IsLoggedIn);
    }

    void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void LogIn()
    {
        List<string> permissions = new List<string>();
        permissions.Add("public_profile");
        permissions.Add("email");
        permissions.Add("user_friends");

        FB.LogInWithReadPermissions(permissions, AuthCallBack);

    }

    void AuthCallBack(IResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("FB is logged in");

                var aToken = AccessToken.CurrentAccessToken;
                Debug.Log(aToken.UserId);
                
                foreach (string perm in aToken.Permissions)
                {
                    Debug.Log(perm);
                }
                
            }
            else
            {
                Debug.Log("FB is not logged in");
            }

            DealWithFBMenus(FB.IsLoggedIn);
        }
    }

    void DealWithFBMenus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, DisplayUsername);
            //FB.API("/me/picture?type=square&height=127&width=128", HttpMethod.GET, DisplayProfilePic);
        }
        else
        {

        }
    }

    public bool isInitialized { get; private set; }
    void DisplayUsername(IResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            UserID = result.ResultDictionary["first_name"].ToString();
            //Debug.Log(result.ResultDictionary["first_name"]);
            isInitialized = true;
            Debug.Log(UserID);
        }
    }

    public void LogOut()
    {
        UserID = null;
        FB.LogOut();
    }


}
