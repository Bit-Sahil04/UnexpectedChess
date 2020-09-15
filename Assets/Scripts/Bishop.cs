using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Chessman
{
    public override bool[,] PossibleMove()
    {
        //in order: ............SM, CM,CA, DM, DA, CFA, DFA, FA/SF, CH, DH
        base_stats = new int[] { 0, 0, 0, 8, 8, 0, 0, 0, 0, 0 };

        bool[,] r = ExtendedMoves();

        return r;
    }
}
