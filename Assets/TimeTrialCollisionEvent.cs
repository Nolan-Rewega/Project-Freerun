using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialCollisionEvent : MonoBehaviour
{
    public enum CollisionType
    {
        Start,
        End,
        Checkpoint
    }

    public CollisionType collisionType;
}
