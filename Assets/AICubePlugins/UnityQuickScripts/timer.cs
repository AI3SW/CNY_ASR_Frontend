using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System ;

namespace UnityDecoupledBehavior
{
    [RequireComponent(typeof(Image))]
    public class timer : MonoBehaviour
    {
        float gameTimer;
        /// <summary>
        /// Time in Seconds
        /// </summary>
        float timerDuration = 10;

        [Serializable]
        public enum timerState
        {
            resetted = 0,
            started = 1,
            stopped = 2,
        };


        public timerState timestate
        {
            get;
            private set;
        }
        public Action TimeUpEvent;
        public Action CancelEvent;

        /// <summary>
        /// for sound
        /// </summary>
        public Action TickTockEvent;
        int currentTime;

        Image timerUI;

        float getTimeWeight => ((timerDuration - gameTimer) / timerDuration);
        // Start is called before the first frame update
        void Awake()
        {
            timerUI = GetComponent<Image>();
        }

        public void StartTimer(float timeInSec, Action onTimeUp, Action onCancel)
        {
            if (timestate != timerState.resetted)
            {
                Debug.Log("pls reset Timer");
                return;
            }
            timerDuration = timeInSec;
            TimeUpEvent = onTimeUp;
            CancelEvent = onCancel;
            timestate = timerState.started;
        }
        public void StopTimer()
        {
            if (timestate != timerState.resetted)
            {
                timestate = timerState.stopped;
            }


        }
        public void CancelTimer()
        {
            if (timestate == timerState.started)
            {
                if (gameTimer < timerDuration) CancelEvent?.Invoke();
            }

            StopTimer();
        }
        public void ResetTimer()
        {
            if (timestate == timerState.stopped)
            {
                gameTimer = 0.0f;
                timerUI.fillAmount = getTimeWeight;
                timestate = timerState.resetted;
                Debug.Log("timer is stopped");
            }

        }

        public float elaspedTime => gameTimer;
        // Update is called once per frame

        void Update()
        {
            if (timestate == timerState.started)
            {
                gameTimer += Time.deltaTime;
                timerUI.fillAmount = getTimeWeight;
                if (gameTimer > timerDuration)
                {

                    CancelTimer();
                    TimeUpEvent?.Invoke();
                    return;
                }
                int newTime = Mathf.FloorToInt(gameTimer);
                if (newTime > currentTime)
                {
                    currentTime = newTime;
                    TickTockEvent?.Invoke();
                }
                //Debug.Log(getTimeWeight);

            }
        }
    }
}