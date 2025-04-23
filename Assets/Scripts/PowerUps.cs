using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUps : MonoBehaviour
{
    [Header("Traffic PowerUp")]
    [SerializeField] private Button stopTrafficBtn;
    [SerializeField] private GameObject[] traffic;
    [Header("RunWay PowerUp")]
    [SerializeField] private Button stopRunwayBtn;
    [SerializeField] private GameObject[] runways;

    public void stopTraffic()
    {
        for (int i = 0; i < traffic.Length; i++)
        {
            traffic[i].GetComponent<ObstacleMover>().enabled = false;
        }
        stopTrafficBtn.interactable = false;
        Invoke(nameof(RunTraffic),5f);
    }

    void RunTraffic()
    {
        for (int i = 0; i < traffic.Length; i++)
        {
            traffic[i].GetComponent<ObstacleMover>().enabled = true;
        }
    }

    public void stopRunway()
    {
        for (int i = 0; i < runways.Length; i++)
        {
            runways[i].GetComponent<ObstacleMover>().enabled = false;
        }
        stopRunwayBtn.interactable = false;
        Invoke(nameof(RunRunway),5f);
    }

    void RunRunway()
    {
        for (int i = 0; i < runways.Length; i++)
        {
            runways[i].GetComponent<ObstacleMover>().enabled = true;
        }
    }
}
