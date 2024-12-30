using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntranceController : MonoBehaviour
{
    [SerializeField]
    private GameObject openCave;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExplosionTrigger"))
        {
            GameObject instantiatedCave =
                Instantiate(openCave, gameObject.transform.parent);
            instantiatedCave.transform.position = gameObject.transform.position;
            instantiatedCave.transform.rotation = gameObject.transform.rotation;
            instantiatedCave.transform.localScale = gameObject.transform.localScale;
            Destroy(gameObject);
        }
    }
}
