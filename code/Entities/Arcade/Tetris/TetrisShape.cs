namespace Home;

public class TetrisShape
{
    public int[] Blocks {get; set;}

    public int[] GetGrid()
    {
        var grid = new int[16];
        for(int i=0; i<16; i++)
        {
            var x = i % 4;
            var y = i / 4;
            if(Blocks.Contains(i))
            {
                grid[i] = 1;
            }
            else
            {
                grid[i] = 0;
            }
        }
        return grid;
    }
}

public static class TetrisShapes
{
    public static readonly TetrisShape[] I = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {4,5,6,7}},
        new TetrisShape {Blocks = new int[4] {2,6,10,14}},
        new TetrisShape {Blocks = new int[4] {8,9,10,11}},
        new TetrisShape {Blocks = new int[4] {1,5,9,13}}
    };

    public static readonly TetrisShape[] O = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {5,6,9,10}},
        new TetrisShape {Blocks = new int[4] {5,6,9,10}},
        new TetrisShape {Blocks = new int[4] {5,6,9,10}},
        new TetrisShape {Blocks = new int[4] {5,6,9,10}}
    };

    public static readonly TetrisShape[] T = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {1,4,5,6}},
        new TetrisShape {Blocks = new int[4] {1,5,6,9}},
        new TetrisShape {Blocks = new int[4] {4,5,6,9}},
        new TetrisShape {Blocks = new int[4] {1,4,5,9}}
    };

    public static readonly TetrisShape[] S = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {1,2,4,5}},
        new TetrisShape {Blocks = new int[4] {0,4,5,9}},
        new TetrisShape {Blocks = new int[4] {1,2,4,5}},
        new TetrisShape {Blocks = new int[4] {0,4,5,9}}
    };

    public static readonly TetrisShape[] Z = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {0,1,5,6}},
        new TetrisShape {Blocks = new int[4] {1,5,4,8}},
        new TetrisShape {Blocks = new int[4] {0,1,5,6}},
        new TetrisShape {Blocks = new int[4] {1,5,4,8}}
    };

    public static readonly TetrisShape[] J = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {0,4,5,6}},
        new TetrisShape {Blocks = new int[4] {1,2,5,9}},
        new TetrisShape {Blocks = new int[4] {4,5,6,10}},
        new TetrisShape {Blocks = new int[4] {1,5,9,8}}
    };

    public static readonly TetrisShape[] L = new TetrisShape[4] {
        new TetrisShape {Blocks = new int[4] {2,4,5,6}},
        new TetrisShape {Blocks = new int[4] {1,5,9,10}},
        new TetrisShape {Blocks = new int[4] {4,5,6,8}},
        new TetrisShape {Blocks = new int[4] {0,1,5,9}}
    };

    public static TetrisShape GetShape(ArcadeScreenTetris.BlockType blockType, int rotation)
    {
        if(rotation < 0) rotation += 4;
        switch(blockType)
        {
            case ArcadeScreenTetris.BlockType.I:
                return I[rotation];
            case ArcadeScreenTetris.BlockType.O:
                return O[rotation];
            case ArcadeScreenTetris.BlockType.T:
                return T[rotation];
            case ArcadeScreenTetris.BlockType.S:
                return S[rotation];
            case ArcadeScreenTetris.BlockType.Z:
                return Z[rotation];
            case ArcadeScreenTetris.BlockType.J:
                return J[rotation];
            case ArcadeScreenTetris.BlockType.L:
                return L[rotation];
            default:
                return I[rotation];
        }
    }
}
