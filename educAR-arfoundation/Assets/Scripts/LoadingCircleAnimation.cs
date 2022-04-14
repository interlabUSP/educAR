using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircleAnimation : MonoBehaviour
{
    private float rotateSpeed = 200f;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}
