using UnityEngine;
using System.Collections;

public class GroundBrake : MonoBehaviour {

    public Rigidbody Body;
    public SlingerBody BodyCollisionInfo;
    public float BrakeRatio;

    private SteamVR_TrackedObject _trackedObj = null;
    private SteamVR_Controller.Device _device;

    private bool _applyBrake;
    private bool _collidingWithSurface;
    private bool _wasColliding;
    private float _previousSpeed;

    // Use this for initialization
    void Start () {
	
	}
	
    void Update()
    {
        if (_trackedObj == null)
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            if (_trackedObj == null)
                return;
        }

        _device = SteamVR_Controller.Input((int)_trackedObj.index);

        if (_device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            _applyBrake = true;
        }
        
        if (_device.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            _applyBrake = false;
        }
    }

	void FixedUpdate ()
    {
        if (_trackedObj == null)
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            if (_trackedObj == null)
                return;
        }
        
        if (BodyCollisionInfo.IsColliding && !_wasColliding)
        {
            SteamVR_Controller.Input((int)_trackedObj.index).TriggerHapticPulse((ushort)(_previousSpeed * 1000));
        }

        if (_applyBrake && BodyCollisionInfo.IsColliding)
        {
            if (Body.velocity.sqrMagnitude < Mathf.Epsilon)
            {
                Body.velocity = Vector3.zero;
            }
            else
            {
                var brake = BrakeRatio * Body.mass * -Body.velocity;
                SteamVR_Controller.Input((int)_trackedObj.index).TriggerHapticPulse((ushort)(Body.velocity.magnitude * 100));
                Body.AddForce(brake);
            }
        }

        _wasColliding = BodyCollisionInfo.IsColliding;
        _previousSpeed = Body.velocity.magnitude;
    }
}
