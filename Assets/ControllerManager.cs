using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour {

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    static int userResponse = 50;
    public Text outputUserResponse;
    double tempUserResponse = userResponse;

    void Start()
    {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        PlayerPrefs.SetInt("userResponse", 50);
        userResponse = PlayerPrefs.GetInt("userResponse");
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
        }
    }
}