using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setHeightToWidth : MonoBehaviour
{
    // Start is called before the first frame update
    RectTransform rect;
    void Awake()
    {
        rect = GetComponent<RectTransform>();

    }

    private void Start()
    {
        rect.sizeDelta = Vector2.one * rect.rect.height;
        rect.offsetMin = new Vector2(rect.offsetMin.x, 0);
        rect.offsetMax = new Vector2(rect.offsetMax.x, 0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
