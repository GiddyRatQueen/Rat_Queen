using System.Collections.Generic;
using UnityEngine;

public class CarryableObject : MonoBehaviour
 {
    [Header("Carry Settings")]
    public int requiredMinions = 3;
    public Transform carryDestination;
    public float moveSmoothness = 5f;

    private List<Transform> activeSockets = new List<Transform>();
    private List<MinionControllerOld> registeredMinions = new List<MinionControllerOld>();

    private bool isBeingCarried = false;
    private bool isClaimed = false;

    void LateUpdate()
    {
        if (!isBeingCarried || activeSockets.Count == 0)
            return;

        // Move to average of sockets
        Vector3 average = Vector3.zero;
        foreach (Transform socket in activeSockets)
            average += socket.position;

        average /= activeSockets.Count;

        transform.position = Vector3.Lerp(transform.position, average, Time.deltaTime * moveSmoothness);

        // Check if arrived
        float dist = Vector3.Distance(transform.position, carryDestination.position);
        if (dist < 1f)
        {
            DropObject();
        }
    }

    public void RegisterMinion(MinionControllerOld minion)
    {
        if (isClaimed || isBeingCarried || registeredMinions.Contains(minion)) return;

        registeredMinions.Add(minion);
        minion.SetBusy(true);

        if (registeredMinions.Count >= requiredMinions)
        {
            isClaimed = true;
            AssignMinions(registeredMinions);
        }
    }

    private void AssignMinions(List<MinionControllerOld> minions)
    {
        activeSockets.Clear();

        for (int i = 0; i < requiredMinions; i++)
        {
            Transform socket = minions[i].GetCarrySocket();

            GameObject anchor = new GameObject($"CarryAnchor_{i}");
            anchor.transform.position = transform.position;
            anchor.transform.rotation = transform.rotation;
            anchor.transform.SetParent(socket, worldPositionStays: true);

            activeSockets.Add(anchor.transform);
        }

        isBeingCarried = true;
        Debug.Log("Carry started with minions: " + registeredMinions.Count);
    }

    private void DropObject()
    {
        isBeingCarried = false;

        foreach (Transform t in activeSockets)
        {
            if (t != null)
                Destroy(t.gameObject);
        }

        activeSockets.Clear();

        foreach (var minion in registeredMinions)
        {
            minion.SetBusy(false);
        }

        registeredMinions.Clear();
        isClaimed = false;

        Debug.Log("Object delivered.");
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}