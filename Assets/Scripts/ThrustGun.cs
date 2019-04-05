using UnityEngine;
using System.Collections;
using Valve.VR;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ThrustGun : MonoBehaviour {

    public Transform Thruster;
    public Rigidbody Body;
    public float MaxForce;
    public float VerticalBias;

    private SteamVR_TrackedObject _trackedObj = null;
    private SteamVR_Controller.Device _device;
    private bool _applyForce;
    private float _currentForceRatio;

    void Update()
    {
        if (_trackedObj == null)
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            if (_trackedObj == null)
                return;
        }

        _device = SteamVR_Controller.Input((int)_trackedObj.index);
        var triggerAxis = _device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);
        
        if (triggerAxis.x > Mathf.Epsilon)
        {
            _applyForce = true;
            _currentForceRatio = triggerAxis.x;
        }
        else
        {
            _applyForce = false;
            _currentForceRatio = 0f;
        }

        if (_device.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            Body.MovePosition(Vector3.zero + .01f * Vector3.up);
            Body.velocity = Vector3.zero;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (_applyForce)
        {
            SteamVR_Controller.Input((int)_trackedObj.index).TriggerHapticPulse((ushort)(1000f * _currentForceRatio));
            var force = _currentForceRatio * MaxForce;

            var dot = Vector3.Dot(Thruster.up, Vector3.up);
            var bias = (VerticalBias - 1) * Mathf.Abs(dot) + 1;

            Body.AddForce(force * bias * -Thruster.up);
        }
    }
}
