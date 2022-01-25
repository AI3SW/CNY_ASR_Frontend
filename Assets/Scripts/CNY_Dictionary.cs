using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using AICUBE.REST;
using System.Text;
public class CNY_Dictionary : MonoBehaviour
{
    // <summary>
    // Purpose of CNY dictionary is to act as a source to grab the words from backend database ONLY
    // Once we have the database on the local memory, we use createwordList and getrandomword to generate our own words
    // that will be used in game logic.
    // This script SHOULD NOT contain game logic.
    // </summary>
    // 

    public string ip;
    public string port;
    public bool secured;

    public Dictionary<int, string> databaseMapWords;

    private RESTinterface restServer;
    private Options _serverOptions;

    public bool usePsuedoLibrary = true;

    private void Awake()
    {
        setupConnection(new Options(ip, port, secured));
    }

    private static void isStringFound(string a, string b)
    {
        if (a.Contains(b))
        {
            Debug.Log("string l " + a + " contains r " + b);
        }
        else
        {
            Debug.Log("string l " + a + "does not contains r " + b);
        }
    }
    public bool debugOn = false;
    public void setupConnection(Options serverOptions)
    {
        _serverOptions = serverOptions;
        restServer = new RESTinterface(_serverOptions);
        restServer.debugOn = debugOn;
    }
    async public Task<bool> connectDatabase()
    {
        //Debug.Log(restServer.serverInfo.finalUrl);

        if(usePsuedoLibrary)
        {
            Debug.Log("using psudo library");
            databaseMapWords = new Dictionary<int, string>();
            databaseMapWords.Add(0, "恭喜发财");
            databaseMapWords.Add(1, "新年快乐");
            databaseMapWords.Add(2, "万事如意");
            databaseMapWords.Add(3, "心想事成");
            databaseMapWords.Add(4, "年年有余");
            
        }
        foreach(KeyValuePair<int,string> var in databaseMapWords)
        {
            Debug.Log(var.Value);

        }

        return true;


        var connectionResult = await restServer.getJsonData<AICUBE.REST.CNY.CNY_Phrases>("words");
        /*
        //pull list to here. dictionary exist as a model of the MVC
        //databaseMapWords pull from list
        //await connection

        //StringBuilder alltext = new StringBuilder();
        bool isConnected = (connectionResult != null && connectionResult.isConnected);
        if (isConnected)
        {
            databaseMapWords = new Dictionary<int, string>();
            foreach (var line in connectionResult.jsonData.data)
            {
                databaseMapWords.Add(line.word_id, line.word);
                foreach(var str in line.word)
                {
                    alltext.Append("" + str);
                }
                alltext.Append(",\n");
                
                //Debug.Log(line.word_id +"  "+ line.word);
            }
        }

        //Debug.Log(alltext.ToString());
        */
        return connectionResult.isConnected;// databaseMapWords;

    }

    public static KeyValuePair<int, string> GetRandomWORD(Dictionary<int, string> wordList)
    {
        //Dictionary starts from 1 in JSON
        int key = Random.Range(0, wordList.Count) ;
        Debug.Log(key);
        Debug.Log(wordList[key]);
        string value = wordList[key];

        return new KeyValuePair<int, string>(key, value);
    }

    public static List<KeyValuePair<int, string>> CreateUniqueWordList(int wordCount, Dictionary<int, string> wordList)
    {
        List<KeyValuePair<int, string>> newList = new List<KeyValuePair<int, string>>();
        ulong counter = 0;
        Debug.Log(wordList.Count);
        while (newList.Count < wordCount)
        {
            KeyValuePair<int, string> newword = GetRandomWORD(wordList);
            if (!newList.Contains(newword))
            {
                newList.Add(newword);
                ++counter;
            }
        }
        return newList;
    }
    public static List<KeyValuePair<int, string>> CreateTestList()
    {
        List<KeyValuePair<int, string>> newList = new List<KeyValuePair<int, string>>();

        newList.Add(new KeyValuePair<int, string>(0, "HELLO"));
        newList.Add(new KeyValuePair<int, string>(1, "THANKS"));
        newList.Add(new KeyValuePair<int, string>(2, "BYE"));
        newList.Add(new KeyValuePair<int, string>(3, "SORRY"));
        newList.Add(new KeyValuePair<int, string>(1, "I AM GOOD"));
        return newList;
    }


}
