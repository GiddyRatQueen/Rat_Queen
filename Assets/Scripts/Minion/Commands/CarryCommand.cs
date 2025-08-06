using System;
using UnityEngine;

public class CarryCommand : IMinionCommand
{
    public event Action<IMinionCommand> OnComplete;
    public bool IsFinished { get; }
    
    public void Start(Minion minion)
    {
    }

    public void Update()
    {
    }
}
