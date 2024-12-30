using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCave : MonoBehaviour
{
   [SerializeField]
   private Transform outsideCave;
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         other.transform.position = outsideCave.position;
      }
   }
}
