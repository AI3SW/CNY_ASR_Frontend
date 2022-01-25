using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class setColliderWidthtoScreenWidth : MonoBehaviour
{
    BoxCollider2D _box;
    // Start is called before the first frame update
    void Awake()
    {
        _box = GetComponent<BoxCollider2D>();
        _box.size = new Vector3(Screen.width,1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
