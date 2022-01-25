using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
[Serializable]
public enum MicDeviceMode { Default, TargetDevice }

[RequireComponent(typeof(AudioSource))]
public class MicEncoder : MonoBehaviour{


#if !UNITY_WEBGL || UNITY_EDITOR
    //----------------------------------------------
    AudioSource AudioMic;
    private Queue<byte> AudioBytes = new Queue<byte>();
    private Queue<float> AudioFloat = new Queue<float>();
    public MicDeviceMode DeviceMode = MicDeviceMode.Default;
    public string TargetDeviceName = "MacBook Pro Microphone";
    string CurrentDeviceName = null;

    [TextArea]
    public string DetectedDevices;

    //[Header("[Capture In-Game Sound]")]
    public bool StreamGameSound = true;
    public bool StartOnEnable = true;
    public int OutputSampleRate = 11025;
    public int OutputChannels = 1;
    private object _asyncLockAudio = new object();

    int CurrentAudioTimeSample = 0;
    int LastAudioTimeSample = 0;
    //----------------------------------------------

    [Range(1f, 60f)]
    public float StreamFPS = 20f;
    float interval = 0.05f;

    public bool GZipMode = false;
    
    public UnityEventByteArray OnDataByteReadyEvent;
    public Action<float[]> OnDataFloatReadyEvent;
    //[Header("Pair Encoder & Decoder")]
    float next = 0f;
    bool stop = true;

    public int dataLength;
    // Use this for initialization
    AudioSource src;

    
    void Start()
    {

        //StartCoroutine(CaptureMic());
        //if(StreamGameSound)
        //    StartCoroutine(SenderCOR());
    }

    IEnumerator CaptureMic()
    {
        if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
        if (AudioMic == null) AudioMic = gameObject.AddComponent<AudioSource>();

        //Check Target Device
        DetectedDevices = "";

        string[] MicNames = Microphone.devices;
        foreach (string _name in MicNames)
        {
            DetectedDevices += _name + "\n";
            Debug.Log(DetectedDevices);
        }

        if (DeviceMode == MicDeviceMode.TargetDevice)
        {
            bool IsCorrectName = false;
            for(int i = 0; i<MicNames.Length; i++)
            {
                if(MicNames[i] == TargetDeviceName)
                {
                    IsCorrectName = true;
                    break;
                }
            }
            if (!IsCorrectName) TargetDeviceName = null;
        }
        //Check Target Device

        CurrentDeviceName = DeviceMode == MicDeviceMode.Default ? MicNames[MicNames.Length-1] : TargetDeviceName;
        //Debug.Log(CurrentDeviceName);
        int minFreq, maxFreq;
        Microphone.GetDeviceCaps(CurrentDeviceName, out minFreq, out maxFreq);
        //Debug.Log(minFreq + " "+ maxFreq );
        AudioMic.clip = Microphone.Start(CurrentDeviceName, true, 1, OutputSampleRate);
        //Debug.Log(AudioMic.clip.loadType);
        AudioMic.loop = true;
        while (!(Microphone.GetPosition(CurrentDeviceName) > 0)) { }
        Debug.Log("Start Mic(pos): " + Microphone.GetPosition(CurrentDeviceName));
        LastAudioTimeSample = CurrentAudioTimeSample = 0;
        //AudioMic.Play();

        AudioMic.volume = 1f;

        OutputChannels = AudioMic.clip.channels;
        Debug.Log("recording");
        while (!stop)
        {
            AddMicData();
            //Debug.Log("test");
            yield return null;
        }
        yield return null;
    }


    void AddMicData()
    {
        //Debug.Log("capturing");
        LastAudioTimeSample = CurrentAudioTimeSample;
        //CurrentAudioTimeSample = AudioMic.timeSamples;
        CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);

