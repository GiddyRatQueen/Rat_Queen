using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarryableObject : MonoBehaviour
{
    public int NeededCarries
    {
        get
        {
            return _attachPoints.Length;
        }
    }

    public event Action<CarryableObject> OnCarried;
    
    [SerializeField] private AttachPoint[] _attachPoints;
    private List<Minion> _carriers = new List<Minion>();
    private List<Minion> _readyCarriers = new List<Minion>(); // List of carriers that are in place, ready to carry the object

    private MinionController _controller;
    
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = false;
    }

    private void Update()
    {
        if (_agent.enabled && _controller != null)
        {
            Vector3 position = _controller.transform.position - transform.forward * 5.0f;
            _agent.SetDestination(position);
        }
    }

    /// <summary>
    /// Registers the minion as a carry of this object and returns it's carry point
    /// </summary>
    public AttachPoint RegisterCarrier(Minion minion)
    {
        if (_carriers.Contains(minion))
        {
            return GetAttachPoint(_carriers.IndexOf(minion));
        }
        
        if (_carriers.Count >= _attachPoints.Length)
            return null;
        
        _carriers.Add(minion);
        return GetAttachPoint(_carriers.Count - 1);
    }

    public void NotifyCarrierArrived(Minion minion, AttachPoint attachPoint)
    {
        _readyCarriers.Add(minion);
        if (_readyCarriers.Count == _attachPoints.Length)
        {
            _agent.enabled = true;
            OnCarried?.Invoke(this);
        }
    }

    public void RegisterOwner(MinionController controller)
    {
        _controller = controller;
    }

    public AttachPoint GetAttachPoint(int index)
    {
        return _attachPoints[index];
    }

    public bool IsCarried()
    {
        foreach (AttachPoint attachPoint in _attachPoints)
        {
            if (attachPoint.IsAvailable)
            {
                return false;
            }
        }
        
        return _carriers.Count == _attachPoints.Length;
    }
    
    /*
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
   */
}