using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class setRectTransformToObject : MonoBehaviour
{
    [SerializeField]
    RectTransform refRectObj;
    [SerializeField]
    RectTransform refImageObj;
    [HideInInspector]
    RectTransform rect;
    RectTransform par;
    Vector3 origin;
    Image img;
    Vector2 distanceToParameterize;

    public bool parameterizeByHeight = true;
    // Start is called before the first frame update
    private void Awake()
    {

    }
    void Start()
    {
        ;// Init();
    }

    public void Init()
    {
        rect = GetComponent<RectTransform>();
        par = gameObject.transform.parent.GetComponent<RectTransform>();
        img = GetComponent<Image>();
        distanceToParameterize = Vector2.zero;
        if (parameterizeByHeight)
        {
            rect.sizeDelta = new Vector2(refRectObj.rect.size.x, refRectObj.rect.size.y);
            distanceToParameterize.y = refImageObj.rect.size.y;
        }
        else
        {
            rect.sizeDelta = new Vector2(refRectObj.rect.size.x, refRectObj.rect.size.y);
            distanceToParameterize.x = refImageObj.rect.size.x;
        }

        //rect.sizeDelta = refImageObj.rect.size;
        origin = rect.position;
        Debug.Log(distanceToParameterize);
    }
    public void shiftPosition(float fillAmount)
    {

        //Debug.Log(fillAmount);
        if (rect == null)
        {
            Init();


        }
        float newX = (!parameterizeByHeight) ? distanceToParameterize.x * fillAmount : 0;
        float newY = (parameterizeByHeight) ? distanceToParameterize.y * fillAmount : 0; ;
        Vector3 newPos = new Vector3(newX,newY,rect.localPosition.z);
        par.anchoredPosition = newPos; 
        rect.position = origin;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
