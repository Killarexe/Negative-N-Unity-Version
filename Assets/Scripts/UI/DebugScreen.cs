using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{

    [SerializeField] private World world;
    [SerializeField] private Text text;
    [SerializeField] private string version;

    private float fps;
    private float timer = 0f;

    private int halfWorldSizeVoxels;
    private int halfWorldSizeChunks;

    private void Start()
    {
        text = gameObject.GetComponent<Text>();
        world = GameObject.Find("World").GetComponent<World>();

        halfWorldSizeChunks = World.getWorldSizeInChunks / 2;
        halfWorldSizeVoxels = World.getWorldSizeInVoxels / 2;
    }

    void Update()
    {
        text.text = 
            "Negative-N v" + version +" Debug Screen:\n" +
            "FPS: " + fps + "\n" +
            "X/Y/Z: " + (Mathf.FloorToInt(world.getPlayer.position.x) - halfWorldSizeVoxels) + "/"
            + (Mathf.FloorToInt(world.getPlayer.position.y) - halfWorldSizeVoxels) + "/" 
            + (Mathf.FloorToInt(world.getPlayer.position.z) - halfWorldSizeVoxels) + "\n" +
            "Chunk Pos: " + (world.getActivePlayerChunkCoord.x - halfWorldSizeChunks) + ", " + (world.getActivePlayerChunkCoord.z - halfWorldSizeChunks) + "\n"
            ;

        if (timer > 1f)
        {
            fps = (int)(1f / Time.unscaledTime);
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
        }

    }
}
