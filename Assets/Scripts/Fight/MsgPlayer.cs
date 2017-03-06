using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MsgPlayer
{
    public MsgPlayer() { }

    private ulong _id;
    public ulong id
    {
        get { return _id; }
        set { _id = value; }
    }
    private string _name;
    public string name
    {
        get { return _name; }
        set { _name = value; }
    }
    private string _account = "";
    public string account
    {
        get { return _account; }
        set { _account = value; }
    }
    private uint _local = default(uint);
    public uint local
    {
        get { return _local; }
        set { _local = value; }
    }
    private uint _career = default(uint);
    public uint career
    {
        get { return _career; }
        set { _career = value; }
    }
    private uint _camp = default(uint);
    public uint camp
    {
        get { return _camp; }
        set { _camp = value; }
    }
    private uint _EntityType = default(uint);
    public uint EntityType
    {
        get { return _EntityType; }
        set { _EntityType = value; }
    }
    private uint _LifeNum = default(uint);
    public uint LifeNum
    {
        get { return _LifeNum; }
        set { _LifeNum = value; }
    }
    private readonly global::System.Collections.Generic.List<uint> _Objects = new global::System.Collections.Generic.List<uint>();
    public global::System.Collections.Generic.List<uint> Objects
    {
        get { return _Objects; }
    }

    private int _X = default(int);
    public int X
    {
        get { return _X; }
        set { _X = value; }
    }
    private int _Z = default(int);
    public int Z
    {
        get { return _Z; }
        set { _Z = value; }
    }
    private float _Realposx = default(float);
    public float Realposx
    {
        get { return _Realposx; }
        set { _Realposx = value; }
    }
    private float _Realposz = default(float);
    public float Realposz
    {
        get { return _Realposz; }
        set { _Realposz = value; }
    }
    private int _Gold = default(int);
    public int Gold
    {
        get { return _Gold; }
        set { _Gold = value; }
    }
    private int _Level = default(int);
    public int Level
    {
        get { return _Level; }
        set { _Level = value; }
    }
    private int _Movespeed = default(int);
    public int Movespeed
    {
        get { return _Movespeed; }
        set { _Movespeed = value; }
    }
    private float _Radius = default(float);
    public float Radius
    {
        get { return _Radius; }
        set { _Radius = value; }
    }
    private uint _MaxLifeNum = default(uint);
    public uint MaxLifeNum
    {
        get { return _MaxLifeNum; }
        set { _MaxLifeNum = value; }
    }
    private uint _View = default(uint);
    public uint View
    {
        get { return _View; }
        set { _View = value; }
    }
}
