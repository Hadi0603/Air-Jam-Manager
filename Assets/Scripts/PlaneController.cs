using System;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject[] movingObjects;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public int stopWaypointIndex = -1;
    public bool resetObjectsOnStart = true;

    private int[] currentWaypointIndex;
    private bool[] isStopped;

    void Start()
    {
        currentWaypointIndex = new int[movingObjects.Length];
        isStopped = new bool[movingObjects.Length];

        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null && waypoints.Length > 0)
            {
                if (resetObjectsOnStart)
                {
                    movingObjects[i].transform.position = waypoints[0].position;
                }
                else
                {
                    MoveObject(movingObjects[i], i);
                }
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null && waypoints.Length > 0)
            {
                MoveObject(movingObjects[i], i);
            }
        }
    }

    void MoveObject(GameObject obj, int index)
    {
        if (currentWaypointIndex[index] < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex[index]];
            Vector3 targetPosition = targetWaypoint.position;

            if (stopWaypointIndex != -1 && currentWaypointIndex[index] == stopWaypointIndex && !isStopped[index])
            {
                isStopped[index] = true;
                return;
            }

            if (!isStopped[index])
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                Vector3 direction = targetPosition - obj.transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                if (Vector3.Distance(obj.transform.position, targetPosition) < 0.1f)
                {
                    currentWaypointIndex[index]++;

                    if (currentWaypointIndex[index] >= waypoints.Length)
                    {
                        Destroy(obj);
                        movingObjects[index] = null;
                    }
                }
            }
        }
    }
    

    public void ContinueMovement()
    {
        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null && isStopped[i] == true)
            {
                Debug.Log("hi");
                currentWaypointIndex[i] = stopWaypointIndex + 1;
                isStopped[i] = false;

            }
        }
    }
}