using System.Collections.Generic;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // Tracks all persistent objects by ID
    private static HashSet<string> existingIDs = new HashSet<string>();

    [Tooltip("Must be unique per persistent object type")]
    public string persistentID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(persistentID))
        {
            Debug.LogError($"{gameObject.name} has no persistentID!");
            Destroy(gameObject);
            return;
        }

        // If this ID already exists, destroy the duplicate
        if (existingIDs.Contains(persistentID))
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, register and persist
        existingIDs.Add(persistentID);
        DontDestroyOnLoad(gameObject);
    }
}