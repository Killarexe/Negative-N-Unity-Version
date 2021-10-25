using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Biomes Attributes", menuName = "Negative-N/Biomes Attributes")]
public class BiomeAttributes : ScriptableObject
{

    public string biomeName;

    public int solidGroundHeight;
    public int terrainHeight;
    public float scale;

    [Header("Trees")]
    [Range(0f, 10f)]
    public float treeZoneScale = 1.3f;
    [Range(0f, 1f)]
    public float treeZoneThreshold = 0.6f;
    [Range(0f, 50f)]
    public float treePlacementScale = 15f;
    [Range(0f, 1f)]
    public float treePlacementThreshold = 0.8f;

    [Range(5, 20)]
    public int maxTreeHeight = 12;
    [Range(5, 20)]
    public int minTreeHeight = 5;
    public int stemBlockId;
    public int leavesBlockId;

    public Lode[] lodes;
}

[Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffest;
}
