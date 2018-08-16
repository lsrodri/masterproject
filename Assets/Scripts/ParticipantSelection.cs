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
            Scene m_Scene = SceneManager.GetActiveScene();
            Debug.Log(m_Scene.name);
            if (m_Scene.name == "index")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            } else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(4);
            }
        }
        else
        {
            Debug.Log("Empty participant field");
        }
    }

}