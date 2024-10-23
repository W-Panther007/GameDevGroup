using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int NodeIndex;
    public float Speed;
    public int ID;

    public void Init() 
    {
        // Start the enemy at the first node position
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;
    }
}
