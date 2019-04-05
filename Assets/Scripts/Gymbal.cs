using UnityEngine;
using System.Collections;

public class Gymbal : MonoBehaviour {

    public Transform DirectionalGymbal;
    public Transform SteadyGymbal;
    public Rigidbody Body;

    private float _myEpsilon = .01f;

    void Awake()
    {
        if (SteadyGymbal != null)
        {
            DirectionalGymbal.SetParent(SteadyGymbal);
        }
    }
	
	// Update is called once per frame
	void Update () {
        SteadyGymbal.up = Vector3.up;

	    if (Body.velocity.magnitude > _myEpsilon)
        {
            var direction = Body.velocity.normalized;
            DirectionalGymbal.forward = SteadyGymbal.rotation * direction;
        }
        else
        {
            DirectionalGymbal.forward = Body.transform.forward;
        }
	}
}
