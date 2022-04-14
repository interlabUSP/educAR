using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;

public class Canal : MonoBehaviour
{

    public GameObject canal;
    public GameObject senha;
    public GameObject login;

    public Button botao;
    public Button voltar;

    private string Channel;
    private string Password;
    public string api;

    public Text problem;
   

    // Start is called before the first frame update
    void Start()
    {

        Screen.orientation = ScreenOrientation.Portrait; 

        botao = login.GetComponent<Button>();
        botao.onClick.AddListener(validate);

        problem.text = "";

        voltar.onClick.AddListener(Return);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void validate()
    {

        Channel = canal.GetComponent<InputField>().text;
        Password = senha.GetComponent<InputField>().text;

        print ("Conectando no canal");
        StartCoroutine(CallLogin(Channel, Password));
    }

    public IEnumerator CallLogin(string Channel, string Password)
    {

        ApplicationModel.Dados dados = new ApplicationModel.Dados();
        dados.password = Password;
        string debug = JsonUtility.ToJson(dados);
        api = "https://4wu9au10o7.execute-api.us-east-1.amazonaws.com/dev/channels/" + Channel;
        var uwr = new UnityWebRequest(api, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(debug);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");


        yield return uwr.SendWebRequest();
        Debug.Log(uwr.downloadHandler.text);


        if (uwr.error != null)
        {
            Debug.Log("Canal ou Senha Incorreto");
            problem.text = "Canal ou Senha Incorreto";
        }

        else
        {
            Debug.Log("Conectado com sucesso");

            yield return ApplicationModel.cena = JsonUtility.FromJson<ApplicationModel.SceneList> (uwr.downloadHandler.text);
            SceneManager.LoadScene(2); 

        }

   
    }

    private void Return()
    {
        SceneManager.LoadScene(0);
        ApplicationModel._token = "";
    }
}
