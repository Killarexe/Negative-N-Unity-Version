using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEvents : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private World world;

    private void Start()
    {
        world = GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    public void testOreGeneration()
    {

    }

    public void unbreakable()
    {
        world.getChunkFromVector3(player.getSelectedBlock.position).editVoxel(player.getSelectedBlock.position, 4);
    }

}
