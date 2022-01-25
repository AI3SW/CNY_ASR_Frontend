using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace AICUBE.REST
{
    [System.Serializable]
    public class Options
    {
        public Options(string ip, string port = "", bool secured = false, bool needSession = false)
        {
            _ip = ip;
            _port = port;
            _secured = secured;
            _needSession = needSession;
        }
        public string _ip
        {
            get;
            private set;
        }
        public string _port
        {
            get;
            private set;
        }

        public bool _secured
        { 
            get;
            private set;
        }
        public bool _needSession
        {
            get;
            private set;
        }
        public string finalUrl => ((_secured) ? "https" : "http") + $"://{_ip}:" + (!string.IsNullOrWhiteSpace(_port) ? _port +"/" : "")  ;
    }

    public class RESTConnectionResult<T>
    {
        public bool isConnected
        {
            get;
            private set;
        }
        public T jsonData
        {
            get;
            private set;
        }

        public RESTConnectionResult()
        {
            isConnected = false;
            jsonData = default;
        }
        public RESTConnectionResult(T data, bool connection)
        {
            isConnected = connection;
            jsonData = data;
        }
    }
    public class RESTinterface
    {
        public RESTinterface(Options opt)
        {
            serverInfo = opt;
            if(debugOn)
                Debug.Log(opt.finalUrl);
        }
        private RESTinterface()
        {
        }
        public static string gUID
        {
            get;
            private set;
        }
        public static string device_ID
        {
            get;
            private set;
        }
        public bool debugOn = true;
        // Change this as when needed
        public Options serverInfo;

        string LOG_DIR = "/log/";
        string LOG_FILE = "server_logs.log";

        void WriteError(string errorStr, string api)
        {
            string error = "<" + api + "> --- Begin Error Message: " + errorStr + System.Environment.NewLine;
            //File.AppendAllText(LOG_DIR + LOG_FILE, error);
                Debug.Log(error);
        }
        /// <summary>
        /// 
        /// Usage :
        /// Always check for Receive.connected is true before using data
        /// 
        /// </summary>
        /// <typeparam name="Receive"> the json type that server has determined to receive </typeparam>
        /// <param name="api"> string api of what the host address consist, eg; 192.168.0.1/abc/ "abc/" would be the api </param>
        /// <returns></returns>
        public async Task<RESTConnectionResult<Receive>> getJsonData<Receive>(string api)
        {
            RESTConnectionResult<Receive> jsonResult = new RESTConnectionResult<Receive>();
            string url = serverInfo.finalUrl + api;
            if (debugOn) Debug.Log(url);
            if ((serverInfo._needSession) && (string.IsNullOrEmpty(gUID) || string.IsNullOrEmpty(device_ID)))
            {
                if (debugOn)
                    Debug.Log("gUID or device_ID is null, Please Start Session");

                return jsonResult;
            }
            try
            {


                var webResult = await LoadUrlAsync(url);
                if (string.IsNullOrEmpty(webResult.error))
                {
                    string result = webResult.downloadHandler.text.Trim();
                    //Debug.Log(result);
                    if (typeof(Receive).Name == typeof(AICUBE.REST.CNY.CNY_Phrases).Name)
                    {
                        int removeCount = 0;
                        result = "{ \"data\" :" + result.Substring(removeCount, result.Length - removeCount) + "}";
                        //Debug.Log(result);
                    }
                    jsonResult = new RESTConnectionResult<Receive>(JsonUtility.FromJson<Receive>(result), true);
                }
                else
                {
                    if (debugOn)
                    {
                        Debug.Log(webResult.responseCode);
                        WriteError(webResult.error, "getJsonDataT");
                    }

                }
                if (debugOn)
                    Debug.Log(webResult.downloadHandler.text);
            }
            catch (System.Exception ex)
            {
                if (debugOn)
                {
                    AICUBE.Utils.ErrorUtils.printAllErrors(ex);
                }
            }

            return jsonResult;
        }

        /// <summary>
        /// 
        /// Usage :
        /// Always check for Receive.connected is true before using data
        /// 
        /// </summary>
        /// <typeparam name="Receive"> the json type that server has determined to send </typeparam>
        /// <typeparam name="Send"> the json type that server has determined to receive</typeparam>
        /// <param name="api"> string api of what the host address consist, eg; 192.168.0.1/abc/ "abc/" would be the api </param>
        /// <param name="json">json object to be send to destination</param>
        /// <returns></returns>
        public async Task<RESTConnectionResult<Receive>> PostJsonResult<Receive, Send>(string api, Send json)
        {
            RESTConnectionResult<Receive> jsonResult = new RESTConnectionResult<Receive>();

            string url = serverInfo.finalUrl + api;
            Debug.Log(url);
            if (string.IsNullOrEmpty(gUID) || string.IsNullOrEmpty(device_ID))
            {
                if (debugOn)
                    Debug.Log("gUID or device_ID is null, Please Start Session");
                return jsonResult;
            }
            //jsonObj.guid = gUID;
            //jsonObj.device_id = device_ID;
            string jsonstring = JsonUtility.ToJson(json);
            var request = await LoadPostUrlAsync(url, jsonstring);
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.downloadHandler.text.Trim();
                //request.erro
                if (!string.IsNullOrEmpty(result))
                {
                    jsonResult = new RESTConnectionResult<Receive>(JsonUtility.FromJson<Receive>(result), true);
                    Debug.Log(result);
                }
            }
            else
            {
                WriteError(request.error, "analyze");
            }
            if (debugOn && request != null)
                Debug.Log(request.downloadHandler.text);


            return jsonResult;
        }

        async Task<UnityWebRequest> LoadUrlAsync(string url)
        {

            if (debugOn)
                Debug.Log("Loading url: " + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            await new WaitUntil(() => { return request.isDone; }); // artifical wait 
            if (debugOn)
                Debug.Log("Request Receive from url: " + request.url +
                "\nNetworkError: " + request.isNetworkError +
                "\nHttpError: " + request.isHttpError +
                "\ndata: " + request.downloadHandler.text +
                "\nResponseCode: " + request.responseCode);

            await new WaitForSeconds(3.0f); // artifical wait
                                            //callback(request);

            return request;
        }
        async Task<UnityWebRequest> LoadPostUrlAsync(string url, string jsonString)
        {

            //then set the headers Dictionary headers=form.headers; headers["Content-Type"]="application/json";

            WWWForm form = new WWWForm();
            byte[] jsonSenddata = null;
            if (!string.IsNullOrEmpty(jsonString))
            {
                if (debugOn)
                    Debug.Log(jsonString);
                jsonSenddata = System.Text.Encoding.UTF8.GetBytes(jsonString);
            }
            UploadHandlerRaw uH = new UploadHandlerRaw(jsonSenddata);
            form.headers["Content-Type"] = "application/json";
            //form.headers["Accept"] = "application/json";

            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            //headers["Accept"] = "application/json";
            if (debugOn)
                Debug.Log("Loading url: " + url);
            //var request = await new WWW(url, jsonSenddata, headers);
            UnityWebRequest request = UnityWebRequest.Post(url, headers);
            request.uploadHandler = uH;
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Accept", "application/json");

            //Debug.Log(request.GetRequestHeader("Content-Type"));
            await request.SendWebRequest();

            await new WaitUntil(() => { return request.isDone; }); // artifical wait 
            if (debugOn)
                Debug.Log("Request Receive from url: " + request.url +
                "\nNetworkError: " + request.isNetworkError +
                "\nHttpError: " + request.isHttpError +
                "\ndata: " + request.downloadHandler.text +
                "\nResponseCode: " + request.responseCode);
            if (debugOn)
                Debug.Log("ErrorCode :" + request.error);
            //Debug.Log(url + "\n" + request.text);


            await new WaitForSeconds(0.1f); // artifical wait for 150ms


            return request;
        }

        private void Start()
        {
            LOG_DIR = Application.dataPath + LOG_DIR;


        }

        public void StartSession()
        {
            gUID = Guid.NewGuid().ToString();
            device_ID = SystemInfo.deviceUniqueIdentifier;
            //sessionID = GUID + DUID;
            if (debugOn)
            {
                //Debug.Log(sessionID);
                //Debug.Log(sessionID.Length);
                Debug.Log(gUID);
                Debug.Log(device_ID);
            }
        }

        public void EndSession()
        {
            gUID = "";
            //device_ID = "";
        }
    }
}