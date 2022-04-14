using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Login : MonoBehaviour
{

    public GameObject login;
    public Button botao;

    private const string _clientID = "irSydC3PUJ7fBgJ8AlwfQdqS8NXpxVuYbtrVtSWh";
    private const string CallBackUrl = "http://educar.mobile.com/auth";

    public string deepLinkURL;


    void Start()
    {

        Screen.orientation = ScreenOrientation.Portrait; 

        botao = login.GetComponent<Button>();
        botao.onClick.AddListener(sketchfaburl);
            
    }

    private void sketchfaburl()
    {

        Application.OpenURL("https://sketchfab.com/oauth2/authorize/?state=123456789&response_type=token&approval_prompt=auto&client_id=" + _clientID);
        sketchfaboauth();
        if (ApplicationModel._token != ""){
            SceneManager.LoadScene(1);
        }
    }
    
    private void sketchfaboauth()
    {
        Application.deepLinkActivated += onDeepLinkActivated;
        Debug.Log(Application.absoluteURL);
    }

    private void onDeepLinkActivated(string url)
    {
        deepLinkURL = url;
        if (url != "")
        {
            getToken(url);
            SceneManager.LoadScene(1);
        }
        
    }

    private void getToken(string url)
    {
        Uri data = new Uri(url.Replace("#", "?"));
        var dictionary =  HttpUtility.ParseQueryString(data.Query);
        ApplicationModel._token = dictionary.Get("access_token");
    }

}
