using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityDecoupledBehavior;
using UnityEngine.UI;
using UnityEngine.Events;

public class CNY_GameManager : MonoBehaviour
{
    CNY_Dictionary _Dictionary;
    List<KeyValuePair<int, string>> list;

    public UnityEvent OnGameStart;
    public UnityEvent OnNewGameFailed;
    public UnityEvent OnNewGameSuccess;
    public UnityEventBool OnGameEnd;

    [SerializeField]
    DayNightCycleController DayNightController;
    [SerializeField]
    IdiomProunounciationGuide guide;

    #region GAME_VAR
    [SerializeField]
    [Range(0, 20)]
    private int WordCountPerGame = 20;


    [SerializeField]
    [Range(0, 20)]
    private int wordsToWin = 20;
    private Counter WordsIndentified;
    [SerializeField]
    //RectTransform passIndicator;
    private float ScoreCountPerWord => (float) 1f/ (float)wordsToWin;
    [SerializeField]
    private int numberOfGamesToWinVoucher = 7;
    private float ScoreCountPerGame => (float)1f / (float) numberOfGamesToWinVoucher;

    [SerializeField]
    private int SkipChances = 2;
    private Counter SkipCounter;
    [SerializeField]
    private UnityEventInt onSkip;
    /// <summary>
    /// units in seconds
    /// </summary>
    [SerializeField]
    private float secPerWord = 10;

    #endregion

    int CurrentSentenceId;
    public timer _timer;


    private string sessionID;

    private bool _gamePause = false;

    [SerializeField]
    private TextMeshProUGUI phraseToSay;

    [SerializeField]
    private AICUBE_ASR asrServer;

    [SerializeField]
    SoundManager soundManager;
    public enum gamestate
    {
 
        gameInitiated = 0,
        gameStarted = 1,
        gameEnded = 2,
    }
    [SerializeField]
    Button onDbReadyActivate;
    public gamestate currentState
    {
        get;
        private set;
    }

    public void GameStarting()
    {
        if (currentState != gamestate.gameInitiated) return;
        //clearPartialSentenceStack();
        _gamePause = false;
        CurrentSentenceId = -1;

        currentState = gamestate.gameStarted;
        nextWord(); // FIRST CALL OF NEXTWORD
        

        DayNightController.useGameScore();
        OnGameStart?.Invoke();// to call the UI elements i want to decouple gamemanager and ui manager


        Debug.Log("GAME STARTING");
        //string word = ;
    }

    /// <summary>
    /// Ends connection with ASR
    /// </summary>
    public async void GameEnd()
    {
        if (currentState != gamestate.gameStarted) return;
        //phraseToSay.text = "成功唤醒!";
        currentState = gamestate.gameEnded;
        if (WordsIndentified._value >= wordsToWin)
        {
            phraseToSay.text = "成功唤醒!";
        }
        else
        {
            phraseToSay.text = "再接再厉!";
        }
        await Task.Delay(2000);

        if (WordsIndentified._value >= wordsToWin)
        {
            DayNightController.addMainScore(ScoreCountPerGame, 1f);
        }
        OnGameEnd?.Invoke(WordsIndentified._value >= wordsToWin);
        
        DayNightController.saveMainScore();
        asrServer.DisconnectToWebSocket();
        Debug.Log("RESULTS");
    }

    /// <summary>
    /// starts connection with asr
    /// </summary>
    public async void NewGame()
    {

        if (currentState != gamestate.gameEnded) return;
        WordsIndentified.resetCounter();
        SkipCounter.resetCounter();
        DayNightController.ResetGameScore();
        currentState = gamestate.gameInitiated;
        //list = CNY_Dictionary.CreateTestList();

        Task<bool> waitingConnection = asrServer.ConnectToWebSocket();
        await waitingConnection;
        if(waitingConnection.Result)
        {
            list = CNY_Dictionary.CreateUniqueWordList(WordCountPerGame, _Dictionary.databaseMapWords);

            OnNewGameSuccess?.Invoke();
            //Debug.Log("test");
        } else
        {
            currentState = gamestate.gameEnded;
            OnNewGameFailed?.Invoke();
        }

        //Debug
        Debug.Log("not restarting");
    }

    public void GameContinue()
    {
        //clearPartialSentenceStack();
        changeCurrentWord();

        Debug.Log("GAME Paused");
        _gamePause = false;
        //string word = ;
    }
    public void GamePause()
    {
        _gamePause = true;
        //clearPartialSentenceStack();
        //changeCurrentWord();
        _timer.StopTimer();
        _timer.ResetTimer();
        //_timer.StartTimer(secPerWord, OnTimeUp, nextWord);
        Debug.Log("GAME Paused");
        //string word = ;
    }

