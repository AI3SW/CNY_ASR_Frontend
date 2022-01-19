using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setWidthToHeight : MonoBehaviour
{
    RectTransform rect;
    // Start is called before the first frame update
    void Awake()
    {
        rect = GetComponent<RectTransform>();

    }
    private void Start()
    {
        rect.sizeDelta = Vector2.one * (rect.rect.width);
        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0, rect.offsetMax.y);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
