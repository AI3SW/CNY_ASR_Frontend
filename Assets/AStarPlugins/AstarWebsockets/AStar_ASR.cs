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

public class AStar_ASR : MonoBehaviour
{
    #region Variables
    //textbox output
    public TextMeshProUGUI textbox;
    public UnityEventString OnStringReadyEvent;
    public UnityEventStringJSONwithID OnJSONReadyEvent;

    /// <summary>
    /// fallback for onJSONready
    /// </summary>
    public UnityEventByteArray OnByteReadyEvent;
    public UnityEngine.Events.UnityEvent OnSessionDisconnect;
    /// <summary>
    /// calls GameManager GameStarting
    /// </summary>
    public UnityEngine.Events.UnityEvent OnSessionConnect;

    //autoping function
    public UnityEngine.Events.UnityEvent OnSessionSuddenDisconnect;
    public UnityEngine.Events.UnityEvent OnSessionReconnect;
    public UnityEngine.Events.UnityEvent OnNoNetwork;
    //

    // Server IP address
    [SerializeField]
    private string hostIP;
    [SerializeField]
    private string api = "/ws/streamraw/";
    // Server port
    [SerializeField]
    private string port = "5432";
    private string finalPort => (string.IsNullOrEmpty(port)) ? "" : ":" + port;
    private bool securedconnection = false;

    [SerializeField]
    private bool directToDAT = true;

    [SerializeField]
    private bool useLocalhost = false;



    private string finalIP => useLocalhost ? "localhost" : hostIP;
    // creating a websocket connection than returns me this result of type ASRRESULT as stated in datacontract
    public AStarWebSocketStreamController<ASRResult> websocket
    {
        get;
        private set;
    }

    private ConcurrentQueue<ASRResult> queueStreamResult;
    private ConcurrentQueue<string> queueStringResult;
    [SerializeField]
    private int sampleRate = 16000;
    //[SerializeField]
    //private int channels = 1;
    //[SerializeField]
    //private int bitsize = 16;
    [SerializeField]
    bool useDebug = true;
    ASRConfigFile configsettings;
    public string sessionID
    {
        get;
        private set;
    }

    public bool sessionStarted
    {
        get;
        private set;
    }
    public string url
    {
        get;
        private set;
    }

    public bool _recordState
    {
        get;
        private set;
    }

    public MicEncoder recorder;
    #endregion

    #region UnityFunctions
    void Awake()
    {
        InitializeRecordingVariable();
        queueStreamResult = new ConcurrentQueue<ASRResult>();
        queueStringResult = new ConcurrentQueue<string>();
        loadURLfromFile();
    }

    void loadURLfromFile()
    {
        string data = "{ \"ip\" : \"10.2.117.32\", \"port\" : \"8008\"}";
;
        string filepath = "/ASR/ASR_config.txt";
        string directoryPath = "/ASR";
        if (File.Exists(Application.persistentDataPath + filepath))
        {

            Debug.Log(Application.persistentDataPath + filepath);
            data = File.ReadAllText(Application.persistentDataPath + filepath);
            Debug.Log("Reading");
        }
        else
        {
            try
            {
                Debug.Log("creating");
                Directory.CreateDirectory(Application.persistentDataPath + directoryPath);
                using (File.Create(Application.persistentDataPath + filepath)) ;
                File.WriteAllText(Application.persistentDataPath + filepath, data);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ERROR - {ex.StackTrace}\n\n ErrorMessage - {ex.Message}");
                Exception innerE = ex;
                while ((innerE = innerE.InnerException) != null)
                {
                    Debug.LogWarning($"ERROR - {ex.InnerException.StackTrace}\n\n ErrorMessage - {ex.InnerException.Message}");
                }
            }
        } 
        //TextAsset configFile = Resources.Load("") as TextAsset;
        Debug.Log(data);
        configsettings = JsonUtility.FromJson<ASRConfigFile>(data);
        hostIP = configsettings.ip;
        port = configsettings.port;
    }
    public void Update() {
        ASRResult result;
        if (queueStreamResult.TryDequeue(out result))
        {
            try
            {
                OnJSONReadyEvent?.Invoke(String.Compare(result.cmd, "asrpartial") == 0,result.result,result.uttID);
            }
            catch (Exception e)
            {
                if (useDebug) 
                {

                    Debug.LogError("json result received is unidentified");
                    Astar.Utils.ErrorUtils.printAllErrors(e);
                }
            }
        }
        string sessionData ="";
        if (queueStringResult.TryDequeue(out sessionData))
        {
            try
            {
                Debug.Log(sessionData);
                if (sessionData.Contains("session_id"))
                {
                    sessionData = sessionData.Substring(1, sessionData.Length-1);
                    SentenceInfo sInfo = JsonUtility.FromJson<SentenceInfo>(sessionData);
                    if(!string.IsNullOrEmpty( sInfo.session_id)) {
                        sessionID = sInfo.session_id;
                        OnStringReadyEvent?.Invoke(sessionData);

                        StartNewSession();
                        Debug.Log("started");
                    }

                }
                
            }
            catch (Exception e)
            {
                if (useDebug)
                {
                    Debug.LogError("string received is unidentified");
                    Astar.Utils.ErrorUtils.printAllErrors(e);
                }
            }
        }
    }

