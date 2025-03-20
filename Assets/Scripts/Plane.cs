using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    [SerializeField] UIManager uIManager;

    private void Start()
    {
        uIManager=GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Plane") || other.gameObject.CompareTag("Obstacle") ||
            other.gameObject.CompareTag("Ground"))
        {
            uIManager.GameOver();
            if (other.gameObject.CompareTag("Plane"))
            {
                Debug.Log("Collided with Plane");
            }
            else if (other.gameObject.CompareTag("Obstacle"))
            {
                Debug.Log("Collided with Obstacle");
            }
            else
            {
                Debug.Log("Collided with Ground");
            }
        }
    }
}
