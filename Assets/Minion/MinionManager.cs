using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionManager : MonoBehaviour
{
    public GameObject minionPrefab;
    public Transform player;
    public int numberOfMinions = 5;
    public float returnDelay = 5f;
    public AudioClip commandSound;
    public AudioClip recallSound;

    private AudioSource audioSource;

    private List<MinionController> minions = new List<MinionController>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SpawnMinions();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // Keep movement horizontal
            cameraForward.Normalize();

            Vector3 commandPosition = player.position + cameraForward * 10f; // 10 units forward

            foreach (var minion in minions)
            {
                minion.CommandTo(commandPosition, returnDelay);
            }

            if (commandSound != null)
            {
                audioSource.PlayOneShot(commandSound);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Call back minions
        {
            foreach (var minion in minions)
            {
                minion.ReturnImmediately();
            }

            if (recallSound != null)
            {
                audioSource.PlayOneShot(recallSound);
            }
        }
        if (Input.GetMouseButtonDown(2)) // Middle Mouse Button
        {
            foreach (var minion in minions)
            {
                minion.SetStay(true);
            }
        }
    }

    void SpawnMinions()
    {
        for (int i = 0; i < numberOfMinions; i++)
        {
            Vector3 spawnPos = player.position + Random.insideUnitSphere * 2;
            spawnPos.y = player.position.y;
            GameObject minionObj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            var minion = minionObj.GetComponent<MinionController>();
            minion.SetFollowTarget(player);
            minions.Add(minion);
        }
    }
}