    public void OnEnable()
    {
        //ConnectToWebSocket();
    }
    public void OnDisable()
    {
        //DisconnectToWebSocket();
    }

    #endregion
    #region PublicHandles
    public void InitializeRecordingVariable()
    {
        if(useDebug) Debug.Log("Initializing Session");

        _recordState = false;
        sessionStarted = false;
    }

    public async void StartRecording()
    {
        
        if (!_recordState )
        {
            
            //recordDevice.StartRecording();
            if (websocket.isConnected())
            {
                if(directToDAT)
                {
                    byte[] b = new byte[1] { 0 };
                    AstarStreamWrapper startStream = new AstarStreamWrapper(b, AstarStreamWrapper.wsUsage.UNFORMATTED_BINARY);
                    websocket.stream(startStream);
                }

                recorder.gameObject.SetActive(true);
                //await Task.Delay(500);
                _recordState = true;
                Debug.Log("Start stream with WebService");
            } else
            {
                Debug.Log("Websocket Not Connected");
            }
            
        } else
        {
            if (useDebug)
            {
                if (_recordState) Debug.Log("Already recording");
            }
            
        }
    }

    public async void sendNewWord(int textIndex,int sentenceIndex)
    {
        Debug.Log("newword");
        string stringToSend = "{\"right_text\":\"" + textIndex+ "\", \"session_id\":\"" + sessionID + "\", \"sequence_id\":\"" + sentenceIndex+ "\"}";
        AstarStreamWrapper streamword = new AstarStreamWrapper(stringToSend);

        //await waitingForStream;
        //sendingStream = true;
        await websocket.stream(streamword);
        if (sentenceIndex == 0)
            StartRecording();
        //sendingStream = false;
    }

    public void StopRecording()
    {
        if (_recordState) { 
            _recordState = false;
            recorder.gameObject.SetActive(false);
        } else
        {
            if (useDebug) Debug.Log("Recording has not begun");
            return;
        }

        if (websocket != null && websocket.isConnected())
        {
            if(directToDAT)
            {
                byte[] b = new byte[1] { 1 };
                AstarStreamWrapper endStream = new AstarStreamWrapper(b, AstarStreamWrapper.wsUsage.UNFORMATTED_BINARY);
                websocket.stream(endStream);
            }
            else
            {

            }
            Debug.Log("End stream with WebService");
        }
        else
        {
            Debug.Log("Websocket Not Connected");
        }
    }

    public void DisposeRecording()
    {
        if (_recordState)
        {
            StopRecording();
        }
    }
    #endregion
    #region Callbacks
    //Handle stream results here
    private void receive_Stream_Result(object sender, Astar.Utils.Websocket.OnStreamResultEventArgs<ASRResult> JSONresult)
    {
        queueStreamResult.Enqueue(JSONresult.eventData);

    }
    private void receive_String_Result(object sender, Astar.Utils.Websocket.OnStreamResultEventArgs<string> stringResult)
    {
        
        queueStringResult.Enqueue(stringResult.eventData);
        //Debug.Log(queueStringResult.Count);
        //Debug.Log(stringResult.eventData);
    }

    /*
    private void onWaveInRecordingStopped(object sender, StoppedEventArgs e)
    {
        _recordState = false;
    }*/

    public async void onWaveInDataAvailable(byte[] byteArray)
    {
       
        if (!_recordState || websocket == null) return;
        //Debug.Log("sending");
        //sendingStream = true;
        AstarStreamWrapper streamwrapper = new AstarStreamWrapper(byteArray, AstarStreamWrapper.wsUsage.ASR_DAT);
        await websocket.stream(streamwrapper);
        //sendingStream = false;
        //Debug.Log("audio sending");
    }
    public void onWaveInFloatDataAvailable(float[] floatArray)
    {
        Queue<byte> byteArray = new Queue<byte>();
        for (int i = 0; i < floatArray.Length; i++)
        {
            byte[] byteData = BitConverter.GetBytes(floatArray[i]);
            foreach (byte _byte in byteData) byteArray.Enqueue(_byte);

        }
        onWaveInDataAvailable(byteArray.ToArray());
        //Debug.Log("audio sending");
    }
    /*
    private void onWaveInDataAvailable(object sender, WaveInEventArgs e)
    {
        //calls the above
        onWaveInDataAvailable(e.Buffer);
        //Debug.Log("audio sending");
    }
    */
    #endregion
    #region PauseAndQuitEdgeCases;
    private void OnApplicationPause(bool pause)
    {
        if(pause && _recordState)
        {
            StopRecording();

            if (useDebug) Debug.Log("Game Pause");
            
        }
    }

