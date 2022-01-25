using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter
{
    public Counter (int defaultInt){
        defaultValue = defaultInt;
        Init();
    }
    public Counter()
    {
        defaultValue = 0;
        Init();
    }

    private void Init()
    {
        CounterCallback = new UnityEventInt();
        resetCounter();
    }
    public UnityEventInt CounterCallback;
    public void IncreaseCounter()
    {
        ++_value;
        CounterCallback?.Invoke(_value);
    }
    public void DecreaseCounter()
    {
        --_value;
        CounterCallback?.Invoke(_value);
    }

    public int _value
    {
        get;
        private set;
    }

    private int defaultValue;
    // Start is called before the first frame update
    public void resetCounter()
    {
        _value = defaultValue;
        CounterCallback?.Invoke(_value);
    }

}
