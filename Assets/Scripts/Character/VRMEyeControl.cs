using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMEyeControl : MonoBehaviour {
    Quaternion originRotation;

    void Awake() {
       originRotation = transform.rotation;
       transform.rotation = Quaternion.identity;
    }

    IEnumerator FixEye() {
        yield return new WaitForEndOfFrame();
        transform.rotation = originRotation;
    }

    void Start() {
        StartCoroutine(FixEye());
    }
}
