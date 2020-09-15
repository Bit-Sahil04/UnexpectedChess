using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Chessman : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    public List<Trait> traits=new List<Trait>();
    private List<Trait> activeTraits = new List<Trait>();
    public bool isWhite;
    public float emotion=0.0f;
    public string _name;

    //in order: SM,CM ,CA, DM, DA, CFA, DFA, FA, CH, DH
    protected int[] base_stats = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    protected int[] bonus_Stats = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public Dictionary<Chessman,float> relations=new Dictionary<Chessman, float>();


    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    public void AddTrait(Trait trait)
    {
        traits.Add(trait);
    }

    public void GetActiveTraits()
    {
        activeTraits.Clear();
        foreach (Trait t in traits)
        {
            bool satisfiedThresh = ((t.threshold >= 0 && emotion >= t.threshold) || (t.threshold <= 0 && emotion <= t.threshold));
            int N = BoardManager.Instance.numTurns;
            if (!t.isActive && satisfiedThresh && !(t.onCooldown))
            {
                t.isActive = true;
                t.lastActiveTurn = N;
                t.timesActivated += 1;
                activeTraits.Add(t);
            }
            else if (t.isActive && (t.duration + t.lastActiveTurn < N))
            {
                t.isActive = false;
                t.onCooldown = true;
                t.cooldownTurn = N - 1;
                t.UpdateThreshold();
                t.lastActiveTurn = N - 1; //This may cause errors
                RevertBonusStats(t);
            }
            if (t.isActive && t.lastActiveTurn + t.duration > N)
                activeTraits.Add(t);
        }
    }

    public void GetBonusStats()
    {
        foreach (Trait t in activeTraits)
        {
            int[] temparr = t.GetStats();
            for (int i = 0; i < temparr.Length; i++)
            {
                bonus_Stats[i] += temparr[i];
            }
        }
    }

    public void RevertBonusStats(Trait t)
    {
        int[] temparr = t.GetStats();
        for (int i = 0; i < temparr.Length; i++)
        {
            bonus_Stats[i] -= temparr[i];
        }
    }

    public bool IsValidPos(int i,int j)
    {
        return (i>=0 && i<8) && (j>=0 && j<8) && !(i == CurrentX && j == CurrentY) && (isWhite == BoardManager.Instance.isWhiteTurn);
    }


    public virtual bool[,] PossibleMove()
    {
        
        return new bool[8,8];
    }

    //Gives random relations
    public void SetRelations(List<GameObject> activeChessmans){
        foreach (GameObject go in activeChessmans){
            Chessman chessman=go.GetComponent<Chessman>();
            if(chessman!=this){
                if(chessman.isWhite==isWhite){
                    relations.Add(chessman, UnityEngine.Random.Range(-1.0f,1.0f));
                }
            }
        }
    }


    public void UpdateEmotions(Chessman diedChessmen,Chessman killer, List<GameObject> activeChessmans){
        foreach (GameObject go in activeChessmans){
            Chessman chessman =go.GetComponent<Chessman>();
            if(chessman.relations.ContainsKey(diedChessmen))
            {
                float val=chessman.relations[diedChessmen];
                chessman.emotion= (val >= 0) ? chessman.emotion - val: chessman.emotion + val; // Affect jealous pieces positively, and friendly pieces negatively
            }
            if (chessman.relations.ContainsKey(killer))
            {
                float val = chessman.relations[killer];
                chessman.emotion = (val >= 0) ? chessman.emotion + val : chessman.emotion - val;// Affect Jealous pieces negatively, and friendly pieces positively
            }
        }
    }


    
    public virtual bool[,] ExtendedMoves()
    {
        bool[,] extendedMoves = new bool[8, 8];
        
        GetActiveTraits();
        GetBonusStats();
        Debug.Log(GetChessmanInfo());
        //in order: SM,CM ,CA, DM, DA, CFA, DFA, FA, CH, DH
        for (int i = 1; i < bonus_Stats.Length; i++)
        {
            if((base_stats[i] + bonus_Stats[i]) != 0)
            {
                switch (i)
                {
                    case 1:
                        GetCardinalMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 2:
                        GetCardinalAttackMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 3:
                        GetDiagonalMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 4:
                        GetDiagonalAttackMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 5:
                        GetCardinalFlankAttackMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 6:
                        GetDiagonalFlankAttackMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 7:
                        GetStraightFlank(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 8:
                        GetCardinalHopMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    case 9:
                        GetDiagonalHopMoves(ref extendedMoves);
                        Debug.Log($"{i}");
                        break;

                    default:
                        Debug.LogError("UNIMPLEMENTED TRAIT");
                        break;
                }
            }
        }

        StraightMove(ref extendedMoves);
        
        PrintDebugArray(extendedMoves);
        

        return extendedMoves;
    }

    public string GetChessmanInfo()
    {
        string s = "";
        s += $"Name: {_name}\n";
        s += $"Emotion: {emotion}\n";
        s += $"Traits: ";

        foreach (Trait t in traits)
        {
            s += $"{t.Name} ";
        }
        s += "\n\nActive traits: ";

        foreach (Trait t in activeTraits)
        {
            s += $"{t.Name} \n";
            for (int i = 0; i < t.GetStats().Length; i++)
            {
                s += $"{t.GetStats()[i]}";
            }
        }

        s += "----end----";

        return s;
    }

    public void PrintDebugArray(bool[,] r)
    {
        //Printing Extended moves for debugging
        string arrayString = "";
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                arrayString += $"{(r[j, i] ? 1 : 0)} ";
            }
            arrayString += "\n";
        }

        Debug.Log(arrayString);
    }


    //in order: SM,CM ,CA, DM, DA, CFA, DFA, SF, CH, DH
    public virtual void GetCardinalMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[1] + base_stats[1];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;


        //Right
        int i = CurrentX;
        while(ex > i)
        {
            i++;
            if (!IsValidPos(i, CurrentY))
                break;
            c = BoardManager.Instance.Chessmans[i, CurrentY];

            if (c == null)
                r[i, CurrentY] = true;
            else
                break;
        }

        //Left
        i = CurrentX;
        while (sx < i)
        {
            i--;
            if (!IsValidPos(i, CurrentY))
                break;
            c = BoardManager.Instance.Chessmans[i, CurrentY];

            if (c == null)
                r[i, CurrentY] = true;
            else
                break;
        }

        //Up
        i = CurrentY;
        while (ey > i)
        {
            i++;
            if (!IsValidPos(CurrentX, i))
                break;
            c = BoardManager.Instance.Chessmans[CurrentX, i];

            if (c == null)
                r[CurrentX, i] = true;
            else
                break;
        }

        //Down
        i = CurrentY;
        while (sy < i)
        {
            i--;
            if (!IsValidPos(CurrentX, i))
                break;

            c = BoardManager.Instance.Chessmans[CurrentX, i];

            if (c == null)
                r[CurrentX, i] = true;
            else
                break;
        }

    }

    public virtual void GetCardinalAttackMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[2] + base_stats[2];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        int i;

        //Right
        i = CurrentX;
        while (ex > i)
        {
            i++;

            if (!IsValidPos(i, CurrentY))
                break;

            c = BoardManager.Instance.Chessmans[i, CurrentY];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, CurrentY] = true;
                    break;
                }
                else
                    break;
        }

        //Left
        i = CurrentX;
        while (sx < i)
        {
            i--;

            if (!IsValidPos(i, CurrentY))
                break;

            c = BoardManager.Instance.Chessmans[i, CurrentY];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, CurrentY] = true;
                    break;
                }
                else
                    break;
        }

        //Up
        i = CurrentY;
        while (ey > i)
        {
            i++;

            if (!IsValidPos(CurrentX, i))
                break;

            c = BoardManager.Instance.Chessmans[CurrentX, i];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[CurrentX, i] = true;
                    break;
                }
                else
                    break;
        }

        //Down
        i = CurrentY;
        while (sy < i)
        {
            i--;

            if (!IsValidPos(CurrentX, i))
                break;

            c = BoardManager.Instance.Chessmans[CurrentX, i];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[CurrentX, i] = true;
                    break;
                }
                else
                    break;
        }

    }

    public virtual void GetDiagonalMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[3] + base_stats[3];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        int i, j;

        //Top Left
        i = CurrentX;
        j = CurrentY;
        while (i > sx && j < ey)
        {
            i--;
            j++;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c == null)
                r[i, j] = true;
            else
                break;

        }

        //Top Right
        i = CurrentX;
        j = CurrentY;
        while (i < ex && j < ey)
        {
            i++;
            j++;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c == null)
                r[i, j] = true;
            else
                break;

        }

        //Bot Left
        i = CurrentX;
        j = CurrentY;
        while (i > sx && j > sy)
        {
            i--;
            j--;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c == null)
                r[i, j] = true;
            else
                break;

        }

        //Bot Right
        i = CurrentX;
        j = CurrentY;
        while (i < ex && j > sy)
        {
            i++;
            j--;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c == null)
                r[i, j] = true;
            else
                break;

        }
    }

    public virtual void GetDiagonalAttackMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[4] + base_stats[4];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        int i, j;

        //Top Left
        i = CurrentX;
        j = CurrentY;
        while (i > sx && j < ey)
        {
            i--;
            j++;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, j] = true;
                    break;
                }

        }

        //Top Right
        i = CurrentX;
        j = CurrentY;
        while (i < ex && j < ey)
        {
            i++;
            j++;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, j] = true;
                    break;
                }

        }

        //Bot Left
        i = CurrentX;
        j = CurrentY;
        while (i > sx && j > sy)
        {
            i--;
            j--;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, j] = true;
                    break;
                }

        }

        //Bot Right
        i = CurrentX;
        j = CurrentY;
        while (i < ex && j > sy)
        {
            i++;
            j--;

            if (!IsValidPos(i, j))
                break;

            c = BoardManager.Instance.Chessmans[i, j];

            if (c != null)
                if (c.isWhite != isWhite)
                {
                    r[i, j] = true;
                    break;
                }

        }
    }

    public virtual void GetCardinalFlankAttackMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[5] + base_stats[5];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        if(IsValidPos(sx, CurrentY + 1))
            r[sx, CurrentY + 1] = KMove(sx, CurrentY + 1); // Left Up

        if (IsValidPos(sx, CurrentY - 1))
            r[sx, CurrentY - 1] = KMove(sx, CurrentY - 1); // Left Down

        if (IsValidPos(CurrentX + 1, ey))
            r[CurrentX + 1, ey] = KMove(CurrentX + 1, ey); // Up Right

        if (IsValidPos(CurrentX - 1, ey))
            r[CurrentX - 1, ey] = KMove(CurrentX - 1, ey); // Up Left

        if (IsValidPos(ex, CurrentY - 1))
            r[ex, CurrentY - 1] = KMove(ex, CurrentY - 1); // Right up

        if (IsValidPos(ex, CurrentY + 1))
            r[ex, CurrentY + 1] = KMove(ex, CurrentY + 1); // Right Down

        if (IsValidPos(CurrentX - 1, sy))
            r[CurrentX - 1, sy] = KMove(CurrentX - 1, sy); // Down Left

        if (IsValidPos(CurrentX + 1, sy))
            r[CurrentX + 1, sy] = KMove(CurrentX + 1, sy); // Down Right


        bool KMove(int x, int y)
        {
                c = BoardManager.Instance.Chessmans[x, y];
                if (c == null)
                    return true;

                else if (isWhite != c.isWhite)
                    return true;

            return false;
        }
    }

    public virtual void GetDiagonalFlankAttackMoves(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[6] + base_stats[6];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        if (IsValidPos(sx, ey - 1))
            r[sx, ey - 1] = KMove(sx, ey - 1); // Left Up Down
        
        if (IsValidPos(sx + 1, ey))
            r[sx + 1, ey] = KMove(sx + 1, ey); // Left Up Right

        if (IsValidPos(ex, ey - 1))
            r[ex, ey - 1] = KMove(ex, ey - 1); // Right Up Down

        if (IsValidPos(ex - 1, ey))
            r[ex - 1, ey] = KMove(ex - 1, ey); // Right Up Left

        if (IsValidPos(sx + 1, sy))
            r[sx + 1, sy] = KMove(sx + 1, sy); // Left Down Right

        if (IsValidPos(sx, sy + 1))
            r[sx, sy + 1] = KMove(sy, sy + 1); // Left Down Up

        if (IsValidPos(ex - 1, sy))
            r[ex - 1, sy] = KMove(ex - 1, sy); // Right Down Left

        if (IsValidPos(ex, sy + 1))
            r[ex, sy + 1] = KMove(ex, sy + 1); // Right Down Up


        bool KMove(int x, int y)
        {
            c = BoardManager.Instance.Chessmans[x, y];
                if (c == null)
                    return true;

                else if (isWhite != c.isWhite)
                    return true;

            return false;
        }
    }

    public virtual void GetStraightFlank(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[7] + base_stats[7];

        int sy = CurrentY - AttackRange;
        int ey = CurrentY + AttackRange;

        if (isWhite)
        {
            if (IsValidPos(CurrentX - 1, ey))
                r[CurrentX - 1, ey] = KMove(CurrentX - 1, ey); // top left

            if (IsValidPos(CurrentX + 1, ey))
                r[CurrentX + 1, ey] = KMove(CurrentX + 1, ey); // top right
        }
        else
        {
            if (IsValidPos(CurrentX - 1, sy))
                r[CurrentX - 1, sy] = KMove(CurrentX - 1, sy); // Bot left

            if (IsValidPos(CurrentX + 1, sy))
                r[CurrentX + 1, sy] = KMove(CurrentX + 1, sy); // Bot right
        }

        bool KMove(int x, int y)
        {
           
            c = BoardManager.Instance.Chessmans[x, y];
            if (c!= null)
                if (isWhite != c.isWhite)
                    return true;

            return false;
        }

    }

    public virtual void GetCardinalHopMoves(ref bool[,] r)
    {
        int AttackRange = (int)bonus_Stats[8] + base_stats[8];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        if (IsValidPos(CurrentX, ey))
            r[CurrentX, ey] = KMove(CurrentX, ey); // Up

        if (IsValidPos(CurrentX, sy))
            r[CurrentX, sy] = KMove(CurrentX, sy); // Down

        if (IsValidPos(sx, CurrentY))
            r[sx, CurrentY] = KMove(sx, CurrentY); // Left

        if (IsValidPos(ex, CurrentY))
            r[ex, CurrentY] = KMove(ex, CurrentY); // Right


    }

    public virtual void GetDiagonalHopMoves(ref bool[,] r)
    {
        int AttackRange = (int)bonus_Stats[9] + base_stats[9];

        int sx = CurrentX - AttackRange;
        int sy = CurrentY - AttackRange;
        int ex = CurrentX + AttackRange;
        int ey = CurrentY + AttackRange;

        if (IsValidPos(ex, ey))
            r[ex, ey] = KMove(ex, ey); // Up - Right

        if (IsValidPos(sx, sy))
            r[sx, sy] = KMove(sx, sy); // Up - Left

        if (IsValidPos(sx, sy))
            r[sx, sy] = KMove(sx, sy); // Down - Right

        if (IsValidPos(ex, ey))
            r[ex, ey] = KMove(ex, ey); // Down - Left


    }

    public virtual void StraightMove(ref bool[,] r)
    {
        Chessman c;
        int AttackRange = (int)bonus_Stats[0] + base_stats[0];

        int sy = CurrentY - AttackRange;
        int ey = CurrentY + AttackRange;

        if (isWhite)
        {
            //Up
            int i = CurrentY;
            while (ey > i)
            {
                i++;
                if (!IsValidPos(CurrentX, i))
                    break;
                c = BoardManager.Instance.Chessmans[CurrentX, i];

                if (c == null)
                    r[CurrentX, i] = true;
                else
                    break;
            }
        }
        else
        { 
            //Down
            int i = CurrentY;
            while (sy < i)
            {
                i--;
                if (!IsValidPos(CurrentX, i))
                    break;

                c = BoardManager.Instance.Chessmans[CurrentX, i];

                if (c == null)
                    r[CurrentX, i] = true;
                else
                    break;
            }
        }

        if(AttackRange < 0) // paralysis condition
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    r[i, j] = false;
                }
            }
        }
    }

    private bool KMove(int x, int y)
    {
        Chessman c;
        if (IsValidPos(x, y))
        {
            c = BoardManager.Instance.Chessmans[x, y];
            if (c == null)
                return true;
        }

        return false;
    }


}

