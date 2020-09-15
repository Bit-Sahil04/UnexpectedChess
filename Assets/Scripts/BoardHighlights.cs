using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlights : MonoBehaviour
{
    
    //This is our object spawning pooler
    //IT will spawn highlight objects (max : N movable tiles)
    public static BoardHighlights Instance { set; get; }
    public GameObject highlightPrefab;
    private List<GameObject> highlights;

    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject getHighLightObject()
    {
        //Find the first inactive object from highlights list
        GameObject go = highlights.Find(g => !g.activeSelf);

        //If no inactive object is present, then instantiate one and add it to the list
        if(go == null)
        {
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }


        return go;
    }

    public void HighlightAllowedMoves(bool[,] moves)
    {
        for(int i = 0; i< 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if (moves[i, j])
                {
                    GameObject go = getHighLightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                }
            }
        }
    }
    
    public void Hidehilights()
    {
        foreach(GameObject go in highlights)
        {
            go.SetActive(false);
        }
    }
}
