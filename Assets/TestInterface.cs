using SP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestInterface : MonoBehaviour , ASR_UploadandReceive
{

    SP.MODE services;

    public event Action<string> On_ReceiveASR_Results;

    public async Task<bool> ConnectandSend()
    {
        string data = "lorem ipsum";
        await Task.Delay(1000);
        On_ReceiveASR_Results.Invoke(data);
        return true;
    }
    void onReceivehehe_ASR_RecResults(string data)
    {
        Debug.Log(GetName() + " output : " + data);
    }
    public MODE GetMode()
    {
        return services;
    }

    public string GetName()
    {
        return "test";
    }

    public async Task recordAudio(float duration)
    {
        await Task.Delay(Mathf.RoundToInt(1000 * duration));
    }

    // Start is called before the first frame update
    void Start()
    {
        services = SP.MODE.None;
        On_ReceiveASR_Results += onReceivehehe_ASR_RecResults;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
