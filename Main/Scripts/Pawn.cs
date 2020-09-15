
public class Pawn : Chessman
{
    //TODO: Implement special moves
    bool isMoved = true;
    bool specialCondFill = false;
    public override bool[,] PossibleMove()
    {

        //in order: SM,CM ,CA, DM, DA, CFA, DFA, FA/SF, CH, DH
        base_stats = new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0 };

        //Special Move
        if ((isWhite && CurrentY == 1) || (!isWhite && CurrentY == 6))
        {
            isMoved = false;
            specialCondFill = false;
        }
        if (!isMoved && !specialCondFill)
        {
            base_stats[0] += 1;
            specialCondFill = true;
        }
        if (isMoved && specialCondFill)
            base_stats[0] -= 1;


        bool[,] r = ExtendedMoves();

        return r;
    }
}
