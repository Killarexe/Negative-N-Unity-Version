using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    [SerializeField] private World world;
    [SerializeField] private Player player;
    [SerializeField] private RectTransform hightlight;
    [SerializeField] private ItemSlot[] itemSlots;

    private int slotIndex = 0;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();

        foreach (ItemSlot slot in itemSlots)
        {
            if (slot.getIsBlockItem)
            {
                slot.getIcon.sprite = world.getBlockTypes[slot.getItemID].getIcon;
                slot.getIcon.enabled = true;
            }
            else if (!slot.getIsBlockItem)
            {
                slot.getIcon.sprite = world.getItemTypes[slot.getItemID].getIcon;
                slot.getIcon.enabled = true;
            }
        }
        player.getSelectedBlockType = itemSlots[slotIndex].getItemID;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {

            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex > itemSlots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = itemSlots.Length - 1;

            hightlight.position = itemSlots[slotIndex].getIcon.transform.position;
            player.getSelectedBlockType = itemSlots[slotIndex].getItemID;
            player.getCanPoseBlock = itemSlots[slotIndex].getIsBlockItem;
        }
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] private byte itemID;
    [SerializeField] private Image icon;
    [SerializeField] private bool isBlockItem;

    public bool getIsBlockItem
    {
        get { return isBlockItem; }
    }

    public Image getIcon
    {
        get
        {
            return icon;
        }
        set
        {
            icon = value;
        }
    }

    public byte getItemID
    {
        get
        {
            return itemID;
        }
    }
}