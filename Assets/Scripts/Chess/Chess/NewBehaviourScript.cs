using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : Chessman
{
    public override bool[,] PossibleMove(bool fake = false)
    {
        bool[,] r = new bool[5, 5];
        // Chessman c,c1,c2,c3,c4,c5,c6,c7;
        List<Tuple<int, int>> temp = BoardManager.Instance.dske[Tuple.Create(CurrentX, CurrentY)];

        foreach (var item in temp)
        {
            Chessman x;
            if (!fake)
            {
                x = BoardManager.Instance.Chessmans[item.Item1, item.Item2];
            }
            else
            {
                x = AiChess.Instance.Chessmans[item.Item1, item.Item2];
            }
            if (x == null)
            {
                r[item.Item1, item.Item2] = true;
            }
        }
        return r;
    }

    public override bool KhongDiDuoc(bool fake = false)
    {
        List<Tuple<int, int>> temp = BoardManager.Instance.dske[Tuple.Create(CurrentX, CurrentY)];

        foreach (var item in temp)
        {
            Chessman x;
            if (!fake)
            {
                x = BoardManager.Instance.Chessmans[item.Item1, item.Item2];
            }
            else
            {
                x = AiChess.Instance.Chessmans[item.Item1, item.Item2];
            }
            
            if (x == null)
            {
                return false;
            }
        }
        return true;
    }

    public override List<Tuple<int, int>> Ganh(bool isWhite,bool fake = false)
    {
        List<Tuple<int, int>> temp = new List<Tuple<int, int>>();
        //Chessman c,c1,c2,c3,c4,c5,c6,c7;
        
        List<Tuple<int, int>> temp1 = BoardManager.Instance.dske[Tuple.Create(CurrentX, CurrentY)];;
 
        int count = temp1.Count;
        if (count == 3)
        {
            if ((CurrentX == 0 || CurrentX == 4) && (CurrentY == 0 || CurrentY == 4))
            {
                return temp;
            }
            else
            {
                Chessman x;
                Chessman x1;
                if (!fake)
                {
                    x = BoardManager.Instance.Chessmans[temp1[0].Item1, temp1[0].Item2];
                    x1 = BoardManager.Instance.Chessmans[temp1[count-1].Item1, temp1[count-1].Item2];
                }
                else
                {
                    x = AiChess.Instance.Chessmans[temp1[0].Item1, temp1[0].Item2];
                     x1 = AiChess.Instance.Chessmans[temp1[count-1].Item1, temp1[count-1].Item2];
                }
                if (x != null && x1 != null)
                {
                    if (x.isWhite != isWhite && x1.isWhite != isWhite)
                    {
                        temp.Add(new Tuple<int, int>(temp1[0].Item1, temp1[0].Item2));
                        temp.Add(new Tuple<int, int>(temp1[count- 1].Item1, temp1[count - 1].Item2));
                    }
                }
            }
        }else if (count == 4)
        {
            for (int i = 0; i < 2; i++)
            {
                Chessman x;
                Chessman x1;
                if (!fake)
                {
                    x = BoardManager.Instance.Chessmans[temp1[i].Item1, temp1[i].Item2];
                    x1 = BoardManager.Instance.Chessmans[temp1[i+2].Item1, temp1[i+2].Item2];
                }
                else
                {
                    x = AiChess.Instance.Chessmans[temp1[i].Item1, temp1[i].Item2];
                    x1 = AiChess.Instance.Chessmans[temp1[i+2].Item1, temp1[i+2].Item2];
                }
                if (x != null && x1 != null)
                {
                    if (x.isWhite != isWhite && x1.isWhite != isWhite)
                    {
                        temp.Add(new Tuple<int, int>(temp1[i].Item1, temp1[i].Item2));
                        temp.Add(new Tuple<int, int>(temp1[i + 2].Item1, temp1[i + 2].Item2));
                    }
                }
            }
        }else if (count == 5)
        {
            Chessman x;
            Chessman x1;
            if (!fake)
            {
                x = BoardManager.Instance.Chessmans[temp1[0].Item1, temp1[0].Item2];
                x1 = BoardManager.Instance.Chessmans[temp1[count-1].Item1, temp1[count-1].Item2];
            }
            else
            {
                x = AiChess.Instance.Chessmans[temp1[0].Item1, temp1[0].Item2];
                x1 = AiChess.Instance.Chessmans[temp1[count-1].Item1, temp1[count-1].Item2];
            }
            if (x != null && x1 != null)
            {
                if (x.isWhite != isWhite && x1.isWhite != isWhite)
                {
                    temp.Add(new Tuple<int, int>(temp1[0].Item1, temp1[0].Item2));
                    temp.Add(new Tuple<int, int>(temp1[count - 1].Item1, temp1[count - 1].Item2));
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                Chessman x;
                Chessman x1;
                if (!fake)
                {
                    x = BoardManager.Instance.Chessmans[temp1[i].Item1, temp1[i].Item2];
                    x1 = BoardManager.Instance.Chessmans[temp1[i+4].Item1, temp1[i+4].Item2];
                }
                else
                {
                    x = AiChess.Instance.Chessmans[temp1[i].Item1, temp1[i].Item2];
                    x1 = AiChess.Instance.Chessmans[temp1[i+4].Item1, temp1[i+4].Item2];
                }
                if (x != null && x1 != null)
                {
                    if (x.isWhite != isWhite && x1.isWhite != isWhite)
                    {
                        temp.Add(new Tuple<int, int>(temp1[i].Item1, temp1[i].Item2));
                        temp.Add(new Tuple<int, int>(temp1[i + 4].Item1, temp1[i + 4].Item2));
                    }
                }
            }
        }
        return temp;
    }
}
