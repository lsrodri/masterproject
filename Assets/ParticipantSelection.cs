using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VR;

public class ParticipantSelection : MonoBehaviour
{

    public InputField participantInput;
    private string participantNumber = "1";

    public void loadExperiment()
    {
        if (participantInput.text != "")
        {
            participantNumber = participantInput.text;
            PlayerPrefs.SetString("participant", participantNumber);
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("Empty participant field");
        }
    }

}