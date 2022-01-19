using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityDecoupledBehavior;
public class CNY_UI_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public PageController pageSys;
    
    public PageController.pageName lastPage {
        get =>  (pageSys.current == 1 || pageSys.current == 4) ? PageController.pageName.Game_Screensaver : (PageController.pageName)pageSys.current-1;
    }
    public PageController.pageName nextPage
    {
        get => (pageSys.current == 3 || pageSys.current == 6) ? PageController.pageName.Game_Screensaver : (PageController.pageName)pageSys.current + 1;
    }

    public string winText;
    public string loseText;
    
    public TMPro.TextMeshProUGUI results;

    [SerializeField]
    Button Voucher;
    [SerializeField]
    Button startBtn;

    [SerializeField]
    GameObject Sun;
    [SerializeField]
    GameObject Chest;
    [SerializeField]
    ParticleSystem ChestPS;

    [SerializeField]
    DayNightCycleController DayNightController;

    public TMPro.TextMeshProUGUI skipText;
    [SerializeField]
    SoundManager soundManager;
    #region Game_UI_Public_functions

    [SerializeField]
    [Range(SoundManager.FADE_MIN,SoundManager.FADE_MAX)]
    float ScreensaverVol = 0.3f;

    [SerializeField]
    [Range(SoundManager.FADE_MIN, SoundManager.FADE_MAX)]
    float gameVol = 0.1f;

    [SerializeField]
    [Range(SoundManager.FADE_MIN, SoundManager.FADE_MAX)]
    float mainVol = 0.4f;
    public void ToGame()
    {
        pageSys.transitPage((int)PageController.pageName.Game_Instruction);
    }

    #endregion
    #region WellWishes_UI_Public_functions

    #endregion
    #region Generic_UI_Public_functions

    public void Quit()
    {
        //Debug.Log("quitApp");
        Application.Quit();
    }
    public void Back()
    {
        pageSys.transitPage((int)lastPage);
    }
    public void Next()
    {
        
        pageSys.transitPage((int)nextPage);
    }
    public void onGameStart()
    {
        soundManager.PlayBGM(1, 2);
        soundManager.BGMVolume = gameVol;
        Next();
    }

    public void onSkip(int val)
    {
        skipText.text = val.ToString();

    }


    public void onGameEnd(bool wingame)
    {
        pageSys.transitPage((int)nextPage);
        Debug.Log(wingame);
        if(wingame)
        {
            Debug.Log("win game");
            string[] textArray = winText.Split(' ');
            results.text = textArray[0] +'\n'+ textArray[1];
        } else
        {
            Debug.Log("lose game");
            string[] textArray = loseText.Split(' ');
            results.text = textArray[0] + '\n' + textArray[1];
        }
        
    }
    public void ToScreensaver()
    {
        pageSys.transitPage((int)PageController.pageName.Game_Screensaver);
        soundManager.PlayBGM(0, 2);
        soundManager.BGMVolume = ScreensaverVol;
    }
    #endregion


    void Start()
    {

        pageSys.activatePage(pageSys.firstPage);

    }

    async public void onOpenChest()
    {
        ChestPS.gameObject.SetActive(true);
        Voucher.interactable = false;
        await Task.Delay(4000);
        pageSys.transitPage((int)PageController.pageName.Game_Screensaver);
        ChestPS.gameObject.SetActive(false);
        Voucher.interactable = true;
        DayNightController.ResetMainScore();
        DayNightController.saveMainScore();

    }

    public void onPrizePage()
    {
        pageSys.transitPage((int)PageController.pageName.Game_Prize);
        Voucher.interactable = true;
        Sun.SetActive(true);
        Chest.SetActive(false);
        DayNightController.hideCows();
    }

    public void onBarMax(float value)
    {
        if (value >= 1f)
        {

            Sun.SetActive(false);
            Chest.SetActive(true);

        }
    }
}
