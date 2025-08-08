using System;
using UnityEngine;

public class CarryCommand : IMinionCommand
{
    public event Action<IMinionCommand> OnComplete;
    public bool IsFinished { get; private set; }

    private Minion _minion; 
        
    private CarryableObject _objectToCarry;
    private AttachPoint _attachPoint;
    private bool _onRoute;

    public CarryCommand(CarryableObject obj)
    {
        _objectToCarry = obj;
    }
    
    public void Start(Minion minion)
    {
        _minion = minion;
        IsFinished = false;
        
        _attachPoint = _objectToCarry.RegisterCarrier(minion);
        if (_attachPoint != null)
        {
            minion.MoveTo(_attachPoint);
            _attachPoint.RegisterMinion(minion);
            _onRoute = true;
        }
    }

    public void Update()
    {
        if (IsFinished)
            return;

        const float kMinDistance = 1.0f;
        if (_onRoute && _minion.Agent.hasPath && _minion.Agent.remainingDistance <= kMinDistance)
        { 
            _minion.SetCarryTarget(_objectToCarry, _attachPoint);
            _objectToCarry.NotifyCarrierArrived(_minion, _attachPoint);
                
            IsFinished = true;
            OnComplete?.Invoke(this);
        }
    }
}
