using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UnityDecoupledBehavior
{
    public class PageController : MonoBehaviour
    {
        public List<CanvasGroup> pageList;
        public int current
        {
            get;
            private set;
        }
        bool inTransition;
        public CanvasGroup firstPage;

        [System.Serializable]
        public enum pageName
        {
            Game_Screensaver = 0,
            Game_Instruction,
            Game_Game,
            Game_Result,
            Game_Prize,
            WellWishes_Send,
            WellWishes_Result,
        }

        // Start is called before the first frame update
        void Awake()
        {
            foreach (CanvasGroup page in pageList)
            {
                deactivatePage(page);
            }
            firstPage = pageList[current];
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                transitPage((int)pageName.Game_Screensaver);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                transitPage((int)pageName.Game_Instruction);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                transitPage((int)pageName.Game_Game);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                transitPage((int)pageName.Game_Result);
            }*/
        }

        public void transitPage(int nextPageId)
        {
            if ((int)nextPageId == current || inTransition) return;
            else
            {
                inTransition = true;
                CanvasGroup currPage = pageList[current];

                currPage.DOFade(0, 0.5f).OnComplete(() =>
                {
                    currPage.gameObject.SetActive(false);
                    inTransition = false;
                });
                currPage.blocksRaycasts = false;
                currPage.interactable = false;

                CanvasGroup nextPage = pageList[(int)nextPageId];
                nextPage.gameObject.SetActive(true);
                nextPage.DOFade(1, 0.5f).OnComplete(() =>
                {
                    nextPage.blocksRaycasts = true;
                    nextPage.interactable = true;
                    inTransition = false;
                });
                current = (int)nextPageId;
            }
        }

        void deactivatePage(CanvasGroup page)
        {
            page.alpha = 0;
            page.blocksRaycasts = false;
            page.interactable = false;
            page.gameObject.SetActive(false);
        }
        public void activatePage(CanvasGroup page)
        {
            page.alpha = 1;
            page.blocksRaycasts = true;
            page.interactable = true;
            page.gameObject.SetActive(true);
        }
    }

}