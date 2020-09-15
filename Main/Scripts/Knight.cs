using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Chessman
{
    public override bool[,] PossibleMove()
    {
        //in order: SM,CM ,CA, DM, DA, CFA, DFA, FA/SF, CH, DH
        base_stats = new int[] { 0, 0, 0, 0, 0, 2, 0, 0, 0, 0 };

        bool[,] r = ExtendedMoves();


        return r;
    }

}
/*
 *     public override bool[,] ExtendedMoves()
    {
        bool[,] extendedMoves=new bool[8,8];

        int AttackRange=(int)GetAttackRange();

        int sx=CurrentX-AttackRange;
        int sy=CurrentY-AttackRange;
        int ex=CurrentX+AttackRange;
        int ey=CurrentY+AttackRange;


        
        Chessman c;
        for (int i=sx;i<=ex;i++){
            for(int j=sy;j<=ey;j++){
                if( IsValidPos(i,j)){
                    c = BoardManager.Instance.Chessmans[i, j];
                    if (c!=null)
                        if(c.isWhite != isWhite)
                            extendedMoves[i,j]=true;
                }
            }
        }
        return extendedMoves;
    }
 */