using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneController : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject[] movingObjects;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float speedMultiplier = 0.1f;
    public float maxSpeed = 10f;
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
    private float[] currentSpeed;
    private bool[] isStopped;
    private Rigidbody[] rigidbodies;

    private bool isGameOver = false;

    //public int[] boardingWaypoints;
    //public GameObject[] passengers;
    //private Dictionary<GameObject, List<GameObject>> loadedPassengers = new Dictionary<GameObject, List<GameObject>>();
    //private int maxPassengers = 2;


    void Start()
    {
        currentWaypointIndex = new int[movingObjects.Length];
        isStopped = new bool[movingObjects.Length];
        rigidbodies = new Rigidbody[movingObjects.Length];
        currentSpeed = new float[movingObjects.Length];
        /*foreach (var obj in movingObjects)
        {
            loadedPassengers[obj] = new List<GameObject>();
        }*/

        for (int i = 0; i < movingObjects.Length; i++)
        {
            currentSpeed[i] = moveSpeed;
        }

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

                currentWaypointIndex[i] = (startingWaypointIndices != null && startingWaypointIndices.Length > i)
                    ? startingWaypointIndices[i]
                    : 0;

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
        {
            bool allDestroyed = true;

            for (int i = 0; i < movingObjects.Length; i++)
            {
                if (movingObjects[i] != null)
                {
                    allDestroyed = false;

                    if (!IsPlaneInFront(movingObjects[i]) && !uiManager.isPaused)
                    {
                        MoveObject(movingObjects[i], i);
                    }

                    else
                    {
                        rigidbodies[i].velocity = Vector3.zero;
                        currentSpeed[i] = 0f;
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
    }

    public void MoveObject(GameObject obj, int index)
        {
            if (currentWaypointIndex[index] < waypoints.Length)
            {
                Transform targetWaypoint = waypoints[currentWaypointIndex[index]];
                Vector3 targetPosition = targetWaypoint.position;

                if (stopWaypointIndices != null &&
                    Array.Exists(stopWaypointIndices, stopIndex => stopIndex == currentWaypointIndex[index]) &&
                    !isStopped[index])
                {
                    isStopped[index] = true;
                    return;
                }

                if (!isStopped[index])
                {
                    currentSpeed[index] += speedMultiplier * Time.fixedDeltaTime;
                    currentSpeed[index] = Mathf.Clamp(currentSpeed[index], moveSpeed, maxSpeed);

                    Vector3 direction = (targetPosition - obj.transform.position).normalized;
                    rigidbodies[index].velocity = direction * currentSpeed[index];

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
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        targetRotation *= Quaternion.Euler(-90, 0, 0);
                        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation,
                            rotationSpeed * Time.fixedDeltaTime);
                    }

                    if (Vector3.Distance(obj.transform.position, targetPosition) < 0.5f)
                    {
                        /*if (Array.Exists(boardingWaypoints, w => w == currentWaypointIndex[index]))
                        {
                            if (loadedPassengers[obj].Count < maxPassengers)
                            {
                                LoadPassengers(obj);
                            }

                            StartCoroutine(WaitAtBoardingPoint(obj, index)); // Wait before moving
                            return;
                        }*/

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

                if (isStopped[index])
                {
                    rigidbodies[index].velocity = Vector3.zero;
                    currentSpeed[index] = moveSpeed;
                }
                else
                {
                    currentSpeed[index] += speedMultiplier * Time.fixedDeltaTime;
                    currentSpeed[index] = Mathf.Clamp(currentSpeed[index], moveSpeed, maxSpeed);
                }

            }
        }

        /*void LoadPassengers(GameObject plane)
        {
            List<GameObject> remainingPassengers = new List<GameObject>(passengers);

            foreach (var passenger in passengers)
            {
                if (!loadedPassengers[plane].Contains(passenger) && remainingPassengers.Contains(passenger))
                {
                    passenger.transform.SetParent(plane.transform);
                    passenger.transform.localPosition = Vector3.zero;
                    loadedPassengers[plane].Add(passenger);
                    remainingPassengers.Remove(passenger); // Remove from available passengers list
                    passenger.GetComponent<MeshRenderer>().enabled = false;
                    passenger.GetComponent<BoxCollider>().enabled = false;

                    if (loadedPassengers[plane].Count >= maxPassengers) break;
                }
            }

            passengers = remainingPassengers.ToArray(); // Update available passengers
        }

        private IEnumerator WaitAtBoardingPoint(GameObject obj, int index)
        {
            isStopped[index] = true; // Stop movement
            yield return new WaitForSeconds(1f); // Wait for 2 seconds
            isStopped[index] = false; // Resume movement
            currentWaypointIndex[index]++;
        }*/

        public void ContinueMovement(int stopWaypointIndex, int targetWaypointIndex)
        {
            for (int i = 0; i < movingObjects.Length; i++)
            {
                if (movingObjects[i] != null && isStopped[i] && currentWaypointIndex[i] == stopWaypointIndex)
                {
                    currentWaypointIndex[i] = targetWaypointIndex + 1;
                    isStopped[i] = false;
                    currentSpeed[i] = moveSpeed;
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
                for (int j = 0; j < movingObjects.Length; j++)
                {
                    if (movingObjects[j] != null && isStopped[j] &&
                        currentWaypointIndex[j] == buttonWaypointAssigner.stopWaypointIndices[i])
                    {
                        buttonEnabled = true;
                        break;
                    }
                }

                buttonWaypointAssigner.canvases[i].enabled = buttonEnabled;
            }
        }
    }

