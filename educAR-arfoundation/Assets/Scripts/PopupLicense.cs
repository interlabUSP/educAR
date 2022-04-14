using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupLicense : MonoBehaviour
{

    [SerializeField] Button _close;
    [SerializeField] Text _licenseText;

    public bool popupActivated = false;

    public void Init(Transform canvas, string popupMessage){
        _licenseText.text = popupMessage;
        
        transform.SetParent(canvas);
        transform.localScale = Vector3.one;
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = Vector2.zero;

        _close.onClick.AddListener(() => {
            GameObject.Destroy(this.gameObject);
            popupActivated = false;
        });
    }
}
