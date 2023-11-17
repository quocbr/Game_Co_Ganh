using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chessman : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }
    public bool isWhite;    //Know which team this piece is part of
    public bool isMoved = false;
    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }
    
    public Chessman Clone()
    {
        return (Chessman) this.MemberwiseClone();
    }

    public virtual bool[,] PossibleMove(bool fake = false)  // virtual keyword means, this function will be override by the child class to let the child class add more customized functions
    {
        return new bool[5,5];
    }

    public virtual bool KhongDiDuoc(bool fake = false)
    {
        return false;
    }
    
    public virtual List<Tuple<int, int>> Ganh(bool isWhite,bool fake = false)  // virtual keyword means, this function will be override by the child class to let the child class add more customized functions
    {
        return new List<Tuple<int, int>>();
    }
    private void Start()
    {
        if(isWhite == false)
        {
            transform.Rotate(0, 180, 0);
        }
    }
}