using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelResources", menuName = "ScriptableObjects/LevelResources", order = 1)]
public class LevelResources : ScriptableObject
{
    public List<string> jsonResourcePaths;
}