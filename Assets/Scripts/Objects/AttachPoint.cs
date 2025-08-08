using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    public bool IsAvailable
    {
        get
        {
            return _isAvailable;
        }
        set
        {
            _isAvailable = value;
        }
    }
    
    [SerializeField, ReadOnly] private bool _isAvailable;
    [SerializeField, ReadOnly] private Minion _minion;

    // This allows me to pass AttachPoint as a Vector3, instead of having to manually get the position. It's just a quality of life thing.
    public static implicit operator Vector3(AttachPoint attachPoint)
    {
        return attachPoint != null ? attachPoint.transform.position : Vector3.zero;
    }

    public static implicit operator Transform(AttachPoint attachPoint)
    {
        return attachPoint != null ? attachPoint.transform : null;
    }

    public void RegisterMinion(Minion minion)
    {
        _minion = minion;
        IsAvailable = false;
    }

    public void UnregisterMinion()
    {
        _minion = null;
        IsAvailable = true;
    }
}