        if (CurrentAudioTimeSample != LastAudioTimeSample)
        {
            float[] samples = new float[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, 0);
            //Debug.Log(CurrentAudioTimeSample);
            // 2 threads running, 1 to save and 1 to load. to ensure saveload not corrupted, locking required using _asyncLockAudio
            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {//handle normal case
                lock (_asyncLockAudio) 
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        AudioFloat.Enqueue(samples[i]);
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                        
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            { //handles byte overflow
                lock (_asyncLockAudio) 
                {
                    for (int i = LastAudioTimeSample; i < samples.Length; i++)
                    {
                        AudioFloat.Enqueue(samples[i]);
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                    for (int i = 0; i < CurrentAudioTimeSample; i++)
                    {
                        AudioFloat.Enqueue(samples[i]);
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                }
            }
        }
    }

    IEnumerator SenderCOR()
    {

        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                interval = 1f / StreamFPS;
                next = Time.realtimeSinceStartup + interval;
                EncodeBytes();
                //Debug.Log(Time.realtimeSinceStartup);
            }
            yield return null;
        }
    }

    Queue<byte[]> queueOfBytes = new Queue<byte[]>();
    byte[] audioData;
    void EncodeBytes()
    {

        //==================getting byte data==================
        //byte[] dataByte;
        
        byte[] _samplerateByte = BitConverter.GetBytes(OutputSampleRate);
        byte[] _channelsByte = BitConverter.GetBytes(OutputChannels);
        int sizeToCopy = 4096 - 4; // - 4 to offet for the 1 float that we will be adding in. in stream wrapper
        lock (_asyncLockAudio) // 2 threads running, 1 to save and 1 to load. to ensure saveload not corrupted, locking required
        {
            //dataByte = new byte[AudioBytes.Count + _samplerateByte.Length + _channelsByte.Length];

            //Buffer.BlockCopy(_samplerateByte, 0, dataByte, 0, _samplerateByte.Length);
            //Buffer.BlockCopy(_channelsByte, 0, dataByte, 4, _channelsByte.Length);
            //Buffer.BlockCopy(AudioBytes.ToArray(), 0, dataByte, 8, AudioBytes.Count);
            queueOfBytes.Clear();

            //Debug.Log("test");
            for (int i = 0; i < AudioBytes.Count; i+= sizeToCopy)
            {
                int copySlice = sizeToCopy;
                if (i + sizeToCopy > AudioBytes.Count)
                {
                    copySlice = AudioBytes.Count - i;
                }
                audioData = new byte[copySlice];
                Buffer.BlockCopy(AudioBytes.ToArray(), i, audioData, 0, copySlice);
                queueOfBytes.Enqueue(audioData);
                OnDataByteReadyEvent?.Invoke(audioData);
            }

            //OnDataFloatReadyEvent?.Invoke(AudioFloat.ToArray());

            AudioBytes.Clear();
        }
    }

    public Queue<byte[]> RetrieveEncode()
    {
        return queueOfBytes;
    }
    private void OnEnable()
    {
        if (StartOnEnable)
        {
            StartAll();
        }


        //if (Time.realtimeSinceStartup <= 3f) return;
        //
        //AICUBE_ASR.Singleton._recordReady = true;
    }
    private void OnDisable()
    {
        if (StartOnEnable)
        {
            StopAll();
        }
        //AICUBE_ASR.Singleton._recordReady = false;
    }

    public void StartAll()
    {
        Debug.Log(stop);
        if (stop)
        {
            Debug.Log("Recording Start");
            stop = false;
            StartCoroutine(CaptureMic());
            
            if (StreamGameSound)
                StartCoroutine(SenderCOR());
        }
    }
    public void StopAll()
    {
        stop = true;
        
        StopCoroutine(CaptureMic());
        if (StreamGameSound)
            StopCoroutine(SenderCOR());
        else
            EncodeBytes();
        //AudioMic.Stop();
        Microphone.End(CurrentDeviceName);
        Debug.Log("Recording End");
    }

#endif
}
