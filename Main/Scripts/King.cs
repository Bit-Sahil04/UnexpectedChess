using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class King : Chessman
{

    public override bool[,] PossibleMove()
    {

        //in order: ............SM, CM,CA, DM, DA, CFA, DFA, FA/SF, CH, DH
        base_stats = new int[] { 0, 1, 1, 1, 1, 0, 0, 0, 0, 0 };

        bool[,] r = ExtendedMoves();
        return r;
    }
}

