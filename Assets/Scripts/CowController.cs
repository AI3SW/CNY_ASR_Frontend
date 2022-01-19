using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CowController : MonoBehaviour
{
    RectTransform _rectT;
    Animator _anim;

    public enum Cowstate{
        sleep,
        awake,
        dancing
    }
    public Cowstate currentState = 0;
    // Start is called before the first frame update
    void Awake()
    {

        _anim = GetComponent<Animator>();
       //setState(Cowstate.sleep);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setState(Cowstate state)
    {
        if(currentState != state)
        {
            if (_anim == null) _anim = GetComponent<Animator>();
            _anim.SetTrigger("Transit");
            _anim.SetInteger("State", (int)state);
            currentState = state;
        }

    }
    public async void setStateWithDelay(Cowstate state)
    {
        if (currentState != state)
        {
            currentState = state;
            await Task.Delay(Random.Range(200, 800));
            _anim.SetTrigger("Transit");
            _anim.SetInteger("State", (int)state);
        }

    }
    public void setByWeightState(float weight)
    {
        Cowstate tempstate = Cowstate.awake;
        //Debug.Log(weight);
        if(weight >= 0 && weight < 0.33f)
        {
            tempstate = Cowstate.sleep;
        } else if (weight >= 0.33f && weight < 0.66f)
        {
            tempstate = Cowstate.awake;
        } else
        {
            tempstate = Cowstate.dancing;
        }
        
        setState(tempstate);
    }
    public void nextState()
    {
        //Debug.Log("cows found");
        if (currentState+1 <= Cowstate.dancing)
            setState(1+currentState);
    }
}
