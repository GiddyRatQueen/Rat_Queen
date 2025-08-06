using System;
using UnityEngine;

public interface IMinionCommand
{
    event Action<IMinionCommand> OnComplete; 
    bool IsFinished { get; }
    
    void Start(Minion minion);
    void Update();
}
