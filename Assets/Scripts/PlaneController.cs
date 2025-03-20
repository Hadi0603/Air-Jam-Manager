using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneController : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject[] movingObjects;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public int[] stopWaypointIndices;
    public bool resetObjectsOnStart = true;
    public int[] flyingWaypoints;
    public float detectionRadius = 2f;
    public float frontDetectionAngle = 45f;
    public int[] startingWaypointIndices;
    [SerializeField] int[] endingWaypointIndex;
    [SerializeField] ButtonWaypointAssigner buttonWaypointAssigner;
    [SerializeField] UIManager uiManager;

    private int[] currentWaypointIndex;
    private bool[] isStopped;
    private Rigidbody[] rigidbodies;
    private bool isGameOver = false;

    void Start()
    {
        currentWaypointIndex = new int[movingObjects.Length];
        isStopped = new bool[movingObjects.Length];
        rigidbodies = new Rigidbody[movingObjects.Length];

        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null && waypoints.Length > 0)
            {
                rigidbodies[i] = movingObjects[i].GetComponent<Rigidbody>();
                if (rigidbodies[i] == null)
                {
                    rigidbodies[i] = movingObjects[i].AddComponent<Rigidbody>();
                }
                rigidbodies[i].useGravity = true;
                rigidbodies[i].isKinematic = false;

                currentWaypointIndex[i] = (startingWaypointIndices != null && startingWaypointIndices.Length > i) ? startingWaypointIndices[i] : 0;

                if (resetObjectsOnStart)
                {
                    movingObjects[i].transform.position = waypoints[currentWaypointIndex[i]].position;
                }
            }
        }
        UpdateButtonStates();
    }

    void FixedUpdate()
    {
        bool allDestroyed = true;

        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null)
            {
                allDestroyed = false;

                if (!IsPlaneInFront(movingObjects[i]))
                {
                    MoveObject(movingObjects[i], i);
                }
            }
        }

        if (allDestroyed && !isGameOver)
        {
            isGameOver = true;
            uiManager.TriggerGameWon();
        }
        UpdateButtonStates();
    }

    void MoveObject(GameObject obj, int index)
    {
        if (currentWaypointIndex[index] < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex[index]];
            Vector3 targetPosition = targetWaypoint.position;

            if (stopWaypointIndices != null && Array.Exists(stopWaypointIndices, stopIndex => stopIndex == currentWaypointIndex[index]) && !isStopped[index])
            {
                isStopped[index] = true;
                return;
            }

            if (!isStopped[index])
            {
                Vector3 direction = (targetPosition - obj.transform.position).normalized;
                rigidbodies[index].velocity = direction * moveSpeed;

                if (Array.Exists(flyingWaypoints, w => w == currentWaypointIndex[index]))
                {
                    rigidbodies[index].useGravity = false;
                }
                else
                {
                    rigidbodies[index].useGravity = true;
                }

                if (direction != Vector3.zero) 
                {
                    // Calculate the target rotation based on direction
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Apply the base rotation of (-90,0,0)
                    targetRotation *= Quaternion.Euler(-90, 0, 0);

                    // Smoothly rotate towards the target rotation
                    obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }


                if (Vector3.Distance(obj.transform.position, targetPosition) < 0.5f)
                {
                    currentWaypointIndex[index]++;
                    for (int i = 0; i < endingWaypointIndex.Length; i++)
                    {
                        if (currentWaypointIndex[index] == endingWaypointIndex[i] + 1)
                        {
                            Destroy(obj);
                            movingObjects[index] = null;
                        }
                    }
                }
            }
        }
    }

    public void ContinueMovement(int stopWaypointIndex, int targetWaypointIndex)
    {
        for (int i = 0; i < movingObjects.Length; i++)
        {
            if (movingObjects[i] != null && isStopped[i] && currentWaypointIndex[i] == stopWaypointIndex)
            {
                currentWaypointIndex[i] = targetWaypointIndex + 1;
                isStopped[i] = false;
            }
        }
        UpdateButtonStates();
    }


    bool IsPlaneInFront(GameObject obj)
    {
        Collider[] colliders = Physics.OverlapSphere(obj.transform.position, detectionRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Plane") && col.gameObject != obj)
            {
                Vector3 toPlane = (col.transform.position - obj.transform.position).normalized;
                float angle = Vector3.Angle(-obj.transform.up, toPlane);
                if (angle < frontDetectionAngle / 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void OnDrawGizmos()
    {
        if (movingObjects != null)
        {
            Gizmos.color = Color.red;
            foreach (GameObject obj in movingObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireSphere(obj.transform.position, detectionRadius);
                    Vector3 frontDirection = obj.transform.forward * detectionRadius;
                    Gizmos.DrawLine(obj.transform.position, obj.transform.position + frontDirection);
                }
            }
        }
    }

   public void UpdateButtonStates()
    {
        if (buttonWaypointAssigner == null) return;

        for (int i = 0; i < buttonWaypointAssigner.buttons.Length; i++)
        {
            bool buttonEnabled = false;
            int stopIndex = buttonWaypointAssigner.stopWaypointIndices[i]; // Get the stop index for this button

            for (int j = 0; j < movingObjects.Length; j++)
            {
                if (movingObjects[j] != null && isStopped[j] && currentWaypointIndex[j] == stopIndex)
                {
                    buttonEnabled = true;
                    break; // If any object is at the stop index, enable the button
                }
            }
            buttonWaypointAssigner.buttons[i].interactable = buttonEnabled;
        }
    }
}
