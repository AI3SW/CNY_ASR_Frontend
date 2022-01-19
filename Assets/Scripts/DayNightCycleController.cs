using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityDecoupledBehavior;
public class DayNightCycleController : MonoBehaviour
{
    [SerializeField]
    List<CowController> cowsToChange;

    List<CowController> SleepingCows;
    List<CowController> StandingCows;
    List<CowController> DancingCows;

    [SerializeField]
    Color nightColor;

    [SerializeField]
    Image _Sky;

    [SerializeField]
    Image _Forground;

    [SerializeField]
    Image _Ground;

    [SerializeField]
    ScoreSystem mainscore;

    [SerializeField]
    ScoreSystem gamescore;

    [SerializeField]
    PostProcessVolume ppvol_Home;

    [SerializeField]
    PostProcessVolume ppvol_Game;
    
    public bool useGameBar = false;
    // Start is called before the first frame update
    void Awake()
    {
        useGameBar = false;
        
        //mainscore.resetScore();
    }
    void Start()
    {
        intializeCowSize();
        
        //mainscore.resetScore();
    }

    void intializeCowSize()
    {
        foreach( CowController cow in cowsToChange)
        {
            //cow.setSize(cow.widthPercent * Screen.width);
            cow.setState(CowController.Cowstate.sleep);
        }
        SleepingCows = new List<CowController>(cowsToChange);
        SleepingCows.RemoveAt(0);
        StandingCows = new List<CowController>();
        DancingCows = new List<CowController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(useGameBar)
            setDayDawnWeight(gamescore.score);
         else
            setDayNightWeight(mainscore.score);
    }

    float usedWeights = 0;
    const float startweight = 0f;
    void setDayNightWeight(float weight)
    {

        float newweight = weight * weight;
        float endweight = 0.7f;
        ppvol_Home.weight = Mathf.Lerp(endweight, startweight, newweight / endweight);

        Color currColor = nightColor;
        currColor.r = Mathf.Lerp(nightColor.r, 1, newweight);
        currColor.g = Mathf.Lerp(nightColor.g, 1, newweight);
        currColor.b = Mathf.Lerp(nightColor.b, 1, newweight);

        _Ground.color = currColor;
        _Sky.color = currColor;
        _Forground.color = currColor;

        int totalStates = cowsToChange.Count * 3; //total states
        if (newweight > usedWeights)
        {
            usedWeights += 1f / (float)totalStates;
            arouseCowRandomly();
        }
        //Debug.Log(weight);
        cowsToChange[0].setByWeightState(Mathf.Lerp(startweight, endweight, weight / endweight));
    }
    void setDayDawnWeight(float weight)
    {
        float endweight = 0.8f;
        float newweight = weight;// * weight;
        ppvol_Game.weight = Mathf.Lerp(startweight, endweight, newweight / endweight);
        cowsToChange[0].setByWeightState(ppvol_Game.weight);
    }
    public void ResetMainScore()
    {
        cowsToChange[0].gameObject.SetActive(true);
        mainscore.resetScore();
    }
    public void addMainScore(float val,float time)
    {
        Debug.Log(val);
        mainscore.addScore(val, time);
    }
    public void saveMainScore()
    {
        mainscore.saveScore();
    }
    public void loadMainScore()
    {
        mainscore.loadScore();
    }
    public void hideCows()
    {
        for (int i = 0; i < cowsToChange.Count; ++i)
        {
            cowsToChange[i].gameObject.SetActive(false);
        }
    }

    public void useMainScore()
    {
        float tempScore = mainscore.score;
        mainscore.resetScore();
        mainscore.addScore(tempScore, 2);
        useGameBar = false;
        ppvol_Game.gameObject.SetActive(useGameBar);
        ppvol_Home.gameObject.SetActive(!useGameBar);
        for (int i = 0; i < cowsToChange.Count; ++i)
        {
            if (i == 0)
            {

                cowsToChange[i].setState(CowController.Cowstate.sleep);
                cowsToChange[i].gameObject.SetActive(true);
            }
            else
            {
                cowsToChange[i].setState(CowController.Cowstate.sleep);
                cowsToChange[i].gameObject.SetActive(false);
            }
        }
    }


    public void useGameScore()
    {
        useGameBar = true;
        ppvol_Game.gameObject.SetActive(useGameBar);
        ppvol_Home.gameObject.SetActive(!useGameBar);
        Color currColor = Color.white;

        _Ground.color = currColor;
        _Sky.color = currColor;
        _Forground.color = currColor;
        SleepingCows = new List<CowController>(cowsToChange);
        SleepingCows.RemoveAt(0);
        //Debug.Log(SleepingCows.Count);
        DancingCows.Clear();
        StandingCows.Clear();
        for (int i = 0; i < cowsToChange.Count; ++i)
        {
            if(i != 0)
            {
                
                cowsToChange[i].setState(CowController.Cowstate.sleep);
                cowsToChange[i].gameObject.SetActive(true);
            }
            else
            {
                cowsToChange[i].setState(CowController.Cowstate.sleep);
                cowsToChange[i].gameObject.SetActive(false);
            }
        }
    }

    public void ResetGameScore()
    {
        gamescore.resetScore();
    }
    public void addGameScore(float val, float time)
    {
        gamescore.addScore(val, time);
        int randomTimes = Random.Range(2, cowsToChange.Count/2);
        for (int i = 0; i < randomTimes; ++i )
        {
            arouseCowRandomly();
        }

    }

    public void arouseCowRandomly()
    {
        CowController randomCow = null;
        int randomState = Random.Range(0, 2);
        if (randomState == (int)CowController.Cowstate.sleep)
        {
            randomCow = ArouseCowFromSleeping();
            if(randomCow == null)
            {
                randomCow = ArouseCowFromStanding();
            }
        } else {
            randomCow = ArouseCowFromStanding();
            if (randomCow == null)
            {
                randomCow = ArouseCowFromSleeping();
            }
        }
        if(randomCow != null)
        {

            randomCow.nextState();
        } else
        {
            //Debug.Log("no cows found");
        }
    }

    CowController ArouseCowFromStanding()
    {
        return ArouseCowFromList(StandingCows, DancingCows);
    }
    CowController ArouseCowFromSleeping()
    {
        return ArouseCowFromList(SleepingCows, StandingCows);
    }
    CowController ArouseCowFromList( List<CowController> CowListInput, List<CowController> CowListOutput)
    {
        CowController randomCow = null;
        if (CowListInput.Count == 0) return randomCow;
        int randomInt = Random.Range(0, CowListInput.Count);
        randomCow = CowListInput[randomInt];
        CowListOutput.Add(randomCow);
        CowListInput.RemoveAt(randomInt);
        return randomCow;
    }
}
