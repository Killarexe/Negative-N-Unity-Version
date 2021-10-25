using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemType
{
    [Serializable] public class OnLeftClickEvent : UnityEvent { }
    [Serializable] public class OnRightClick : UnityEvent { }
    [Serializable] public class OnDropEvent : UnityEvent { }
    [Serializable] public class OnInventoryEvent : UnityEvent { }
    [Serializable] public class OnInventoryTickEvent : UnityEvent { }

    [Header("Variables:")]
    [SerializeField] private string name;
    [Header("Components:")]
    [SerializeField] private Sprite icon;

    [Header("Events:")]
    [SerializeField] private OnLeftClickEvent _onLeftClick = new OnLeftClickEvent();
    [SerializeField] private OnRightClick _onRightClick = new OnRightClick();
    [SerializeField] private OnDropEvent _onDrop = new OnDropEvent();
    [SerializeField] private OnInventoryEvent _onInventory = new OnInventoryEvent();
    [SerializeField] private OnInventoryTickEvent _onInventoryTickEvent = new OnInventoryTickEvent();

    public OnLeftClickEvent onLeftClick
    {
        get { return _onLeftClick; }
        set { _onLeftClick = value; }
    }

    public OnRightClick onRightClick
    {
        get { return _onRightClick; }
        set { _onRightClick = value; }
    }

    public OnDropEvent onDrop
    {
        get { return _onDrop; }
        set { _onDrop = value; }
    }

    public OnInventoryEvent onInventory
    {
        get { return _onInventory; }
        set { _onInventory = value; }
    }

    public OnInventoryTickEvent onInventoryTickEvent
    {
        get { return _onInventoryTickEvent; }
        set { _onInventoryTickEvent = value; }
    }

    public Sprite getIcon
    {
        get { return icon; }
    }
}
