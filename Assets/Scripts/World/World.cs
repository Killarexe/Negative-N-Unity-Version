using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] private BiomeAttributes biome;
    [SerializeField] private Material material;
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject debugObject;
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private BlockType[] blockTypes;
    [SerializeField] private ItemType[] itemTypes;
    [Header("Variables:")]
    [Range(0, int.MaxValue/2)]
    [SerializeField]private int seed;
    [Range(0, 512)]
    [SerializeField] private float noiseOffset = 6;
    [Range(0, 2048)]
    [SerializeField] private int _textureAtlasSize = 6;
    [Range(0, 32)]
    [SerializeField] private int _chunkWidth = 16;
    [Range(0, 256)]
    [SerializeField] private int _chunkHeight = 128;
    [Range(0, 1024)]
    [SerializeField] private int _worldSize = 6;
    [Range(0, 48)]
    [SerializeField] private int _viewDistance = 12;

    private Chunk[,] chunks;
    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private Queue<VoxelMod> mods = new Queue<VoxelMod>();
    private ChunkCoord playerLastChunk;
    private ChunkCoord playerActiveChunk;

    private static int textureAtlasSize;
    private static int ChunkWidth;
    private static int ChunkHeight;
    private static int WorldSize;
    private static int WorldSizeInVoxels;
    private static int ViewDistance;

    private bool isCreatingChunks;

    public static float normalizeBlockTextureSize
    {
        get { return 1f / (float)textureAtlasSize; }
    }

    private void Start()
    {
        UnityEngine.Random.InitState(seed);
        setValues();
        generateWorld();
        debugObject.SetActive(false);
    }

    private void Update()
    {
        playerActiveChunk = getChunkCoord(player.position);

        if (!playerActiveChunk.Equals(playerLastChunk))
        {
            checkViewDistance();
        }

        if(chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("createChunks");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugObject.SetActive(!debugObject.activeSelf);
        }
    }

    private void setValues()
    {
        textureAtlasSize = _textureAtlasSize;
        ChunkWidth = _chunkWidth;
        ChunkHeight = _chunkHeight;
        WorldSize = _worldSize;
        ViewDistance = _viewDistance;
        WorldSizeInVoxels = WorldSize * ChunkWidth;
        player = GameObject.Find("Player").transform;
        spawnPos = new Vector3(WorldSizeInVoxels / 2, (ChunkHeight + 1)/ 2 - 20, WorldSizeInVoxels / 2);
        player.transform.position = spawnPos;
        playerLastChunk = getChunkCoord(player.position);
        chunks = new Chunk[WorldSize, WorldSize];
    }

    private void generateWorld()
    {
        for(int i = (WorldSize / 2) - ViewDistance; i < (WorldSize / 2) + ViewDistance; i++)
        {
            for (int j = (WorldSize / 2) - ViewDistance; j < (WorldSize / 2) + ViewDistance; j++)
            {
                chunks[i, j] = new Chunk(this, new ChunkCoord(i, j), true);
                activeChunks.Add(new ChunkCoord(i, j));
            }
        }

        while(mods.Count > 0)
        {
            VoxelMod v = mods.Dequeue();
            ChunkCoord c = getChunkCoord(v.pos);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chunk(this, c, true);
                activeChunks.Add(c);
            }

            chunks[c.x, c.z].mods.Enqueue(v);

            if(!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }
        }

        for(int i=0; i < chunksToUpdate.Count; i++)
        {
            chunksToUpdate[0].updateChunk();
            chunksToUpdate.RemoveAt(0);
        }
    }

    private IEnumerator createChunks()
    {
        isCreatingChunks = true;

        while(chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].initChunk();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    private void checkViewDistance()
    {
        ChunkCoord coord = getChunkCoord(player.transform.position);
        playerLastChunk = playerActiveChunk;

        List<ChunkCoord> prevousActiveChunks = new List<ChunkCoord>(activeChunks);

        for(int i= coord.x - ViewDistance; i < coord.x + ViewDistance; i++)
        {
            for (int j = coord.z - ViewDistance; j < coord.z + ViewDistance; j++)
            {
                if(isChunkInWorld(new ChunkCoord(i, j)))
                {
                    if(chunks[i, j] == null)
                    {
                        chunks[i, j] = new Chunk(this, new ChunkCoord(i, j), false);
                        chunksToCreate.Add(new ChunkCoord(i, j));
                    }
                    else if(!chunks[i, j].isActive)
                    {
                        chunks[i, j].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(i, j));
                }

                for(int k = 0; k < prevousActiveChunks.Count; k++)
                {
                    if (prevousActiveChunks[k].Equals(new ChunkCoord(i, j)))
                    {
                        prevousActiveChunks.RemoveAt(k);
                    }
                }
            }
        }

        foreach(ChunkCoord coord1 in prevousActiveChunks)
        {
            chunks[coord1.x, coord1.z].isActive = false;
        }
    }

    private bool isChunkInWorld(ChunkCoord coord)
    {
        if(coord.x > 0 && coord.x < WorldSize - 1 && coord.z > 0 && coord.z < WorldSize - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool isVoxelInWorld(Vector3 pos)
    {
        if(pos.x >= 0 && pos.x < WorldSizeInVoxels && pos.y >= 0 && pos.y < ChunkHeight && pos.z >= 0 && pos.z < WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool checkForVoxel(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > ChunkHeight)
        {
            return false;
        }

        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].getVoxelFromVec3(pos)].getIsSolid;
        }

        return blockTypes[getVoxel(pos)].getIsSolid;
    }

    public bool checkForTransparentVoxel(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > ChunkHeight)
        {
            return false;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].getVoxelFromVec3(pos)].getIsTransparent;
        }

        return blockTypes[getVoxel(pos)].getIsTransparent;
    }

    public bool checkForUnbreakableVoxel(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > ChunkHeight)
        {
            return false;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].getVoxelFromVec3(pos)].getIsUnbreakable;
        }

        return blockTypes[getVoxel(pos)].getIsUnbreakable;
    }

    #region Gets

    public byte getVoxel(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);

        if (!isVoxelInWorld(pos))
        {
            return 0;
        }

        if (yPos == 0)
        {
            return 4;
        }

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.get2DPerlin(new Vector2(pos.x, pos.z), noiseOffset, biome.scale)) + biome.solidGroundHeight;
        byte voxelValue;

        if (yPos == terrainHeight)
        {
            voxelValue = 1;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 10;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 2;
        }

        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.get3DPerlin(pos, lode.noiseOffest, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        if(yPos == terrainHeight)
        {
            if(Noise.get2DPerlin(new Vector2(pos.x, pos.y), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                voxelValue = 16;
                if(Noise.get2DPerlin(new Vector2(pos.x, pos.y), 0, biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    voxelValue = 6;
                    mods.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + 1, pos.z), 5));
                }
            }
        }

        return voxelValue;

    }

    private ChunkCoord getChunkCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk getChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / ChunkWidth);
        return chunks[x, z];
    }

    public BlockType getBlockTypeVoxel(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > ChunkHeight)
        {
            return null;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].getVoxelFromVec3(pos)];
        }

        return blockTypes[getVoxel(pos)];
    }

    public BlockType[] getBlockTypes
    {
        get { return blockTypes; }
    }

    public ItemType[] getItemTypes
    {
        get { return itemTypes; }
    }

    public int getTextureAtalsSize
    {
        get
        {
            return textureAtlasSize;
        }
    }

    public Material getMaterial
    {
        get
        {
            return material;
        }
    }

    public Material getTransparentMaterial
    {
        get
        {
            return transparentMaterial;
        }
    }

    public static int getChunkWidth
    {
        get
        {
            return ChunkWidth;
        }
    }
    public static int getChunkHeight
    {
        get
        {
            return ChunkHeight;
        }
    }

    public static int getWorldSizeInVoxels
    {
        get
        {
            return WorldSizeInVoxels;
        }
    }

    public static int getWorldSizeInChunks
    {
        get
        {
            return WorldSize;
        }
    }

    public Transform getPlayer
    {
        get
        {
            return player;
        }
    }

    public ChunkCoord getActivePlayerChunkCoord
    {
        get
        {
            return playerActiveChunk;
        }
    }

    public Vector3 spawnPoint
    {
        get { return spawnPos; }
        set { spawnPos = value; }
    }

    #endregion
}