using System;
using UnityEngine;
using TMPro;

public class TimerController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText; // use UI text that displays the countdown

    // amount of time left on timer
    private float timeRemaining;

    // sees if timer is counting down
    private bool running = false;

    // callback function that runs when the timer = 0
    private Action onExpired;

    void Update()
    {
        // nothing if the timer is not active
        if (!running) return;

        // sub the time passed since the last frame
        timeRemaining -= Time.deltaTime;

        // see if the timer finished
        if (timeRemaining <= 0f)
        {
            // keep time to 0 so it never shows negative values
            timeRemaining = 0f;

            // stop timer
            running = false;

            // refresh UI one more time
            UpdateDisplay();

            // call the expiration callback if one was provided
            onExpired?.Invoke();
        }
        else
        {
            // update timer and display each frame while running
            UpdateDisplay();
        }
    }

    // start timer with a given number of seconds
    // expiredCallback is the method to run when time runs out
    public void StartTimer(float seconds, Action expiredCallback)
    {
        timeRemaining = seconds;
        onExpired = expiredCallback;
        running = true;

        // update UI so timer shows the starting value
        UpdateDisplay();
    }

    // stops/pause timer without resetting remaining time
    public void StopTimer()
    {
        running = false;
    }

    // updates text and color shown on the UI
    private void UpdateDisplay()
    {
        // avoid errors if no text object is assigned
        if (timerText == null) return;

        // round up for more time natural to players
        int secs = Mathf.CeilToInt(timeRemaining);

        // show the remaining time
        timerText.text = $"Time: {secs}s";

        // text color to red when under 5 seconds, otherwise white
        timerText.color = secs <= 5 ? Color.red : Color.white;
    }
}