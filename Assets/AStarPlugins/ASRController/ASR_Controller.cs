using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ASR_Controller : MonoBehaviour
{

    [System.Serializable]
    public enum Source
    {
        A_STAR = 0,
        Amazon,
        Google,
        Test,
    }

    [SerializeField]
    private float _duration = 10f;

    [SerializeField]
    private Source _selectedSrc;

    public TextMeshProUGUI _textbox;
    public Source selectedSrc
    {
        get
        {
            return _selectedSrc;
        }
        set {

            _selectedSrc = value;
        }
    }

    private ASR_UploadandReceive selectedASR;

    public List<GameObject> WebServices;
    // Start is called before the first frame update
    void Start()
    {
        Init(selectedSrc);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onReceivehehe_ASR_RecResults( string data)
    {
        _textbox.text = "Debug : " + data;
        Debug.Log(data);
    }

    void Init(Source src)
    {
        Debug.Log("Init");
        selectedASR = Instantiate<GameObject>(WebServices[(int)_selectedSrc],this.transform).GetComponent<ASR_UploadandReceive>();
        selectedASR.On_ReceiveASR_Results += onReceivehehe_ASR_RecResults;
        recordAndSend();
    }

    public async void recordAndSend()
    {
        Debug.Log("record Start");
        await selectedASR.recordAudio(_duration);

        bool sucessful = await selectedASR.ConnectandSend();
        if (sucessful)
        {
            Debug.Log("data send");
        }
        else
        {
            Debug.Log("data not send");
        }
    }
}
