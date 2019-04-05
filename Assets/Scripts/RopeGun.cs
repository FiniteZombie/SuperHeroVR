using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class RopeGun : MonoBehaviour {
    public Rigidbody Body;
    public float thickness = 0.02f;
    public float SwingForce;
    public float MaxDistance = 300f;

    private SteamVR_LaserPointer _laser;
    private SteamVR_TrackedObject _trackedObj = null;
    private SteamVR_Controller.Device _device;
    private GameObject _targetIndicator;
    private GameObject _rope;
    private bool _targetAvailable;
    private ConfigurableJoint _joint;
    private Vector3 _attachPoint;
    private bool _swinging;
    private float _swingDistance;
    Material _targetMaterial;
    Material _ropeMaterial;

    // Use this for initialization
    void Awake () {
        _laser = GetComponent<SteamVR_LaserPointer>();
        _laser.PointerIn += OnPointerIn;
        _laser.PointerOut += OnPointerOut;
    }

    void Start()
    {
        _laser.pointer.SetActive(false);
        _targetIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(_targetIndicator.GetComponent<Collider>());

        _targetMaterial = new Material(Shader.Find("Unlit/Color"));
        _targetMaterial.SetColor("_Color", _laser.color);
        _targetIndicator.GetComponent<MeshRenderer>().material = _targetMaterial;

        _targetIndicator.SetActive(false);

        _rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _rope.transform.parent = transform;
        _rope.transform.up = transform.forward;
        _rope.transform.localScale = new Vector3(thickness, 100f, thickness);
        _rope.transform.localPosition = new Vector3(0f, 50f, 0f);
        Destroy(_rope.GetComponent<Collider>());

        _ropeMaterial = new Material(Shader.Find("Unlit/Color"));
        _ropeMaterial.SetColor("_Color", Color.black);
        _rope.GetComponent<MeshRenderer>().material = _ropeMaterial;

        _rope.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

        if (_trackedObj == null)
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            if (_trackedObj == null)
                return;
        }

        _device = SteamVR_Controller.Input((int)_trackedObj.index);

        _laser.pointer.SetActive(false);
        _targetIndicator.SetActive(false);
        _rope.SetActive(false);
        if (!_swinging && _device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            _laser.pointer.SetActive(true);
            var distance = (_laser.HitPoint - transform.position).magnitude;

            if (_laser.IsHitting && distance < MaxDistance)
            {
                _targetIndicator.SetActive(true);
                _targetIndicator.transform.position = _laser.HitPoint;
                _targetMaterial.SetColor("_Color", _laser.color);
            }
        }

        if (_device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            var distance = (_laser.HitPoint - transform.position).magnitude;
            if (_laser.IsHitting && distance < MaxDistance)
            {
                _attachPoint = _laser.HitPoint;
                _swinging = true;
            }
        }

        if (_device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            _swinging = false;
            Destroy(_joint);
            _joint = null;
        }

        if (_swinging)
        {
            var distance = (_attachPoint - transform.position).magnitude;
            _rope.SetActive(true);
            _rope.transform.up = (_attachPoint - transform.position).normalized;
            _rope.transform.position = Vector3.Lerp(transform.position, _attachPoint, .5f);
            _rope.transform.localScale = new Vector3(thickness, distance / 2f, thickness);

            _targetIndicator.SetActive(true);
            _targetIndicator.transform.position = _attachPoint;
            _targetMaterial.SetColor("_Color", Color.black);
        }
    }

    void FixedUpdate()
    {
        if (_swinging)
        {
            if (_joint == null)
            {
                _joint = Body.gameObject.AddComponent<ConfigurableJoint>();
                _joint.autoConfigureConnectedAnchor = false;
                _joint.anchor = Body.transform.InverseTransformPoint(transform.position);
                _joint.connectedAnchor = _attachPoint;
                _joint.axis = Vector3.right;
                _joint.secondaryAxis = Vector3.up;
                _joint.xMotion = ConfigurableJointMotion.Limited;
                _joint.yMotion = ConfigurableJointMotion.Limited;
                _joint.zMotion = ConfigurableJointMotion.Limited;

                _swingDistance = (_attachPoint - transform.position).magnitude;
            }

            var distance = (_attachPoint - transform.position).magnitude;
            _swingDistance = Mathf.Min(distance, _swingDistance);

            var limit = new SoftJointLimit();
            limit.limit = _swingDistance;
            _joint.linearLimit = limit;
            _joint.anchor = Body.transform.InverseTransformPoint(transform.position);

            var attachToHand = transform.position - _attachPoint;
            var handProj = Vector3.Project(attachToHand, Vector3.up);
            var handProjWorld = _attachPoint + handProj;

            var cross = Vector3.Cross(-attachToHand, handProjWorld - transform.position);
            var swingRotationalDirection = Vector3.Dot(Body.velocity, cross) > 0f
                ? cross.normalized
                : -cross.normalized;

            var swingForceDirection = Vector3.Lerp(swingRotationalDirection, Body.velocity.normalized, .5f);

            Body.AddForce(SwingForce * swingForceDirection);
        }
    }

    void OnPointerIn(object sender, PointerEventArgs e)
    {
        if (e.target != Body.transform)
        {
            _targetAvailable = true;
        }
    }

    void OnPointerOut(object sender, PointerEventArgs e)
    {
        _targetAvailable = false;
    }
}
