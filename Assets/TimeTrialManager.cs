using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TimeTrialManager : MonoBehaviour
{
    private bool trialStarted;
    private bool checkpointsCompleted;

    private float timer;
    private float newTime;
    private float bestTime;

    [SerializeField] Transform checkpointsParent;
    [SerializeField] List<GameObject> checkpoints;
    [SerializeField] List<GameObject> activatedCheckpoints;

    [SerializeField] TMP_Text currentTimeText;
    [SerializeField] TMP_Text newTimeText;
    [SerializeField] TMP_Text bestTimeText;

    int minutes;
    int seconds;
    int milliseconds;


    private int amountOfCheckpoints;

    private void Start()
    {
        for (int i = 0; i < checkpointsParent.childCount; i++)
        {
            Transform childTransform = checkpointsParent.GetChild(i);
            checkpoints.Add(childTransform.gameObject);
        }

        if (checkpoints.Count != 0) amountOfCheckpoints = checkpoints.Count;
        else checkpointsCompleted = true;

        newTimeText.text = "Current Run: " + string.Format("{0:00}:{1:00}:{2:00}", 0, 0, 0);
        bestTime = PlayerPrefs.GetFloat("Best Time");
        if (bestTime != 0)
        {
            minutes = Mathf.FloorToInt(bestTime / 60); // Calculate whole minutes
            seconds = Mathf.FloorToInt(bestTime % 60); // Calculate remaining seconds
            milliseconds = Mathf.FloorToInt((bestTime * 100) % 100); // Calculate remaining milliseconds
            bestTimeText.text = "Best Run: " + string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
        else bestTimeText.text = "Best Run: " + string.Format("{0:00}:{1:00}:{2:00}", 0, 0, 0);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetTrial(); 


        if (trialStarted) timer += Time.deltaTime;
        minutes = Mathf.FloorToInt(timer / 60); // Calculate whole minutes
        seconds = Mathf.FloorToInt(timer % 60); // Calculate remaining seconds
        milliseconds = Mathf.FloorToInt((timer * 100) % 100); // Calculate remaining milliseconds
        currentTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void StartTrial()
    {
        if (trialStarted) return;
        ResetTrial();
        trialStarted = true;
    }

    public void EndTrial()
    {
        if (!checkpointsCompleted) return;
        trialStarted = false;
        newTime = timer;
        newTimeText.text = "Current Run: " + string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        if (bestTime == 0) SetBestTime();
        else if (newTime < bestTime) SetBestTime();

    }

    private void SetBestTime()
    {
        bestTime = timer;
        bestTimeText.text = "Best Run: " + string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        PlayerPrefs.SetFloat("Best Time", bestTime);
        PlayerPrefs.Save();
    }

    public void ActivateCheckpoint(GameObject checkpointObject)
    {
        if (!trialStarted) return;
        // Check if the checkpointObject exists in the checkpoints list and hasn't been activated yet
        if (checkpoints.Contains(checkpointObject) && !activatedCheckpoints.Contains(checkpointObject))
        {
            activatedCheckpoints.Add(checkpointObject); // Mark checkpoint as activated
            if (activatedCheckpoints.Count == amountOfCheckpoints) checkpointsCompleted = true;
        }
    }

    public void ResetTrial()
    {
        trialStarted = false;
        timer = 0;
        checkpointsCompleted = false;
        activatedCheckpoints.Clear();
    }
}
