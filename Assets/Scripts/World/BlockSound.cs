using System;
using UnityEngine;

public class BlockSound : MonoBehaviour
{
    
    public static BlockSound Instance { get; private set; }

    [SerializeField] private BlockSFX[] SFXS;

    private void Start()
    {
        Instance = this;
    }

    public BlockSFX[] getSFXS
    {
        get { return SFXS; }
    }
}

[Serializable]
public class BlockSFX
{
    public string name;
    public AudioClip[] SFXS;
}

public enum BlockSoundType
{
    GRASS = 0,
    STONE = 1,
    GLASS = 2,
    DIRT = 3,
    METAL = 4,
    NETHERITE = 5,
    WOOD = 6,
    LEAVES = 7,
    WOOL = 8
}