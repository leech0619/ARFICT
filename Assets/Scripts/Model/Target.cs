using System;
using UnityEngine;

/// <summary>
/// Data model for navigation targets
/// </summary>
[Serializable]
public class Target
{
    public string Name; // Display name for the target
    public GameObject PositionObject; // GameObject marking the target location
}