using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEzExp;

public class ControllerManager : MonoBehaviour {

    Experiment _experiment;
   
    static int userResponse = 50;
    
    double tempUserResponse = userResponse;

    public int currentTrialIndex = -1;
    public string inputDataPath;
    public string outputDataPath;
    public Text outputUserResponse;
    public GameObject preExperimentCanvas;
    public GameObject leftStimulus;
    public GameObject rightStimulus;
    public Sprite lenghtSprite;
    public Sprite areaSprite;

    //Initializing as 0 for direct scene testing as this variable is set at the index scene
    private string currentUserId = "1";
    private SpriteRenderer spriteRenderer;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    RunExperiment _runExperiment;

    void Start()
    {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        PlayerPrefs.SetInt("userResponse", 50);
        userResponse = PlayerPrefs.GetInt("userResponse");
        currentUserId = PlayerPrefs.GetString("participant");
    }
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        float controllerValue = device.GetAxis().y;
        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (controllerValue > 0 && userResponse < 100)
            {
                tempUserResponse+= 0.1;
            } else if (controllerValue < 0 && userResponse > 0)
            {
                tempUserResponse -= 0.1;
            }
            userResponse = (int)System.Math.Round(tempUserResponse, System.MidpointRounding.AwayFromZero);
            outputUserResponse.text = userResponse.ToString() + "%";
            PlayerPrefs.SetInt("userResponse", userResponse);
        } else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (preExperimentCanvas.activeSelf)
            {
                StartExperiment(currentUserId, 0, 0, false);
                preExperimentCanvas.SetActive(false);
            }
        }
    }

    public void StartExperiment(string userID, int trialID, int startWith, bool skipTraining, bool forceAvatar = false)
    {

        _experiment = new Experiment(inputDataPath, userID, trialID, "Subject");
        string outputFilePath = outputDataPath + userID + "-results.csv";
        _experiment.SetOutputFilePath(outputFilePath);

        // This is the results you want 
        _experiment.SetResultsHeader(new string[] { "speed", "accuracy" });

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

        // We read the value of the CSV : 
        float ratio = float.Parse(_experiment.GetParameterData("Ratio"));
        string property = _experiment.GetParameterData("Property");
        float propertySize;

        _experiment.StartTrial();
        currentTrialIndex = _experiment.GetCurrentTrialIndex();

        Debug.Log("<color=#E91E63>Current trial : " + currentTrialIndex + "</color>");

        

        if (property == "Length")
        {
            // Scale used by this property to fit the paper and make the right positioning possible
            propertySize = 0.5f;

            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = lenghtSprite;

            leftStimulus.transform.localScale = new Vector3(propertySize, propertySize, propertySize);
            leftStimulus.transform.position = new Vector3(0.048f, 0.012f, 0.063f);

            rightStimulus.transform.localScale = new Vector3(propertySize, propertySize * ratio, propertySize);
            rightStimulus.transform.position = new Vector3(-0.164f, 0.012f, 0.043f);

        } else if (property == "Area")
        {
            rightStimulus.transform.localScale = new Vector3(ratio, ratio, ratio);
        }
        

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


    public void RandomResults()
    {
        SetResults((int)Random.Range(0, 100), Random.Range(0, 1));
    }

    public void ApplicationStop() { }
}