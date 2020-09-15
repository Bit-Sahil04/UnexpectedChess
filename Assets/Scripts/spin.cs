using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class spin : MonoBehaviour
{
    
    public float speed = 0.1f;
    
    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
