using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VR;

public class ParticipantSelection : MonoBehaviour {

    public InputField participantInput;
    private int participantNumber = 0;

	// Use this for initialization
	void Start () {
       //VRSettings.enabled = false;
    }

    public void loadExperiment()
    {
        if (int.TryParse(participantInput.text, out participantNumber))
        {
            PlayerPrefs.SetInt("participant", participantNumber);
            //Application.LoadLevel(1);
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        } else {
            Debug.Log("Entered participant must be an integer.");
        }
    }

}