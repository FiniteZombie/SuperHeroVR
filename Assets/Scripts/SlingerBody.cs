using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlingerBody : MonoBehaviour {

    public bool IsColliding
    {
        get
        {
            return _colliders.Count > 0;
        }
    }

    private HashSet<Collider> _colliders = new HashSet<Collider>();

    void OnCollisionEnter(Collision collisionInfo)
    {
        _colliders.Add(collisionInfo.collider);
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        _colliders.Remove(collisionInfo.collider);
    }
}
