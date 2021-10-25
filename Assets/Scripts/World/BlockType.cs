using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BlockType
{
    [Serializable] public class OnBreakEvent : UnityEvent { }
    [Serializable] public class OnPosedEvent : UnityEvent { }
    [Serializable] public class OnAddedEvent : UnityEvent { }
    [Serializable] public class OnWalkEvent : UnityEvent { }
    [Serializable] public class OnCollideEvent : UnityEvent { }
    [Serializable] public class OnRightClickEvent : UnityEvent { }
    [Serializable] public class OnTickEvent : UnityEvent { }

    [Header("Variables:")]
    [SerializeField] private string blockName;
    [SerializeField] private bool isSolid;
    [SerializeField] private bool isUnbreakable;
    [SerializeField] private bool isTransparent;
    [SerializeField] private bool isSingleTexture;
    [Header("Texture:")]
    [SerializeField] private int[] texIDS = new int[6];
    [Header("Components:")]
    [SerializeField] private Sprite icon;
    [SerializeField] private BlockSoundType soundType;
    [Header("Events:")]
    [SerializeField] private OnBreakEvent _onBreak = new OnBreakEvent();
    [SerializeField] private OnPosedEvent _onPosed = new OnPosedEvent();
    [SerializeField] private OnAddedEvent _onAdded = new OnAddedEvent();
    [SerializeField] private OnWalkEvent _onWalk = new OnWalkEvent();
    [SerializeField] private OnCollideEvent _onCollide = new OnCollideEvent();
    [SerializeField] private OnRightClickEvent _onRightClick = new OnRightClickEvent();
    [SerializeField] private OnTickEvent _onTick = new OnTickEvent();

    #region getter
    public int getTextureID(int texID)
    {
        if (!isSingleTexture)
        {
            switch (texID)
            {
                case 0:
                    return texIDS[0];
                    break;
                case 1:
                    return texIDS[1];
                    break;
                case 2:
                    return texIDS[2];
                    break;
                case 3:
                    return texIDS[3];
                    break;
                case 4:
                    return texIDS[4];
                    break;
                case 5:
                    return texIDS[5];
                    break;
                default:
                    return texIDS[0];
                    Debug.LogWarning("Texture not defind at block '" + blockName + "' !");
            }
        }
        else
        {
            return texIDS[0];
        }
    }

    public OnBreakEvent onBreak
    {
        get { return _onBreak; }
        set { _onBreak = value; }
    }

    public OnPosedEvent onPosed
    {
        get { return _onPosed; }
        set { _onPosed = value; }
    }

    public OnAddedEvent onAdded
    {
        get { return _onAdded; }
        set { _onAdded = value; }
    }

    public OnWalkEvent onWalk
    {
        get { return _onWalk; }
        set { _onWalk = value; }
    }

    public OnCollideEvent onCollide
    {
        get { return _onCollide; }
        set { _onCollide = value; }
    }

    public OnRightClickEvent onRightClick
    {
        get { return _onRightClick; }
        set { _onRightClick = value; }
    }

    public OnTickEvent onTick
    {
        get { return _onTick; }
        set { _onTick = value; }
    }

    public string getBlockName
    {
        get { return blockName; }
    }

    public bool getIsSolid
    {
        get { return isSolid; }
    }

    public bool getIsTransparent
    {
        get { return isTransparent; }
    }

    public bool getIsUnbreakable
    {
        get { return isUnbreakable; }
    }

    public AudioClip getPoseSFX
    {
        get { return BlockSound.Instance.getSFXS[(int)soundType].SFXS[0]; }
    }

    public AudioClip getBreakSFX
    {
        get { return BlockSound.Instance.getSFXS[(int)soundType].SFXS[0]; }
    }
    public AudioClip getWalkSFX
    {
        get { return BlockSound.Instance.getSFXS[(int)soundType].SFXS[1]; }
    }

    public Sprite getIcon
    {
        get { return icon; }
    }
#endregion
}

public class VoxelMod
{
    public Vector3 pos;
    public byte id;

    public VoxelMod()
    {
        pos = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 pos, byte id)
    {
        this.pos = pos;
        this.id = id;
    }
}