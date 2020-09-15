using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Need to refactor this a bit 
public class Trait
{
    //Stats
    public int SM;  // Straight Move
    public int CM;  // Cardinal Move
    public int CA;  // Cardinal Attack
    public int DM;  // Diagonal Move
    public int DA;  // Diagonal Attack
    public int CFA; // Cardinal Flank Attack
    public int DFA; // Diagonal Flank Attack
    public int SF;  // Straight Flank
    public int CH;  // Cardinal Hop
    public int DH;  // Diagonal Hop

    //Properties
    public string Name;
    public float threshold;
    public int duration;
    public int cooldown;
    public bool isActive;
    public int lastActiveTurn;
    public int timesActivated;
    public bool onCooldown;
    public int cooldownTurn;
    
    
    public Trait(string Name, float threshold, int duration, int cooldown, int straightMove,
        int cardinalMove, int cardinalAttack, int diagonalMove, int diagonalAttack, int cardinalFlankAttack,
        int diagonalFlankAttack, int straightFlank, int cardinalHop, int diagonalHop=0)
    {
        this.Name = Name;
        this.threshold = threshold;
        this.duration = duration;
        this.cooldown = cooldown;
        this.SM = straightMove;
        this.CM = cardinalMove;
        this.CA = cardinalAttack;
        this.DM = diagonalMove;
        this.DA = diagonalAttack;
        this.CFA = cardinalFlankAttack;
        this.DFA = diagonalFlankAttack;
        this.SF = straightFlank;
        this.CH = cardinalHop;
        this.DH = diagonalHop;

        this.timesActivated = -1;
    }

    public int[] GetStats()
    {
        int[] s = new int[]
        {
        this.SM,
        this.CM,
        this.CA,
        this.DM,
        this.DA,
        this.CFA,
        this.DFA,
        this.SF,
        this.CH,
        this.DH,
    };

        return s;
    }
    
    public void UpdateThreshold()
    {
        if (!this.isActive && this.timesActivated > 0)
            this.threshold += 0.5f;
    }
}
