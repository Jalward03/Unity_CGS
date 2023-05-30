using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class SpinningObject : MonoBehaviour
{
    public bool spinOnXAxis, spinOnYAxis, spinOnZAxis;
    public bool counterClockWise;
    public bool randomDirection;
    [Range(100, 500f)] public float rotationSpeed;

    private Vector3 rotationValue;

    private void Awake()
    {
        if(randomDirection)
        {
            int rand = Random.Range(0, 2);

            counterClockWise = rand == 1;
        }
        if(counterClockWise) rotationSpeed *= -1;
        
        
        rotationValue = new Vector3();

        rotationValue.x = spinOnXAxis ? rotationSpeed  : 0;
        rotationValue.y = spinOnYAxis ? rotationSpeed  : 0;
        rotationValue.z = spinOnZAxis ? rotationSpeed  : 0;
    }

    
    void FixedUpdate()
    {
        transform.rotation *= Quaternion.Euler(rotationValue * Time.deltaTime);
    }
}
