using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Minion : MonoBehaviour
{
    public MinionController Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }

    public NavMeshAgent Agent
    {
        get;
        private set;
    }

    public IMinionCommand CurrentCommand
    {
        get;
        private set;
    }

    public bool IsCommanded
    {
        get { return _isCommanded; }
        set { _isCommanded = value; }
    }

    public CarryableObject CurrentCarryTarget 
    {
        get;
        private set;
    }

    public event Action<Minion, IMinionCommand> OnCommandComplete;

    private Rigidbody _rigidbody;
    
    [SerializeField, ReadOnly] private MinionController _owner;
    [SerializeField, ReadOnly] private bool _isCommanded;

    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _minDistanceToTarget = 0.5f;

    private AttachPoint _attachPoint;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // If the minion has any queued commands
        if (CurrentCommand == null)
        {
            //LookAtPoint(Owner.transform.position);
        }
        else
        {
            CurrentCommand.Update();
        }
    }
    
    #region Public Methods

    public void IssueCommand(IMinionCommand command)
    {
        // Prevents the action being called on the previous command 
        if (CurrentCommand != null)
        {
            CurrentCommand.OnComplete -= HandleCommandComplete;
        }
        
        command.Start(this);
        CurrentCommand = command;
        CurrentCommand.OnComplete += HandleCommandComplete;
    }

    public void ClearCommands()
    {
        if (CurrentCommand != null)
        {
            CurrentCommand.OnComplete -= HandleCommandComplete;
            CurrentCommand = null;
        }
    }
    
    public void MoveTo(Vector3 destination)
    {
        IsCommanded = true;
        Agent.SetDestination(destination);
    }

    public void ReturnToOwner()
    {
        if (Owner == null)
        {
            Debug.LogError("No Owner assigned", this);
            return;
        }
    }

    public void SetOwner(MinionController owner)
    {
        Owner = owner;
    }

    public bool IsOwned()
    {
        return Owner != null;
    }

    public bool HasReachedDestination()
    {
        return Agent.remainingDistance <= _minDistanceToTarget;
    }
    
    public void SetCarryTarget(CarryableObject obj, AttachPoint attachPoint)
    {
        if (attachPoint == null)
            return;
        
        CurrentCarryTarget = obj;
        _attachPoint = attachPoint;
        
        Agent.ResetPath();
        Agent.enabled = false;
        _rigidbody.isKinematic = true;
        transform.SetParent(attachPoint);
    }

    public void ReleaseCarry()
    {
        if (CurrentCarryTarget != null)
        {
            transform.SetParent(null, true);
            Agent.enabled = true;

            CurrentCarryTarget = null;
            _attachPoint = null;
        }
    }
    
    #endregion
    
    #region Private Methods
    
    private void LookAtPoint(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0.0f; //Prevent tilting;

        if (direction.sqrMagnitude != 0.0f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }
    }
    
    private void HandleCommandComplete(IMinionCommand command)
    {
        command.OnComplete -= HandleCommandComplete;
        CurrentCommand = null;
        
        OnCommandComplete?.Invoke(this, command);
    }
    
    #endregion
}
