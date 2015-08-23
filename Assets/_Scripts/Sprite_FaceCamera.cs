using UnityEngine;
using System.Collections;

public class Sprite_FaceCamera : MonoBehaviour 
{
    Transform cameraT;

    public void Start()
    {
        cameraT = Camera.main.transform;
    }

    public void Update()
    {
        Vector3 toCam = cameraT.position - transform.position;

        Quaternion referentialShift = Quaternion.FromToRotation(transform.forward, new Vector3(toCam.x, 0, toCam.z));
        transform.rotation *= referentialShift;
    }
}
