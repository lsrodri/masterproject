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
    public Text outputConfirmation;
    public GameObject preExperimentCanvas;
    public GameObject confirmationCanvas;
    public GameObject invalidInputCanvas;
    public GameObject leftStimulus;
    public GameObject rightStimulus;
    public GameObject horizontalBar;
    public Sprite lenghtSprite;
    public Sprite areaSprite;
    public Sprite barSprite;

    //Initializing as 0 for direct scene testing as this variable is set at the index scene
    private string currentUserId = "1";
    private SpriteRenderer leftSpriteRenderer;
    private SpriteRenderer rightSpriteRenderer;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    private bool userHasEstimated = false;
    private float tempTime;
    private float trialTime;

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
            // Flag that user has entered a value for this trial
            userHasEstimated = true;

            /* If user presses the wheel while confirmation screen is open 
             * the application hides the confirmation screen
             * to allow changing the answer
            */
            if (confirmationCanvas.activeSelf)
            {
                confirmationCanvas.SetActive(false);
            }

            // Closing the invalid input warning when the user clicks the wheel as it defines a value
            if (invalidInputCanvas.activeSelf)
            {
                invalidInputCanvas.SetActive(false);
            }

            // Only allowing value to change if the experiment has already started
            if (!preExperimentCanvas.activeSelf)
            {
                // Allowing the value to be incremented when controller input is positive and calculated value < 100
                if (controllerValue > 0 && userResponse < 100)
                {
                    tempUserResponse += 0.1;
                }
                // Allowing the value to be decreased when controller input is positive and calculated value > 0
                else if (controllerValue < 0 && userResponse > 0)
                {
                    tempUserResponse -= 0.1;
                }
            }
            
            userResponse = (int)System.Math.Round(tempUserResponse, System.MidpointRounding.AwayFromZero);
            outputUserResponse.text = userResponse.ToString() + "%";
            outputConfirmation.text = userResponse.ToString() + "%";
            PlayerPrefs.SetInt("userResponse", userResponse);
        }
        else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // The Pre-experiment canvas is active before the experiment starts
            if (preExperimentCanvas.activeSelf)
            {
                StartExperiment(currentUserId, 0, 0, false);
                preExperimentCanvas.SetActive(false);
            }
            else if (userHasEstimated == true)
            {
                if (currentTrialIndex != -1 && !confirmationCanvas.activeSelf)
                {
                    confirmationCanvas.SetActive(true);
                }
                else if (currentTrialIndex != -1 && confirmationCanvas.activeSelf)
                {
                    NextTrial();
                }
            } else if (userHasEstimated == false)
            {
                invalidInputCanvas.SetActive(true);
            }
        }
    }

    public void StartExperiment(string userID, int trialID, int startWith, bool skipTraining, bool forceAvatar = false)
    {

        _experiment = new Experiment(inputDataPath, userID, trialID, "Subject");
        string outputFilePath = outputDataPath + userID + "-results.csv";
        _experiment.SetOutputFilePath(outputFilePath);

        // This is the results you want 
        _experiment.SetResultsHeader(new string[] { "Subject","Trial","Ratio","Property","Answer","CompletionTime"});

        Debug.Log("Output path : <color=#E91E63>" + outputFilePath + "</color>");

        Debug.Log("<color=#E91E63>Current userId : " + currentUserId + "</color>");

        tempTime = Time.time;

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
        float calculatedSize;
        float rightBarZ;

        _experiment.StartTrial();
        currentTrialIndex = _experiment.GetCurrentTrialIndex();

        Debug.Log("<color=#E91E63>Current trial : " + currentTrialIndex + "</color>");

        leftSpriteRenderer = leftStimulus.GetComponent<SpriteRenderer>();
        rightSpriteRenderer = rightStimulus.GetComponent<SpriteRenderer>();

        // Resetting values and outputs for the next trial
        confirmationCanvas.SetActive(false);
        userHasEstimated = false;
        outputUserResponse.text = "";
        outputConfirmation.text = "";
        userResponse = 50;
        PlayerPrefs.SetInt("userResponse", userResponse);
        tempUserResponse = userResponse;

        if (property == "Length")
        {
            // Scale used by this property to fit the paper and make the right positioning possible
            propertySize = 0.5f;
            calculatedSize = propertySize * ratio;

            leftSpriteRenderer.sprite = lenghtSprite;
            leftStimulus.transform.localScale = new Vector3(propertySize, propertySize, propertySize);
            leftStimulus.transform.localPosition = new Vector3(0.048f, 0.012f, 0.063f);
                        
            rightSpriteRenderer.sprite = lenghtSprite;
            rightStimulus.transform.localScale = new Vector3(propertySize, calculatedSize, propertySize);
            rightStimulus.transform.localPosition = new Vector3(-0.164f, 0.012f, 0.043f);

            horizontalBar.SetActive(false);

        } else if (property == "Area")
        {
            propertySize = 0.2f;
            calculatedSize = propertySize * ratio;

            leftSpriteRenderer.sprite = areaSprite;
            leftStimulus.transform.localScale = new Vector3(propertySize, propertySize, propertySize);
            leftStimulus.transform.localPosition = new Vector3(0.045f, 0.012f, 0.063f);
        
            rightSpriteRenderer.sprite = areaSprite;
            rightStimulus.transform.localScale = new Vector3(calculatedSize, calculatedSize, calculatedSize);
            rightStimulus.transform.localPosition = new Vector3(-0.16f, 0.012f, 0.033f);

            horizontalBar.SetActive(false);

        } else if (property == "BarChart")
        {
            propertySize = 0.4f;
            calculatedSize = propertySize * ratio;

            leftSpriteRenderer.sprite = barSprite;
            leftStimulus.transform.localScale = new Vector3(propertySize, propertySize, propertySize);
            leftStimulus.transform.localPosition = new Vector3(-0.113f, 0.012f, 0.063f);

            rightSpriteRenderer.sprite = barSprite;
            rightStimulus.transform.localScale = new Vector3(propertySize, calculatedSize, propertySize);

            // Calculating the right bar position to align bars to the bottom
            rightBarZ = (System.Math.Abs(propertySize - calculatedSize) / 2) + 0.063f;

            rightStimulus.transform.localPosition = new Vector3(-0.257f, 0.012f, rightBarZ);

            horizontalBar.SetActive(true);
        }

        trialTime = Time.time - tempTime;
        tempTime = Time.time;

        SetResults(
            currentUserId,
            currentTrialIndex,
            ratio,
            property,
            PlayerPrefs.GetInt("userResponse"),
            trialTime
        );

    }

    public void TrialEnded()
    {
        Debug.Log("The trial has ended. You can display a panel with the questions.");
    }


    public void SetResults(string currentUserId, int currentTrialIndex, float ratio, string property, int userResponse, float CompletionTime)
    {
        // The result data correspond to _experiment.SetResultsHeader
        // Subject,Trial,Ratio,Property,Answer,CompletionTime

        _experiment.SetResultData("Subject", currentUserId);
        _experiment.SetResultData("Trial", currentTrialIndex.ToString());
        _experiment.SetResultData("Ratio", ratio.ToString());
        _experiment.SetResultData("Property", property);
        _experiment.SetResultData("Answer", userResponse.ToString());
        _experiment.SetResultData("CompletionTime", CompletionTime.ToString());     
        Debug.Log("Setting the results");
        _experiment.EndTrial();
    }

    void ExperienceFinished()
    {
        Debug.Log("The experience is finished");
        // Calling another scene to avoid further interaction and give user a message
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }


    public void RandomResults()
    {
        //SetResults((int)Random.Range(0, 100), Random.Range(0, 1));
    }

    public void ApplicationStop() { }
}