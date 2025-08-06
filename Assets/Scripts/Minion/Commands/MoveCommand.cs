using System;
using UnityEngine;

public class MoveCommand : IMinionCommand
{
    public event Action<IMinionCommand> OnComplete;
    public bool IsFinished { get; private set; }

    private Vector3 _destination;
    private Minion _minion;

    public MoveCommand(Vector3 destination)
    {
        _destination = destination;
    }
    
    public void Start(Minion minion)
    {
        _minion = minion;
        _minion.MoveTo(_destination);
    }

    public void Update()
    {
        if (_minion.Agent.hasPath && _minion.HasReachedDestination() && !IsFinished)
        {
            IsFinished = true;
            OnComplete?.Invoke(this);
        }
    }
}
