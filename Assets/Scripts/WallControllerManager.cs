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

    RunExperiment _runExperiment;

    void Start()
    {
        
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        PlayerPrefs.SetInt("userResponse", 50);
        userResponse = PlayerPrefs.GetInt("userResponse");
        currentUserId = PlayerPrefs.GetString("participant");
        //Debug.Log(cameraEye.transform.position);
        
    }
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        float controllerValue = device.GetAxis().y;

        Vector2 dir = new Vector2(stimulusPaper.transform.position.x, stimulusPaper.transform.position.z) - new Vector2(cameraEye.transform.position.x, cameraEye.transform.position.z);
        cameraStimuliAngle.text = "Camera-Stimuli Angle: " + (System.Math.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));

        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            // Only running this code for the first time the user presses an answer
            if (userHasEstimated == false)
            {
                // Getting participant angle on the moment they answer
                /*Vector2 dir = new Vector2(stimulusPaper.transform.position.x, stimulusPaper.transform.position.z) - new Vector2(cameraEye.transform.position.x, cameraEye.transform.position.z);
                var forward = new Vector2(cameraEye.transform.forward.x, cameraEye.transform.forward.z);
                participantAngle = System.Math.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);*/

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
                        participantAngle
                    );

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

        // REading values from the CSV : 
        float ratio = float.Parse(_experiment.GetParameterData("Ratio"));
        string property = _experiment.GetParameterData("Property");
        string orientation = _experiment.GetParameterData("Orientation");
        int inputPosition = int.Parse(_experiment.GetParameterData("Position")); 
        float propertySize;
        float calculatedRightSize;
        float rightBarZ;
        float widthAdjusted;
        float calculatedLeftSize;
        //float[] stimulusZPos = new float[2];
        int invertedBoolInt;

        _experiment.StartTrial();
        currentTrialIndex = _experiment.GetCurrentTrialIndex();

        //trialTime = Time.time - tempTime;
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
            case 150:
                stimuliBundle.transform.position = new Vector3(2.8255f, 1.3f, -5.5615f);
                break;
            case 155:
                stimuliBundle.transform.position = new Vector3(2.6f, 1.3f, -5.5615f);
                break;
            case 160:
                stimuliBundle.transform.position = new Vector3(2.27f, 1.3f, -5.5615f);
                break;
            case 165:
                stimuliBundle.transform.position = new Vector3(1.737f, 1.3f, -5.5615f);
                break;
            case 170:
                stimuliBundle.transform.position = new Vector3(0.68f, 1.3f, -5.5615f);
                break;
            case 175:
                stimuliBundle.transform.position = new Vector3(-2.5f, 1.3f, -5.5615f);
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

        if (property == "Area")
        {

            //stimuliBundle.transform.rotation = Quaternion.Euler(90f, 0, 0);

            calculatedLeftSize = 0.2f;
            calculatedRightSize = calculatedLeftSize * ratio;

            float[] stimulusZPos = { 0.063f, -0.193f };
            float leftStimulusZ = stimulusZPos[invertedBoolInt];

            // Inverting the boolean above to position right stimulus differently from its left counterpart
            int invertedInt = System.Math.Abs(1 - invertedBoolInt);
            float rightStimulusZ = stimulusZPos[invertedInt];

            leftSpriteRenderer.sprite = areaSprite;
            leftStimulus.transform.localScale = new Vector3(calculatedLeftSize, calculatedLeftSize, calculatedLeftSize);
            leftStimulus.transform.localPosition = new Vector3(0.045f, 0.012f, leftStimulusZ);

            rightStimulusZ = RandomUnalignedZPos(leftStimulusZ, rightStimulusZ, calculatedLeftSize, calculatedRightSize);

            rightSpriteRenderer.sprite = areaSprite;
            rightStimulus.transform.localScale = new Vector3(calculatedRightSize, calculatedRightSize, calculatedRightSize);
            rightStimulus.transform.localPosition = new Vector3(-0.16f, 0.012f, rightStimulusZ);

            horizontalBar.SetActive(false);

        } else if (property == "BarChart")
        {
            calculatedLeftSize = 0.4f;
            calculatedRightSize = calculatedLeftSize * ratio;

            leftSpriteRenderer.sprite = barSprite;
            leftStimulus.transform.localScale = new Vector3(calculatedLeftSize, calculatedLeftSize, calculatedLeftSize);
            horizontalBar.SetActive(true);
            
            leftStimulus.transform.localPosition = new Vector3(-0.0987f, 0.012f, 0.2756f);
            rightStimulus.transform.localPosition = new Vector3(-0.271f, 0.012f, 0.2756f);


            rightSpriteRenderer.sprite = barSprite;
            rightStimulus.transform.localScale = new Vector3(calculatedLeftSize, calculatedRightSize, calculatedLeftSize);

            // Calculating the right bar position to align bars to the bottom
            rightBarZ = (System.Math.Abs(calculatedLeftSize - calculatedRightSize) / 2) + 0.063f;
            
        }     

    }

    public void TrialEnded()
    {
        Debug.Log("The trial has ended. You can display a panel with the questions.");
    }


    public void SetResults(int response, float CompletionTime, float participantAngle)
    {
        // The result data correspond to _experiment.SetResultsHeader
        // Participant,Trial,Ratio,Property,Answer,CompletionTime

        /*_experiment.SetResultData("Participant", currentUserId);
        _experiment.SetResultData("Trial", currentTrialIndex.ToString());
        _experiment.SetResultData("Ratio", ratio.ToString());
        _experiment.SetResultData("Property", property);*/
        _experiment.SetResultData("Answer", response.ToString());
        _experiment.SetResultData("CompletionTime", CompletionTime.ToString());
        _experiment.SetResultData("ParticipantAngle", participantAngle.ToString());

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

    public float RandomUnalignedZPos(float leftStimulusZ, float rightStimulusZ, float calculatedLeftSize, float calculatedRightSize)
    {
        /* Checking if position needs to be randomized in case the bottom of the right stimulus
             * is within .01f of the bottom of the left stimulus
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
            Debug.Log("z position randomized");
        }

        return rightStimulusZ;
    }
}