using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEzExp;


public class RunExperiment : MonoBehaviour
{

    //  string[] columnsResult = new string[] { "Arousal", "Valence", "Confidence", "Emotion1", "Emotion2" };

    Experiment _experiment;

    [Space]
    [Header("Experience")]
    public string currentUserId = "1";
    public int currentTrialIndex = -1;

    public string inputDataPath;
    public string outputDataPath;



    void Start()
    {

    }
 

    public void StartExperiment(string userID, int trialID, int startWith, bool skipTraining, bool forceAvatar = false)
    {

        _experiment = new Experiment(inputDataPath, userID, trialID, "Subject");
        string outputFilePath = outputDataPath + userID + "-results.csv";
        _experiment.SetOutputFilePath(outputFilePath);

        // This is the results you want 
        _experiment.SetResultsHeader(new string[] {"speed", "accuracy"});

        Debug.Log("Output path : <color=#E91E63>" + outputFilePath + "</color>");

        Debug.Log("<color=#E91E63>Current userId : " + currentUserId + "</color>");

        NextTrial();
    }

    public void NextTrial()
    {
        // We try to load the next trial variables
        try
        {
            Trial t = _experiment.LoadNextTrial();
        }
        catch (AllTrialsPerformedException e)
        {
            ExperienceFinished();
            return; //info temporary
        }

        _experiment.StartTrial();
        currentTrialIndex = _experiment.GetCurrentTrialIndex();

        Debug.Log("<color=#E91E63>Current trial : " + currentTrialIndex + "</color>");

        // We read the value of the CSV : 
        int color = int.Parse(_experiment.GetParameterData("Color"));
        string shape = _experiment.GetParameterData("Shape");

        // Here you start your trial based on what you get :
        Debug.Log("Loading the shape " + shape + " with the color id " + color);
    }

    public void TrialEnded()
    {
        Debug.Log("The trial has ended. You can display a panel with the questions.");
    }


    public void SetResults(int speed, float accuracy)
    {
        // The result data correspond to _experiment.SetResultsHeader
        _experiment.SetResultData("speed", speed.ToString());
        _experiment.SetResultData("accuracy", accuracy.ToString());
        Debug.Log("Setting the results");
        _experiment.EndTrial();
    }

    void ExperienceFinished()
    {
      Debug.Log("The experience is finished");
    }


    #region UI
    public void StartButtonClicked()
    {
        StartExperiment(currentUserId, 0, 0, false);
    }


    public void RandomResults()
    {
        SetResults((int)Random.Range(0,100), Random.Range(0, 1));
    }


    #endregion

    public void ApplicationStop() { }
}