    public void onReceiveASR_Results(bool isPartial, string data, ulong id)
    {
        //currentUtteranceOffsetId = currentUtteranceId;
        if (currentState != gamestate.gameStarted) return;

        //Debug.Log(data);
        //Trimming white space within words
        data = data.Trim();
        string[] linesSeperateBySpace = data.Split(' ');
        data = "";
        for (int i =0; i < linesSeperateBySpace.Length;++i)
        {
            data += linesSeperateBySpace[i].Trim();
        }
        // end of trim
        bool isFound = data.Contains(getCurrentWord);

        if (!isPartial)
        {//Full
            //text.text += data;
            Debug.Log("[Full] UttID=" + id + ", Partial, " + data);
            textbox.text = "Debug : " + "[Partial] UttID=" + id + ", Partial, " + data;
        }
        else
        {//partial
            Debug.Log("[Partial] UttID=" + id + ", Partial, " + data);
            textbox.text = "Debug : " + "[Partial] UttID=" + id + ", Partial, " + data;
        }


        if (isFound)
        {
            WordsIndentified.IncreaseCounter();
            nextWord();
            soundManager.PlayOneShot(0);
            //debug.Log("found");
        } else
        {
            
            //Debug.Log("not found");
        }
        /*
        if (!isPartial)
        {//Full

            bool isFound = data.Contains(getCurrentWord);
            
            if (isFound)
            {
                OnWordFound();
                nextWord();
            }
        }
        else
        {//partial
            //lock (_asyncLockStack)
            {
                partialSentence.Push(data);
            }

        }

        */
        //fordebug

    }
    public TextMeshProUGUI textbox;
    private void Awake()
    {

        currentState = gamestate.gameEnded;
        //partialSentence = new Stack<string>();
        //_timer = GetComponent<timer>();
       
        DayNightController.loadMainScore();
        DayNightController.useMainScore();
        SkipCounter = new Counter(SkipChances);
        SkipCounter.CounterCallback.AddListener((val)=> { onSkip?.Invoke(val); });

        WordsIndentified = new Counter();
        WordsIndentified.CounterCallback.AddListener((val) => {
            DayNightController.addGameScore(ScoreCountPerWord, 0.5f); 
        });
        
        wordsToWin = Mathf.Clamp(wordsToWin, 0, WordCountPerGame);

        float passWeight = (float)wordsToWin / (float)WordCountPerGame;
        //passIndicator.anchorMin = new Vector2( passWeight, passIndicator.anchorMin.y);
        //passIndicator.anchorMax = new Vector2( passWeight, passIndicator.anchorMax.y);
        //passIndicator.anchoredPosition = new Vector3(0, 0, 0);
    }

    // Start is called before the first frame update
    async void Start()
    {
        onDbReadyActivate.interactable = false;
        _Dictionary = GetComponent<CNY_Dictionary>();
        
        Task<bool> connectionWithDB = _Dictionary.connectDatabase();
        await connectionWithDB;
        onDbReadyActivate.interactable = true;



    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            DayNightController.addMainScore(1f, 1f);
        }
    }
    async void nextWord()
    {
        if (currentState != gamestate.gameStarted) return;
        Debug.Log("sentence id : "+ CurrentSentenceId + " word count per game : " + WordCountPerGame);

        if ((int)CurrentSentenceId < WordCountPerGame)
        {
            if (!_gamePause)
            {
                CurrentSentenceId = (CurrentSentenceId + 1 <= WordCountPerGame) ? CurrentSentenceId +1 : CurrentSentenceId;
            }
            _timer.StopTimer(); // calling recursivelly
            

            //clearPartialSentenceStack();
            //Debug.Log("test");

            if (CurrentSentenceId < WordCountPerGame && WordsIndentified._value < wordsToWin)
            {
                _timer.ResetTimer();
                phraseToSay.text = list[CurrentSentenceId].Value;
                asrServer.sendNewWord(list[CurrentSentenceId].Key, CurrentSentenceId);
                //Debug.Log(list[CurrentSentenceId].Value);
                guide.playSpeech(list[CurrentSentenceId].Key);
                await Task.Delay(1000);
                _timer.StartTimer(secPerWord, OnTimeUp, nextWord);
            }
            else
            {
                
                GameEnd();
            }
            
            

        } else
        {
            await Task.Delay(2000);
            GameEnd();
        }
    }
    void OnTimeUp()
    {
        bool isFound = false;
        //lock (_asyncLockStack)
        /*
        {
            while (partialSentence.Count > 0 && !isFound)
            {
                string stackPeek = partialSentence.Pop();
                isFound = stackPeek.Contains(getCurrentWord);
            }
            partialSentence.Clear();
        }
        */
        handleResult(isFound);
        soundManager.PlayOneShot(1);
    }

    void handleResult(bool isWordSame)
    {
        if (isWordSame)
        {
            WordsIndentified.IncreaseCounter();
            nextWord();
            //Debug.Log("win");
        }
        else
        {
            nextWord();
            //Debug.Log("lose");
        }
    }

    //called when session has started


    void clearPartialSentenceStack()
    {
        //lock (_asyncLockStack)
        {
            //partialSentence.Clear();
        }
    }
    string getCurrentWord => list[CurrentSentenceId].Value;

    public void changeCurrentWord()
    {
        if(SkipCounter._value > 0)
        {
            KeyValuePair<int, string> newword = CNY_Dictionary.GetRandomWORD(_Dictionary.databaseMapWords);
            list[CurrentSentenceId] = newword;
            phraseToSay.text = list[CurrentSentenceId].Value;
            guide.playSpeech(list[CurrentSentenceId].Key);
            asrServer.sendNewWord(list[CurrentSentenceId].Key, CurrentSentenceId);
            _timer.StartTimer(secPerWord, OnTimeUp, nextWord);
            SkipCounter.DecreaseCounter();
        }

    }

    string getLastWord => (list.Count-1 > 0) ? list[(CurrentSentenceId - 1)].Value : "";



}
