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
        }
    }
}
