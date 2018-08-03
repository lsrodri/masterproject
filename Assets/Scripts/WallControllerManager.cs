using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEzExp;

public class WallControllerManager : MonoBehaviour {

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
    public GameObject stimuliBundle;
    public GameObject stimulusPaper;
    public GameObject cameraEye;
    public GameObject canvases;
    public GameObject sphere;
    public GameObject preNextTrial;
    public GameObject stimuliWall;
    public Sprite lenghtSprite;
    public Sprite areaSprite;
    public Sprite barSprite;
    public Text cameraStimuliAngle;

    //Initializing as 0 for direct scene testing as this variable is set at the index scene
    private string currentUserId = "1";
    private SpriteRenderer leftSpriteRenderer;
    private SpriteRenderer rightSpriteRenderer;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    private bool userHasEstimated = false;
    private float tempTime;
    private float trialTime;
    private float leftStimuliVariation;
    private float size1;
    private float participantAngle;
    private bool invertingBoolean = true;
    private bool visibilityConfirmed = false;
    private Camera cam;
    private float headToWallDistance = 0; 

    RunExperiment _runExperiment;

    void Start()
    {
        
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        PlayerPrefs.SetInt("userResponse", 50);
        userResponse = PlayerPrefs.GetInt("userResponse");
        currentUserId = PlayerPrefs.GetString("participant");
        cam = cameraEye.GetComponent<Camera>();
    }
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        float controllerValue = device.GetAxis().y;

        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            // Only running this code for the first time the user presses an answer
            if (userHasEstimated == false)
            {
                // Getting participant angle on the moment they answer
                Vector2 dir = new Vector2(stimulusPaper.transform.position.x, stimulusPaper.transform.position.z) - new Vector2(cameraEye.transform.position.x, cameraEye.transform.position.z);
                var forward = new Vector2(cameraEye.transform.forward.x, cameraEye.transform.forward.z);
                participantAngle = System.Math.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

                headToWallDistance = cameraEye.transform.position.z - stimuliWall.transform.position.z;

                // Flag that user has entered a value for this trial
                userHasEstimated = true;
            }

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
            outputUserResponse.text = userResponse.ToString();
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
            // Checking if user has estimated a value before showing the confirmation canvas
            else if (userHasEstimated == true)
            {
                if (currentTrialIndex != -1 && !confirmationCanvas.activeSelf)
                {
                    confirmationCanvas.SetActive(true);
                }
                else if (currentTrialIndex != -1 && confirmationCanvas.activeSelf)
                {
                    trialTime = Time.time - tempTime;

                    SetResults(
                        PlayerPrefs.GetInt("userResponse"),
                        trialTime,
                        participantAngle,
                        headToWallDistance
                    );

                    NextTrial();
                }
            } else if (userHasEstimated == false)
            {
                invalidInputCanvas.SetActive(true);
            }
        }
    }

    void FixedUpdate()
    {
        if (!visibilityConfirmed)
        {
            // Checking if stimuli is currently visible for the VR camera
            Vector3 screenPoint = cam.WorldToViewportPoint(stimuliBundle.transform.position);
            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                // Confirming visibility to stop this from running during update
                visibilityConfirmed = true;
                // Updating chronometer starting time from the moment the stimuli becomes visible
                tempTime = Time.time;
            }
        }

    }
        public void StartExperiment(string userID, int trialID, int startWith, bool skipTraining, bool forceAvatar = false)
    {

        _experiment = new Experiment(inputDataPath, userID, trialID, "Participant");
        string outputFilePath = outputDataPath + userID + "-results.csv";
        _experiment.SetOutputFilePath(outputFilePath);

        // This is the results you want 
        _experiment.SetResultsHeader(new string[] { "Answer", "CompletionTime", "ParticipantAngle" });

        Debug.Log("Output path : <color=#E91E63>" + outputFilePath + "</color>");

        Debug.Log("<color=#E91E63>Current userId : " + currentUserId + "</color>");

        tempTime = Time.time;

        stimuliBundle.SetActive(true);

        NextTrial();

    }

    public void NextTrial()
    {
        // Trying to load the next trial variables
        try
        {
            Trial t = _experiment.LoadNextTrial();
        }
        catch (AllTrialsPerformedException e)
        {
            ExperienceFinished();
            return;
        }

        // Reading values from the CSV : 
        float ratio = float.Parse(_experiment.GetParameterData("Ratio"));
        string orientation = _experiment.GetParameterData("Orientation");
        int inputPosition = int.Parse(_experiment.GetParameterData("Position")); 
        float calculatedRightSize;
        float rightBarZ;
        float calculatedLeftSize;
        int invertedBoolInt;
        visibilityConfirmed = false;

        _experiment.StartTrial();
        currentTrialIndex = _experiment.GetCurrentTrialIndex();

        /* This variable holds the time when a new trial started
         * it is reassigned on the FixedUpdate method with the time a participant sees the stimuli
        */
        tempTime = Time.time;

        leftStimuliVariation = ((int)UnityEngine.Random.Range(95, 105) * 0.01f);
        
        Debug.Log(currentTrialIndex);

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

        // Inverting the order from last trial and then converting it to integer to access array
        invertingBoolean = !invertingBoolean;
        invertedBoolInt = invertingBoolean ? 1 : 0;

        switch (inputPosition)
        {
            case 0:
                stimuliBundle.transform.position = new Vector3(3.4f, 1.3f, -5.57617f);

                canvases.transform.localPosition = new Vector3(3.332f, 1.605f, -5.2541f);
                canvases.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                canvases.transform.localScale = new Vector3(0.3369711f, 0.3369711f, 0.4813873f);
                break;
            case 1:
                stimuliBundle.transform.position = new Vector3(0.648f, 1.3f, -5.57617f);

                canvases.transform.localPosition = new Vector3(-2.7052f, 1.3292f, -5f);
                canvases.transform.localRotation = Quaternion.Euler(0, -90f, 0);
                canvases.transform.localScale = new Vector3(0.7077155f, 0.7077155f, 1.011023f);
                break;
            case 2:
                stimuliBundle.transform.position = new Vector3(-1.416f, 1.3f, -5.57617f);

                canvases.transform.localPosition = new Vector3(-2.7052f, 1.3292f, -5f);
                canvases.transform.localRotation = Quaternion.Euler(0, -90f, 0);
                canvases.transform.localScale = new Vector3(0.7077155f, 0.7077155f, 1.011023f);
                break;
            default:
                Console.WriteLine("inputPosition not in switch case list");
                break;
        }

        if (orientation == "vertical")
        {
            stimuliBundle.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        }
        else
        {
            stimuliBundle.transform.localRotation = Quaternion.Euler(180f, -90, -90f);
        }
        
        calculatedLeftSize = 0.4f;
        calculatedRightSize = calculatedLeftSize * ratio;

        leftSpriteRenderer.sprite = barSprite;
        leftStimulus.transform.localScale = new Vector3(calculatedLeftSize, calculatedLeftSize, calculatedLeftSize);
        horizontalBar.SetActive(true);

        leftStimulus.transform.localPosition = new Vector3(-0.094f, 0.012f, 0.243f);
        rightStimulus.transform.localPosition = new Vector3(-0.2663f, 0.012f, 0.243f);


        rightSpriteRenderer.sprite = barSprite;
        rightStimulus.transform.localScale = new Vector3(calculatedLeftSize, calculatedRightSize, calculatedLeftSize);

        // Calculating the right bar position to align bars to the bottom
        rightBarZ = (System.Math.Abs(calculatedLeftSize - calculatedRightSize) / 2) + 0.063f;

    }

    public void TrialEnded()
    {
        Debug.Log("The trial has ended. You can display a panel with the questions.");
    }


    public void SetResults(int response, float CompletionTime, float participantAngle, float headToWallDistance)
    {
        // The result data correspond to _experiment.SetResultsHeader
        _experiment.SetResultData("Answer", response.ToString());
        _experiment.SetResultData("CompletionTime", CompletionTime.ToString());
        _experiment.SetResultData("ParticipantAngle", participantAngle.ToString());
        _experiment.SetResultData("headToWallDistance", headToWallDistance.ToString()); 

        _experiment.EndTrial();
    }

    void ExperienceFinished()
    {
        Debug.Log("The experience is finished");
        // Calling another scene to avoid further interaction and give user a message
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }


    public float RandomUnalignedZPos(float leftStimulusZ, float rightStimulusZ, float calculatedLeftSize, float calculatedRightSize)
    {
        /* Checking if position needs to be randomized in case the bottom of the right stimulus
             * is within .02f of the bottom of the left stimulus
             * loop will run until the right stimulus gets out of that range
             * which will happen by changing the z position of the right stimulus
        */

        float rightStimulusZBottom = rightStimulusZ + calculatedRightSize;
        float leftStimulusZBottom = calculatedLeftSize + leftStimulusZ;
        while (rightStimulusZBottom >= (leftStimulusZBottom - 0.02f)
                && rightStimulusZBottom <= (leftStimulusZBottom + 0.02f))
        {
            rightStimulusZ = UnityEngine.Random.Range(rightStimulusZ, leftStimulusZ);
            rightStimulusZBottom = rightStimulusZ + calculatedRightSize;
        }

        return rightStimulusZ;
    }
}