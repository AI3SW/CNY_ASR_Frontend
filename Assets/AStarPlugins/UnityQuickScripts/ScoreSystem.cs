using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDecoupledBehavior
{
    [RequireComponent(typeof(Image))]
    public class ScoreSystem : MonoBehaviour
    {
        [SerializeField]
        string filename = "CNY";
        [SerializeField]
        string LOG_DIR = "/Astar/";
        [SerializeField]
        string FILE_EXT = ".txt";
        string LOG_PATH => LOG_DIR + filename + FILE_EXT;
        // Start is called before the first frame update
        public float score
        {
            get => targetScore;
            private set
            {
                inTransit = true;
                targetScore = Mathf.Clamp(value, 0.00001f, 1f); //setting the value

                diff = targetScore - currentScore;
                lerpAmount = (diff / transit_duration) * Time.deltaTime;
                elapsed_duration = 0;
            }
        }

        public Image imgToInterpolate;
        public UnityEventFloat callback;
        [SerializeField]
        float currentScore;
        [Range(0.00001f, 1f)]
        [SerializeField]
        float targetScore;
        [SerializeField]
        bool inTransit;

        float transit_duration;
        float elapsed_duration;
        float lerpAmount;
        float diff;

        public setRectTransformToObject objectToShift;
        private void Awake()
        {
            imgToInterpolate = GetComponent<Image>();
            objectToShift.Init();
        }
        void Start()
        {

        }

        public void loadScore()
        {
            string[] data = Astar.Utils.IOUtils.loadStringLinesfromFile(LOG_PATH);
            if (data != null && data.Length > 0)
            {

                setScore(float.Parse(data[0]));
                Debug.Log("loaded with " + score);
            }
            else
            {
                resetScore();
                Debug.Log("unloaded");
            }

        }

        public void saveScore()
        {

            if (Astar.Utils.IOUtils.saveDataToFile(LOG_DIR, LOG_PATH, targetScore.ToString()))
            {
                Debug.Log("file saved sucessfuly");
            }
            else
                Debug.Log("file failed to save sucessfuly");
        }

        // Update is called once per frame
        void Update()
        {
            if (inTransit)
            {
                elapsed_duration += Time.deltaTime;
                //interpolate

                currentScore += lerpAmount;
                float newdiff = targetScore - currentScore;
                if (elapsed_duration >= transit_duration)
                {
                    currentScore = targetScore;
                    inTransit = false;
                    callback?.Invoke(currentScore);
                }
                imgToInterpolate.fillAmount = currentScore;
                objectToShift.shiftPosition(currentScore);
            }
        }

        public void addScore(float newscore, float transitDuration)
        {
            transit_duration = transitDuration;
            score += newscore;
        }
        public void setScore(float newscore)
        {
            currentScore = newscore;
            targetScore = newscore;
            if (imgToInterpolate == null) imgToInterpolate = GetComponent<Image>();
            imgToInterpolate.fillAmount = newscore;
            //objectToShift.shiftPosition(newscore);
            inTransit = false;
            callback?.Invoke(newscore);
        }
        public void resetScore()
        {
            setScore(0f);
            objectToShift.shiftPosition(targetScore);
        }
    }
}