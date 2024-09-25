using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : MonoBehaviour
{
    public Transform wheel;

    public float rotationSpeed = 10f;

    public Transform[] ferris;
    private Quaternion[] initialRotation;
    void Start()
    {
        initialRotation = new Quaternion[ferris.Length];
        for(int i = 0; i < ferris.Length; i++)
        {
            initialRotation[i] = ferris[i].rotation;
        }
    }

    void Update()
    {
        wheel.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        for (int i = 0; i < ferris.Length; i++)
        {
             ferris[i].rotation = initialRotation[i];
        }
    }
}
