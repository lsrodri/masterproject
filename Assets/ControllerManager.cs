using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour {

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    public int userResponse = 50;
    public Text outputUserResponse;

    void Start()
    {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        float controllerValue = device.GetAxis().y;
        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad) && controllerValue != 0)
        {
            if (controllerValue > 0 && userResponse < 100)
            {
                userResponse++;
            } else if (controllerValue < 0 && userResponse > 0)
            {
                userResponse--;
            }
            outputUserResponse.text = userResponse.ToString() + "%";
        }
    }
}