using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("StartTrial"))
        {
            other.gameObject.GetComponentInParent<TimeTrialManager>().StartTrial();
        }
        else if (other.gameObject.CompareTag("EndTrial"))
        {
            other.gameObject.GetComponentInParent<TimeTrialManager>().EndTrial();
        }
        else if (other.gameObject.CompareTag("TrialCheckpoints"))
        {
            other.gameObject.GetComponentInParent<TimeTrialManager>().ActivateCheckpoint(other.gameObject);
        }
    }
}
