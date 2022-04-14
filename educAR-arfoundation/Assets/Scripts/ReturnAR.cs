using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ReturnAR : MonoBehaviour
{
    public Button voltar;

    void Start()
    {
        voltar.onClick.AddListener(Return);
    }

    private void Return()
    {
        SceneManager.LoadScene(1);
        LoaderUtility.Deinitialize();
    }
}
