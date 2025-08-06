using System.Collections.Generic;
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
            
            CommandTo(commandPoint);
        }
    }

    #region Public Methods

    public void AssignMinion(Minion minion)
    {
        if (!_minions.Contains(minion))
        {
            _minions.Add(minion);
            minion.SetOwner(this);
        }
    }

    public void CommandTo(Vector3 position)
    {
        for (int i = 0; i < _minions.Count; i++)
        {
            Minion minion = _minions[i];
            Vector3 offset = CalculateOffset(i, _minions.Count);
            
            IMinionCommand command = new MoveCommand(position + offset);
            minion.IssueCommand(command);
            
            _audioSource.PlayOneShot(_whistleAudio);
        }
    }

    #endregion
    
    #region Private Methods

    private void OnCommandComplete(Minion minion, IMinionCommand command)
    {
        minion.Agent.ResetPath();
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
    
    #endregion
}
