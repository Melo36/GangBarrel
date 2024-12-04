using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveExplosionController : MonoBehaviour
{
    public GameObject[] rockArray;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExplosionTrigger"))
        {
            for(int i = 0; i < rockArray.Length; i++)
            {
                rockArray[i].SetActive(true);
            }
        }
    }
}
