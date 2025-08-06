using UnityEngine;
using UnityEngine.AI;

public class MinionMovement : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1.0f;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (HasDestination())
        {
            _agent.updatePosition = false;
            
            Vector3 worldTarget = _agent.steeringTarget;
            Vector3 toTarget = (worldTarget - transform.position).normalized;

            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.LookRotation(toTarget, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            }
            
            Vector3 forwardMovement = transform.forward * _agent.speed * Time.deltaTime;
            transform.position += forwardMovement;
            transform.position = new Vector3(transform.position.x, _agent.nextPosition.y, transform.position.z);
            
            _agent.nextPosition = transform.position;
        }
        else
        {
            _agent.updatePosition = true;
            _agent.nextPosition = transform.position;
        }
    }

    private bool HasDestination()
    {
        return _agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance;
    }
}
