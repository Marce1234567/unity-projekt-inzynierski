using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;

    [Header("Finish UI")]
    public TMP_Text finalTimeText;
    public TMP_Text rankText;

    [Header("Rank Times")]
    public float rankSTime = 30f;
    public float rankATime = 45f;
    public float rankBTime = 60f;

    private float currentTime = 0f;
    private bool timerRunning = true;

    void Update()
    {
        if (!timerRunning)
            return;

        currentTime += Time.deltaTime;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Czas: " + FormatTime(currentTime);
    }

    public void StopTimer()
    {
        timerRunning = false;

        string finalTime = FormatTime(currentTime);
        string rank = GetRank();

        if (finalTimeText != null)
            finalTimeText.text = "Czas: " + finalTime;

        if (rankText != null)
            rankText.text = "Ranga: " + rank;
    }

    string GetRank()
    {
        if (currentTime <= rankSTime)
            return "S";

        if (currentTime <= rankATime)
            return "A";

        if (currentTime <= rankBTime)
            return "B";

        return "C";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}