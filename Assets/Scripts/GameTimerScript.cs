using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Game;
namespace Timer
{
    public class StageTimer : MonoBehaviour
    {
        private bool paused = false;
        private float stageTimeSeconds = 20;
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

        public void Setup(float timeSeconds){
            timeElapsed = 0f;
            stageTimeSeconds = timeSeconds;
            lastDisplayedSeconds = -1;
            UpdateTimerText();
            paused = true;
        }
        public void Toggle()
        {
            paused = !paused;
        }
        void Update()
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
                    timerText.text = new string("YOU LOSE.");
                    paused = true;
                    return;
                }
                UpdateTimerText();
                lastDisplayedSeconds = currentDisplayedSeconds;
            }
        }

        void UpdateTimerText()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeElapsed);
            string formattedTime = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            timerText.text = formattedTime;
        }
    }
}