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

    public event Action<Minion, IMinionCommand> OnCommandComplete;

    [SerializeField, ReadOnly] private MinionController _owner;
    [SerializeField, ReadOnly] private bool _isCommanded;

    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _minDistanceToTarget = 0.5f;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // If the minion has any queued commands
        if (CurrentCommand == null)
        {
            LookAtPoint(Owner.transform.position);
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

    public void ClearCommand()
    {
        if (CurrentCommand != null)
        {
            CurrentCommand.OnComplete -= HandleCommandComplete;
            CurrentCommand = null;
        }
    }
    
    public void MoveTo(Vector3 position)
    {
        IsCommanded = true;
        
        Agent.ResetPath();
        Agent.SetDestination(position);
        
        Debug.Log($"Given Move To command at {position}");
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
    
    #endregion
    
    #region Private Methods

    private void HandleCommandComplete(IMinionCommand command)
    {
        command.OnComplete -= HandleCommandComplete;
        OnCommandComplete?.Invoke(this, command);
        CurrentCommand = null;
    }
    
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
    
    #endregion
}
