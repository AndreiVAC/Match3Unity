using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Level;
namespace Timer
{
    public class StageTimer : MonoBehaviour
    {
        private bool paused = false;
        private float stageTimeSeconds = 45;
        private int currentDisplayedSeconds = 0;
        public TextMeshProUGUI timerText;
        private float timeElapsed;
        private int lastDisplayedSeconds = -1;

        public static StageTimer Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            timeSpanTotal = TimeSpan.FromSeconds(stageTimeSeconds);
            timeElapsed = 0f;
            UpdateTimerText();
        }

        public void ObjectMoved()
        {
            if(paused && currentDisplayedSeconds < stageTimeSeconds)
            {
                paused = false;
            }
        }

        TimeSpan timeSpanTotal;
        public void Setup(float timeSeconds){
            timeElapsed = 0f;
            stageTimeSeconds = timeSeconds;
            lastDisplayedSeconds = -1;
            UpdateTimerText();
            paused = true;
            timeSpanTotal = TimeSpan.FromSeconds(stageTimeSeconds);
        }
        public void Toggle()
        {
            paused = !paused;
        }
        private void Update()
        {
            if (paused)
            {
                return;
            }
            timeElapsed += Time.deltaTime;

            currentDisplayedSeconds = Mathf.FloorToInt(timeElapsed);

            if (currentDisplayedSeconds != lastDisplayedSeconds)
            {
                if (currentDisplayedSeconds > stageTimeSeconds)
                {
                    Debug.Log("YOU LOSE, time has run out. Reloading stage...");
                    Level.Manager.Instance.LoadCurrentStage();
                    paused = true;
                    return;
                }
                UpdateTimerText();
                lastDisplayedSeconds = currentDisplayedSeconds;
            }
        }

        private void UpdateTimerText()
        {
            timerText.text = GetTime();
        }
        public string GetTime()
        {
            TimeSpan timeSpanElapsed = TimeSpan.FromSeconds(timeElapsed);
            string formattedTime = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", timeSpanElapsed.Minutes, timeSpanElapsed.Seconds,
            timeSpanTotal.Minutes,timeSpanTotal.Seconds);
            return formattedTime;
        }
    }
}