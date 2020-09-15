using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }
    public Chessman[,] Chessmans { set; get; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = true;
    //Metric: size of pieces/tiles of checkers
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    /* Vector representation:
     * Forward  = Positive Z || Backward = Negative Z
     * Right    = Positive X || Left     = Negative X
     * Top      = Positive Y || Bottom   = Negative Y
    */

    //Collection of prefabs:
    public List<GameObject> chessmanPrefabs;
    public List<GameObject> activeChessman;

    //Storing Current and Previous Materials
    private Material previousMat;
    public Material selectMat;
    public List<Material> relationshipHighlightMat;
    private List<Material> relationshipOriginalMat;

    //FIXME: Orient both knights properly!
    private Quaternion orientation = Quaternion.Euler(0, 180, 0);


    private TraitPreset traitPreset;
    private bool gameOver = false;
    public int numTurns;

    private void Start()
    {

        traitPreset = new TraitPreset();
        spawnAllChessmen();
        GenerateRelations();
        Instance = this;
        numTurns = 0;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        DrawChessboard();
        updateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0 && !gameOver)
            {
                if (selectedChessman == null)
                {
                    
                    //select it
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    //move it
                    MoveChessman(selectionX, selectionY);
                }
            }
        }
    }

    private void SelectChessman(int x, int y)
    {

        if (Chessmans[x, y] == null)
        {
            UIManager.Instance.CascadeSidebar();
            return;
        }


        //Get all possible moves and instantiate objects to indicate them
        allowedMoves = Chessmans[x, y].PossibleMove();

        selectedChessman = Chessmans[x, y];


        //Store this chessman's material and replace it with selectMat
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedChessman.GetComponent<MeshRenderer>().material = selectMat;
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);

        UIManager.Instance.CascadeSidebar(); // Refresh sidebar to destroy previously highlighted piece
        UIManager.Instance.PreviewPiece(selectedChessman._name);
        UIManager.Instance.DrawSidebar();
        UIManager.Instance.SetMorale(selectedChessman.emotion);

    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                //Capture
                if (c.GetType() == typeof(King))
                {
                    //end game
                    gameOver = true;
                    UIManager.Instance.GameOverSeq();
                    return;
                }
                activeChessman.Remove(c.gameObject);
                //Update emotions of other chessmans
                c.UpdateEmotions(c, selectedChessman, activeChessman);
                Destroy(c.gameObject);
            }

            //Promotion of Pawn - > Queen
            //Todo: Promote Pieces via UI
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if (y == 7 && isWhiteTurn)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    spawnChessman(1, x, 7, "White Queen");
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0 && !isWhiteTurn)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    spawnChessman(7, x, 0, "Black Queen");
                    activeChessman.Remove(c.gameObject);
                    selectedChessman = Chessmans[x, y];
                }
            }

            //Setting the selected chessman from possible chessman array to null because we are taking it out of stack
            //after moving it to position x, y, we set the chessman back to all possible chessmans
            //After making the movement we flip the value of isWhiteTurn to indicate end of turn
            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;

            isWhiteTurn = !isWhiteTurn;
            numTurns += 1;
            UIManager.Instance.CascadeSidebar();
            updateCam();


        }

        //Reset the selected material to original material
        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;
        UIManager.Instance.CascadeSidebar();

        //If move is not possible, then deselect the chessman
        selectedChessman = null;
        BoardHighlights.Instance.Hidehilights();
    }

    private void updateSelection()
    {
        //Checking for camera
        if (!Camera.main)
            return;

        RaycastHit hit;

        // Raycast from the location of the mouse position,
        // if it hits the chessboard layer (ChessPlane) within 25 unit distance,
        // return coordinate as hit 
        // LayerMask.GetMask is used to get the proper index of the layer ChessPlane
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25f, LayerMask.GetMask("ChessPlane")))
        {
            // Mapping mouse coordinates and mapping them to Chess board coordinates X and Z
            // Z is represented as Y here to make it easy to program,
            // also coordinates are typecasted to int to get cell's bot left corner
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }

    }

    private void DrawChessboard()
    {
        // Unit vector pointing to right and forward for 8 tiles in chess board
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightLine = Vector3.forward * 8;

        // Drawing a grid of lines to represent board
        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++)
            {
                // Reusing same vector to save some memory!
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }
        // Drawing selection square
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1)
                );

            Debug.DrawLine(
               Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
               Vector3.forward * (selectionY) + Vector3.right * (selectionX + 1)
               );
        }
    }

    // Returns calculated coordinates from passed board index
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }
    private void spawnAllChessmen()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];
        // White King:
        spawnChessman(0, 4, 0, "White King");
        // White Queen
        spawnChessman(1, 3, 0, "White Queen");

        // White Rooks
        spawnChessman(2, 0, 0, "White Rook");
        spawnChessman(2, 7, 0, "White Rook");

        // White Bishops
        spawnChessman(3, 2, 0, "White Bishop");
        spawnChessman(3, 5, 0, "White Bishop");
        // White Knights
        spawnChessman(4, 1, 0, "White Knight");
        spawnChessman(4, 6, 0, "White Knight");

        // White Pawns
        for (int i = 0; i < 8; i++)
        {
            spawnChessman(5, i, 1, "White Pawn");
        }

        // -------------------------------------

        // Black King:
        spawnChessman(6, 4, 7, "Black King");
        // Black Queen
        spawnChessman(7, 3, 7, "Black Queen");

        // Black Rooks
        spawnChessman(8, 0, 7, "Black Rook");
        spawnChessman(8, 7, 7, "Black Rook");

        // Black Bishops
        spawnChessman(9, 2, 7, "Black Bishop");
        spawnChessman(9, 5, 7, "Black Bishop");

        // Black Knights
        spawnChessman(10, 1, 7, "Black Knight");
        spawnChessman(10, 6, 7, "Black Knight");

        // Black Pawns
        for (int i = 0; i < 8; i++)
        {
            spawnChessman(11, i, 6, "Black Pawn");
        }

    }

   
    private void spawnChessman(int index, int x, int y, string _name)
    {
        // Spawn the object at index I facing towards orientation, then add it to active chessman list.
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        Chessmans[x, y]._name = _name;

        
        for (int i = 0; i < 3; i++)
        {
            var t = traitPreset.GetTrait(UnityEngine.Random.Range(0, traitPreset.allTraits.Count - 1));
            if (!Chessmans[x, y].traits.Contains(t))
                Chessmans[x, y].AddTrait(t);
        }

        if (Chessmans[x, y]._name == "White Knight" || Chessmans[x, y]._name == "White Bishop")
            go.transform.Rotate(Vector3.up, -180);

        // Set the object associated with the script as the parent of this new object
        go.transform.SetParent(transform);

        // Add this to the list of active chessmen
        activeChessman.Add(go);
    }

    //Here in case we decide to add a restart button instead of going back to main menu everytime
    public void restartGame()
    {
        foreach (GameObject go in activeChessman)
            Destroy(go);

        isWhiteTurn = true;
        numTurns = 0;
        gameOver = false;
        BoardHighlights.Instance.Hidehilights();
        spawnAllChessmen();

    }


    private void GenerateRelations()
    {
        foreach (GameObject go in activeChessman)
        {
            Chessman chessman = go.GetComponent<Chessman>();
            chessman.SetRelations(activeChessman);
        }
    }

    void updateCam()
    {
        if (!isWhiteTurn) // Rotate the camera to show black
        {
            Camera Cam = Camera.main;
            Cam.transform.Rotate(Vector3.up, -180);
            Cam.transform.Rotate(Vector3.right, 90);
            Cam.transform.position = new Vector3(4, 6, 11);
        }
        else
        {
            Camera Cam = Camera.main;
            Cam.transform.Rotate(Vector3.up, -180);
            Cam.transform.Rotate(Vector3.left, -90);
            Cam.transform.position = new Vector3(4, 6, -3);
        }
    }

    public void HighlightRelationShips()
    {
        relationshipOriginalMat = new List<Material>();
        if (selectedChessman != null)
        {
            foreach (GameObject go in activeChessman)
            {
                Chessman chessman = go.GetComponent<Chessman>();
                if (chessman.relations.ContainsKey(selectedChessman))
                {
                    float i = chessman.relations[selectedChessman];
                    var hmat = go.GetComponent<MeshRenderer>();
                    relationshipOriginalMat.Add(hmat.material);

                    if (i <= -0.5f)
                        hmat.material = relationshipHighlightMat[0]; //  hate 
                    else if (i < 0 && i > -0.5f)
                        hmat.material = relationshipHighlightMat[1]; //  Dislike
                    else if (i > 0 && i < 0.5f)
                        hmat.material = relationshipHighlightMat[2]; //  Like
                    else if (i > 0.5f)
                        hmat.material = relationshipHighlightMat[3]; //  Love

                }
            }
        }

    }

    public void RevertRelationshipHighlights()
    {
        foreach (GameObject go in activeChessman)
        {
            Chessman chessman = go.GetComponent<Chessman>();
            if (chessman.relations.ContainsKey(selectedChessman))
            {
                if (chessman.relations[selectedChessman] != 0)
                {
                    var hmat = go.GetComponent<MeshRenderer>();
                    hmat.material = relationshipOriginalMat[0];
                    relationshipOriginalMat.RemoveAt(0);
                }
            }
        }
    }

}
