using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public Transform mainCanvas;

    void Start()
    {
        if (Instance != null){
            GameObject.Destroy(this.gameObject);
        }

        Instance = this;
    }

    public PopupLicense CreatePopup(){
        GameObject popUpGo = Instantiate(Resources.Load("UI/Popup") as GameObject);
        return popUpGo.GetComponent<PopupLicense>();
    }
}
