using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class EnterCave : MonoBehaviour
{
     public Transform insideCavePosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = insideCavePosition.position;
        }
    }
}
