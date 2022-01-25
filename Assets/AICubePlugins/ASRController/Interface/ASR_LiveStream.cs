using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public interface ASR_LiveStream
{
    void onReceiveASR_LiveResults(bool isPartial, string data, ulong id);
    byte[] StartRecording();
    void StopRecording();
    void StreamAudio(byte[] data);

    //connection success or fail
    Task<bool> Connect();
    void Disconnect();

    string GetName();

    SP.MODE GetMode();
}
