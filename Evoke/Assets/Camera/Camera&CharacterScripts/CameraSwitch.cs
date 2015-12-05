using UnityEngine;
using System.Collections;

public class CameraSwitch : MonoBehaviour {

    StandardCamera standard;
    TopDownCamera topDown;
    RTSCamera rts;

    void Start()
    {
        standard = GetComponent<StandardCamera>();
        topDown = GetComponent<TopDownCamera>();
        rts = GetComponent<RTSCamera>();

        SwitchToStandard();
    }

    public void SwitchToStandard()
    {
        standard.enabled = true;
        topDown.enabled = false;
        rts.enabled = false;
    }

    public void SwitchToTopDown()
    {
        standard.enabled = false;
        topDown.enabled = true;
        rts.enabled = false;
    }

    public void SwitchToRTS()
    {
        standard.enabled = false;
        topDown.enabled = false;
        rts.enabled = true;
    }
}
