using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntranceController : MonoBehaviour
{
    public GameObject freeEntrance;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExplosionTrigger"))
        {
            /*GameObject instantiatedEntrance = Instantiate(freeEntrance, transform.position, transform.rotation);
            Quaternion t = instantiatedEntrance.transform.rotation;
            instantiatedEntrance.transform.rotation = Quaternion.Euler(t.x - 90, t.y, t.z);
            instantiatedEntrance.transform.localScale = transform.localScale;*/
            
            Destroy(gameObject);
        }
    }
}
