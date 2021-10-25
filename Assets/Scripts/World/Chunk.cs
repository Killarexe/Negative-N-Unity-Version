using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;
    private GameObject chunkObject;
    private MeshRenderer renderer;
    private MeshFilter filter;
    private MeshCollider collider;
    private World world;
    private Material[] materials = new Material[2];
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();
    private List<int> transparentTriangles = new List<int>();
    public Queue<VoxelMod> mods = new Queue<VoxelMod>();

    private static int ChunkWidth = World.getChunkWidth;
    private static int ChunkHeight = World.getChunkHeight;

    public byte[,,] voxelMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];


    private int vertexIndex = 0;
    private bool useMeshCollider = false;
    private bool _isActive;
    private bool isVoxelMapPopulated = false;

    public Chunk(World world, ChunkCoord coord, bool genOnLoad)
    {
        this.world = world;
        this.coord = coord;
        isActive = true;

        if (genOnLoad)
        {
            initChunk();
        }
    }

    public void initChunk()
    {
        chunkObject = new GameObject();
        filter = chunkObject.AddComponent<MeshFilter>();

        renderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.getMaterial;
        materials[1] = world.getTransparentMaterial;

        renderer.materials = materials;
;
        if (useMeshCollider)
        {
            collider = chunkObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.mesh;
        }

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(this.coord.x * ChunkWidth, 0f, this.coord.z * ChunkWidth);
        chunkObject.name = "Chunk (" + this.coord.x + ", " + this.coord.z + ")";

        populateVoxelMap();
        updateChunk();
    }

    public void updateChunk()
    {
        while (mods.Count > 0)
        {
            VoxelMod v = mods.Dequeue();
            Vector3 pos = v.pos -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }

        clearMeshData();

        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {

                    if (world.getBlockTypes[voxelMap[x, y, z]].getIsSolid)
                        UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        createMesh();
    }

    void clearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();

    }

    void updateSurroundingVoxels(int x, int y, int z)
    {

        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {

            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!isVoxelInChunks((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {

                world.getChunkFromVector3(currentVoxel + position).updateChunk();

            }

        }

    }

    public void editVoxel(Vector3 pos, byte newID)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newID;

        updateSurroundingVoxels(xCheck, yCheck, zCheck);

        updateChunk();

    }

    void UpdateMeshData(Vector3 pos)
    {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.getBlockTypes[blockID].getIsTransparent;

        for (int p = 0; p < 6; p++)
        {

            if (checkVoxel(pos + VoxelData.faceChecks[p]))
            {

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                addTextures(world.getBlockTypes[blockID].getTextureID(p));

                if (!isTransparent)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }
                vertexIndex += 4;
            }

        }

    }

    private void createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        filter.mesh = mesh;
    }

    private void populateVoxelMap()
    {
        for (int i = 0; i < ChunkHeight; i++)
        {
            for (int j = 0; j < ChunkWidth; j++)
            {
                for (int k = 0; k < ChunkWidth; k++)
                {    
                    voxelMap[j, i, k] = world.getVoxel(new Vector3(j, i, k) + position);
                    world.getBlockTypes[voxelMap[j, i, k]].onAdded.Invoke();
                }
            }
        }
        isVoxelMapPopulated = true;
    }

    private bool checkVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if(!isVoxelInChunks(x, y, z))
        {
            return world.checkForTransparentVoxel(pos + position);
        }

        return world.getBlockTypes[voxelMap[x, y, z]].getIsTransparent;
    }

    public byte getVoxelFromVec3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    private bool isVoxelInChunks(int x, int y, int z)
    {
        if (x < 0 || x > ChunkWidth - 1 || y < 0 || y > ChunkHeight - 1 || z < 0 || z > ChunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void addTextures(int texID)
    {
        float y = texID / world.getTextureAtalsSize;
        float x = texID - (y * world.getTextureAtalsSize);

        x *= World.normalizeBlockTextureSize;
        y *= World.normalizeBlockTextureSize;

        y = 1f - y - World.normalizeBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + World.normalizeBlockTextureSize));
        uvs.Add(new Vector2(x + World.normalizeBlockTextureSize, y));
        uvs.Add(new Vector2(x + World.normalizeBlockTextureSize, y + World.normalizeBlockTextureSize));
    }

    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }

    }

    public bool isPopulated
    {
        get { return isVoxelMapPopulated; }
    }
}

public class ChunkCoord
{
    public int x, z;

    public ChunkCoord()
    {
        this.x = 0;
        this.z = 0;
    }

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);
        
        x = xCheck/World.getChunkWidth;
        z = zCheck/World.getChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if(other == null)
        {
            return false;
        }
        else if(other.x == x && other.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}