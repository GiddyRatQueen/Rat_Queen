using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public enum FormationType
{
    Follow
}

public class MinionController : MonoBehaviour
{
    // List of controlled minions
    [SerializeField, ReadOnly] private List<Minion> _minions = new List<Minion>();
    [SerializeField] private Transform _commandPointIndicator;

    [SerializeField] private float _followDistance;
    private List<IMinionCommand> _issuedCommands = new List<IMinionCommand>();

    [SerializeField] private float _objectDetectionRadius = 1.0f;
    private List<CarryableObject> _carriedObjects = new List<CarryableObject>();
    
    [Header("Formation Properties")] 
    [SerializeField] private FormationType _formationType = FormationType.Follow;
    [SerializeField, Tooltip("Distance between minions")] private float _formationSpacing = 2.0f;

    [Header("Audio")] 
    [SerializeField] private AudioClip _whistleAudio;
    [SerializeField] private AudioClip _recallAudio;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Have a look into the new Input System
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 commandPoint = GetCommandPoint();
            _commandPointIndicator.position = commandPoint;
            
            // Check for any nearby objects to carry, telling minions to pick it up if there are any.
            if (TryGetCarryableObject(commandPoint, _objectDetectionRadius, out CarryableObject obj))
            {
                CommandCarry(obj);
                Debug.Log("Attempting to carry object");
            }
            else
            {
                MoveTo(commandPoint);
            }
            
            _audioSource.PlayOneShot(_whistleAudio);
        }
        
        if (_issuedCommands.Count == 0)
        {
           //ReturnMinions();
        }
    }
    
    #region Events

    private void OnObjectCarried(CarryableObject obj)
    {
        obj.OnCarried -= OnObjectCarried;
        obj.RegisterOwner(this);
    }
    
    #endregion

    #region Public Methods

    public void AssignMinion(Minion minion)
    {
        if (!_minions.Contains(minion))
        {
            _minions.Add(minion);
            minion.SetOwner(this);
        }
    }

    public void MoveTo(Vector3 position)
    {
        for (int i = 0; i < _minions.Count; i++)
        {
            Minion minion = _minions[i];
            Vector3 offset = CalculateOffset(i, _minions.Count);
            
            IMinionCommand command = new MoveCommand(position + offset);
            minion.IssueCommand(command);
            minion.OnCommandComplete += OnCommandComplete;
            
            _issuedCommands.Add(command);
        }
    }

    public void CommandCarry(CarryableObject obj)
    {
        // Bazinga
        int needed = obj.NeededCarries;
        List<Minion> carriers = _minions.OrderBy(minion => Vector3.Distance(minion.transform.position, obj.transform.position)).Take(needed).ToList();

        foreach (Minion minion in carriers)
        {
            minion.ClearCommands();
            minion.IssueCommand(new CarryCommand(obj));

            obj.OnCarried += OnObjectCarried;
        }
    }

    #endregion
    
    #region Private Methods

    private void OnCommandComplete(Minion minion, IMinionCommand command)
    {
        _issuedCommands.Remove(command);
        
        minion.Agent.ResetPath();
        minion.OnCommandComplete -= OnCommandComplete;
    }

    private void ReturnMinions()
    {
        Vector3 followPoint = transform.position - (transform.forward *  _followDistance);
        
        for (int i = 0; i < _minions.Count; i++)
        {
            Minion minion = _minions[i];
            Vector3 offset = CalculateOffset(i, _minions.Count);
            
            minion.MoveTo(followPoint + offset);
        }
    }
    
    /// <summary> Returns a point to command minions to based on the camera </summary>
    private Vector3 GetCommandPoint()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; //Keep movement horizontal
        cameraForward.Normalize();

        const float kDistance = 10.0f;
        return transform.position + cameraForward * kDistance;
    }

    private Vector3 CalculateOffset(int index, int totalMinions)
    {
        switch (_formationType)
        {
            case FormationType.Follow:
            {
                return CalculateFollowOffset(index, totalMinions);
            }
        }
        
        return Vector3.zero;
    }

    private Vector3 CalculateFollowOffset(int index, int totalMinions)
    {
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalMinions));
        int rows = Mathf.CeilToInt((float)totalMinions / cols);
        
        int row = index / cols;
        int col = index % cols;

        Vector3 right = transform.right;
        Vector3 back = -transform.forward;
        
        float rowOffset = (row - (rows - 1) * 0.5f) * _formationSpacing;
        float colOffset = (col - (cols - 1) * 0.5f) * _formationSpacing;
        
        return back * rowOffset + right * colOffset;
    }

    private Minion GetRandomAvailableMinion()
    {
        int index = Random.Range(0, _minions.Count);
        if (_minions[index] != null)
        {
            return _minions[index];
        }
        
        Debug.LogError($"Unable to find available minion at index {index}", this);
        return null;
    }

    private bool DoAnyMinionsHaveCommands()
    {
        foreach (Minion minion in _minions)
        {
            if (minion.CurrentCommand != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to find a carryable object within a specific radius around a point.
    /// </summary>
    private bool TryGetCarryableObject(Vector3 point, float radius, out CarryableObject carryableObject)
    {
        Collider[] hits = Physics.OverlapSphere(point, radius, LayerMask.GetMask("Carryable"));
        foreach (Collider collider in hits)
        {
            if (collider.TryGetComponent(out CarryableObject obj))
            {
                carryableObject = obj;
                return true;
            }
        }

        carryableObject = null;
        return false;
    }
    
    #endregion
}
