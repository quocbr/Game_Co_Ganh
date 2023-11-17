using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weightMatrix 
{

    int[,] Weight = new int[,]
    {
        {0,  10,  20,  10,  0},
       {10, 40, 30, 40, 10},
        {20, 30, 50, 30, 20},
        {10,  40, 30, 40, 10},
        {0,  10,  20, 10, 0}
    };
    


    public int GetBoardWeight(Vector2 position, string color)
    {
        return Weight[(int)position.x, (int)position.y];
    }



}