using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Home;

public static class TetrisShape
{
    public static readonly int[,] I = new int[4, 4] {
        {4,5,6,7},
        {2,6,10,14},
        {8,9,10,11},
        {1,5,9,13}
    };

    public static readonly int[,] O = new int[4, 4] {
        {5,6,9,10},
        {5,6,9,10},
        {5,6,9,10},
        {5,6,9,10}
    };

    public static readonly int[,] T = new int[4, 4] {
        {1,4,5,6},
        {1,5,6,9},
        {4,5,6,9},
        {1,4,5,9}
    };

    public static readonly int[,] S = new int[4, 4] {
        {1,2,4,5},
        {1,5,6,10},
        {5,6,8,9},
        {2,6,5,9}
    };

    public static readonly int[,] Z = new int[4, 4] {
        {0,1,5,6},
        {2,5,6,9},
        {4,5,9,10},
        {1,5,4,8}
    };

    public static readonly int[,] J = new int[4, 4] {
        {0,4,5,6},
        {1,2,5,9},
        {4,5,6,10},
        {1,5,9,8}
    };

    public static readonly int[,] L = new int[4, 4] {
        {2,4,5,6},
        {1,5,9,10},
        {4,5,6,8},
        {0,1,5,9}
    };

    // public static int[] GetBlockLayout(ArcadeScreenTetris.BlockType type, int rotation = 0)
    // {
    //     // switch(type)
    //     // {
    //     //     ArcadeScreenTetris.BlockType.I:
    //     //         return GetRow(I, rotation);
    //     // }
    // }

    private static int[] GetRow(int[,] block, int row)
    {
        int[] result = new int[4];
        for(int i=0; i<4; i++)
        {
            result[i] = block[row, i];
        }
        return result;
    }
}