    private void OnApplicationQuit()
    {
        DisposeRecording();
        DisconnectToWebSocket();

        if (useDebug) Debug.Log("Game Exit");
    }
    #endregion
    #region Connection

    /// <summary>
    /// have to be called manually when using normal dat db;
    /// Completes the expected protocol handshake
    /// </summary>
    public async void StartNewSession()
    {
        //Debug.Log("startnewsesssion");
        if (!sessionStarted)
        {
            if (!directToDAT)
            { // to Erics APi
                
                sessionStarted = true;
                OnSessionConnect?.Invoke();
                
            }
            else
            {
                //To dats API
                StartRecording();
                await Task.Delay(500);

                sessionStarted = true;
                OnSessionConnect?.Invoke();
            }
        }



    }
    private async void ContinueSession()
    {
        if (!sessionStarted)
        {
            if (!directToDAT)
            {
                StartRecording();

                sessionStarted = true;
                OnSessionReconnect?.Invoke();
                
            }
            else
            {
                StartRecording();
                await Task.Delay(500);

                sessionStarted = true;
                OnSessionReconnect?.Invoke();
            }
        }

    }
    private void DoUnexpectedDisconnection()
    {
        if (_recordState)
        {
            _recordState = false;
            sessionStarted = false;
            OnSessionSuddenDisconnect?.Invoke();
        }
    }

    private void OnNoNetWork()
    {
        _recordState = false;
        sessionStarted = false;
        Debug.Log("Unable To Connect, Try again when there is internet");
        OnNoNetwork?.Invoke();
    }

    public async Task<bool> ConnectToWebSocket()
    {
        bool isConnected = false;
        CreateNewWebSocketIfNull();

        if (!websocket.isConnected())
        {
            Debug.Log("Start ConnectionHandshake");
            //Debug.Log("www.google.com");
            //2 ways of connecting, force reconnection thru autoping until cancelled
            //or 1 time connection with INIT
            if (directToDAT)
                websocket.startAutoPing();
            else
            {
                Task<bool> connection = websocket.Init();
                await connection;
                if(!connection.Result)
                {
                    DisconnectToWebSocket();
                    if (useDebug) Debug.Log("Connection to WebServer has not been established " + url);
                }
                isConnected = connection.Result;
                if (useDebug) Debug.Log("Connection to WebServer has been established " + url);
            }
            
        }
        else
        {
            if (useDebug) Debug.Log("Connection to WebServer has been established " + url);
        }
        return isConnected;
    }

    private void CreateNewWebSocketIfNull()
    {
        if (websocket == null)
        {
            if (directToDAT)
            {
                securedconnection = true;
                url = ((securedconnection) ? "wss" : "ws") + "://" + finalIP + api + sampleRate + "/";
            }
            else
            {
                securedconnection = false;
                url = ((securedconnection) ? "wss" : "ws") + "://" + finalIP + finalPort + "/";
            }

            if (useDebug) Debug.Log(url + " Configuring Server Setup");
            websocket = new AStarWebSocketStreamController<ASRResult>(url);

            websocket.OnConnect += StartNewSession;  // methods greatly differs when using direct to dat and not.
            websocket.OnStreamResult += receive_Stream_Result;
            websocket.OnStringResult += receive_String_Result;

            //websocket.OnByteResult += receive_Stream_Result;
            //Auto ping functions
            websocket.OnUnexpectedDisconnection += DoUnexpectedDisconnection;
            websocket.OnNoNetworkConnection += OnNoNetWork;
            websocket.OnReconnect += ContinueSession;
            //
        }
    }

    public void DisconnectToWebSocket()
    {

        if (websocket != null && websocket.isConnected())
        {
            DisposeRecording();
            websocket.exit();// abort thread
            websocket.OnStreamResult -= receive_Stream_Result;
            websocket.OnStringResult -= receive_String_Result;
            websocket.OnConnect -= StartNewSession;

            OnSessionDisconnect?.Invoke();

            //Auto ping functions
            websocket.OnUnexpectedDisconnection -= DoUnexpectedDisconnection;
            websocket.OnNoNetworkConnection -= OnNoNetWork;
            websocket.killAutoPing();
            websocket.OnReconnect -= ContinueSession;
            sessionStarted = false;
            websocket = null;
            //
            
            if (useDebug) Debug.Log("Disconnect");
        }
        else
        {
            if (useDebug) Debug.Log("It is not connnected");
        }

    }
    #endregion

}
