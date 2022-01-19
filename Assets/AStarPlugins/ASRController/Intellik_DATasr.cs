using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Threading.Tasks;

using Astar.WebSocket;
using Astar.WebSocket.Utils;
using Astar.WebSocket.DataContracts.Receive.HLTASRLib;
using Astar.WebSocket.DataContracts.Receive;

using TMPro;
using UnityEngine;
using System.IO;
using SP;

namespace SP
{
    public enum MODE
    {
        Livestream = 0x00000001 << 0,
        RecAndSend = 0x00000001 << 1,
        None = 0x0
    };
}


public class Intellik_DATasr : MonoBehaviour , ASR_UploadandReceive, ASR_LiveStream//, ASR_ServiceProvider
{

    public MicEncoder _recorder;
    public AStar_ASR _asr_server;

    SP.MODE services;

    public event Action<string> On_ReceiveASR_Results;

    //public ASR_UploadandReceive.onReceiveASR_RecResults onReceiveResults;
    // Start is called before the first frame update
    void Start()
    {
        //_SP = new ASR_ServiceProvider();
        //_SP.name = "DAT's_ASR";
        services = SP.MODE.Livestream | SP.MODE.RecAndSend;
        //RecordAndSend(_recordingTime);
        //On_ReceiveASR_Results += onReceivehehe_ASR_RecResults;
        
        /*
        ASR_UploadandReceive test1 = this;
        ASR_LiveStream test2 = this;
        Debug.Log(SP.MODE.Livestream);
        Debug.Log(SP.MODE.RecAndSend);
        test1.GetMode();
        test2.GetMode();
        */
    }

    private async Task SendSpeechData()
    {
        Debug.Log("Sending Start");
        
        //asr_server.sendNewWord(1, 1);
        Queue<byte[]> byteQ = _recorder.RetrieveEncode();
        while (byteQ.Count > 0)
        {
            byte[] currentByte = byteQ.Dequeue();
            AstarStreamWrapper streamwrapper = new AstarStreamWrapper(currentByte, AstarStreamWrapper.wsUsage.ASR_DAT);
            await _asr_server.websocket.stream(streamwrapper);
        }
        

        //AstarStreamWrapper streamwrapper = new AstarStreamWrapper(data, AstarStreamWrapper.wsUsage.ASR_DAT);
        //await _asr_server.websocket.stream(streamwrapper);

        Debug.Log("Sending End, waiting last info");
        return;
    }

    public void onReceiveASR_Results(bool isPartial, string data, ulong id)
    {
        if (!isPartial)
        {//Full
            //text.text += data;
            Debug.Log("[Full] UttID=" + id + ", Partial, " + data);
            On_ReceiveASR_Results?.Invoke( data);
            //_textbox.text = "Debug : " + "[Partial] UttID=" + id + ", Partial, " + data;
        }
        else
        {//partial
            Debug.Log("[Partial] UttID=" + id + ", Partial, " + data);
            //_textbox.text = "Debug : " + "[Partial] UttID=" + id + ", Partial, " + data;
            
        }
    }

    async public Task recordAudio(float duration)
    {
        Debug.Log("test");
        _recorder.StartAll();
        await Task.Delay(Mathf.RoundToInt(1000 * duration));
        _recorder.StopAll();

        /*
        Queue<byte[]> byteQueue = _recorder.RetrieveEncode();
        List<byte> fullAudio = new List<byte>();
        foreach (var part in byteQueue)
        {
            fullAudio.AddRange(part);
        }

        return fullAudio.ToArray();*/
    }

    async public Task<bool> ConnectandSend()
    {
        Task<bool> waitingConnection = _asr_server.ConnectToWebSocket();
        await waitingConnection;
        if (waitingConnection.Result)
        {
            Debug.Log("Connected");
            await SendSpeechData();

            await Task.Delay(10000);
            _asr_server.DisconnectToWebSocket();
            Debug.Log("Connection End");
        }
        else
        {
            Debug.Log("Not Connected");
        }
        return waitingConnection.Result;
    }

    public string GetName()
    {
        throw new NotImplementedException();
    }

    MODE ASR_UploadandReceive.GetMode()
    {
        //Debug.Log(services);
        return services;
    }

    MODE ASR_LiveStream.GetMode()
    {
        //Debug.Log(services);
        return services;
    }

    public void onReceiveASR_LiveResults(bool isPartial, string data, ulong id)
    {
        throw new NotImplementedException();
    }

    public byte[] StartRecording()
    {
        throw new NotImplementedException();
    }

    public void StopRecording()
    {
        throw new NotImplementedException();
    }

    public void StreamAudio(byte[] data)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Connect()
    {
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }


}
