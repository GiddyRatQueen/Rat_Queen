using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MinionControllerOld : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform followTarget;
    private Vector3 commandTarget;
    private bool isCommanded;
    private Coroutine returnCoroutine;
    private Vector3 stayPosition;
    private bool isStaying = false;
    private bool hasStayPosition = false;

    [SerializeField] private Transform carrySocket;
    
    [Header("Carry Settings")]
    public float carryDetectionRadius = 3f;
    public LayerMask carryableLayer;
    private bool isBusy = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Awake()
    {
        if (carrySocket == null)
            carrySocket = transform.Find("CarrySocket");

        if (!carrySocket)
            Debug.LogError("CarrySocket not assigned or found!");
    }


    public Transform GetCarrySocket()
    {
        return carrySocket;
    }
void Update()
    {
        if (!agent.enabled)
        {
            return;
        }
        else if (isStaying && hasStayPosition)
        {
            float distance = Vector3.Distance(transform.position, stayPosition);

            // Walk to stayPosition if not there yet
            if (distance > 0.5f)
            {
                agent.SetDestination(stayPosition);
            }
            else
            {
                agent.ResetPath(); // Stop moving
                LookAtPlayer();    // Watch the player
            }
        }
        else if (isCommanded)
        {
            agent.SetDestination(commandTarget);
        }
        else if (!isStaying && followTarget != null)
        {
            float followDistance = 2f;
            Vector3 targetPos = followTarget.position - followTarget.forward * followDistance;
            agent.SetDestination(targetPos);
        }
        else if (isStaying)
        {
            agent.SetDestination(transform.position); // Stay in place
        }
        if (!isBusy)
            TryCarryNearby();

    }

    void TryCarryNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, carryDetectionRadius, carryableLayer);

        foreach (var hit in hits)
        {
            CarryableObject carryable = hit.GetComponent<CarryableObject>();
            if (carryable != null && !carryable.IsBeingCarried())
            {
                carryable.RegisterMinion(this);
                break;
            }
        }
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
    }
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
        isCommanded = false;
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }

    public void CommandTo(Vector3 position, float returnAfterSeconds)
    {
        commandTarget = position;
        isCommanded = true;
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(ReturnAfterDelay(returnAfterSeconds));
    }

    IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetFollowTarget(followTarget);
    }

    public void ReturnImmediately()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        isStaying = false;
        SetFollowTarget(followTarget);
    }
    public void SetStay(bool stay)
    {
        isStaying = stay;
        isCommanded = false;

        if (stay)
        {
            stayPosition = transform.position; // Remember where they are
            hasStayPosition = true;
            agent.SetDestination(stayPosition); // Walk to it (if not there yet)
        }
        else
        {
            hasStayPosition = false;
        }
    }
 
    void LookAtPlayer()
    {
        if (followTarget == null) return;

        Vector3 lookDir = followTarget.position - transform.position;
        lookDir.y = 0; // Prevent tilting
        if (lookDir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
   
}
