using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { set; get; }
    public float TransitionSpeed;
    private Vector3 DrawnPos;
    private Vector3 CascadePos;

    public List<GameObject> chessmanPrefabs;
    private GameObject activeChessman;
    private Transform spinner;
    private Dictionary<string, int> names;
    public Slider moraleSlider;

    void Start()
    {
        Instance = this;
        DrawnPos = transform.position;
        transform.position = new Vector3(-10, DrawnPos.y, DrawnPos.z);
        CascadePos = transform.position;
        spinner = transform.Find("SpinChessman");
        InitDict();

    }

    void InitDict()
    {
        names = new Dictionary<string, int>()
        {
            {"White King" ,  0},
            {"White Queen" , 1},
            {"White Rook" ,  2},
            {"White Bishop", 3},
            {"White Knight", 4},
            {"White Pawn" ,  5},

            {"Black King" ,  6},
            {"Black Queen" , 7},
            {"Black Rook" ,  8},
            {"Black Bishop", 9},
            {"Black Knight",10},
            {"Black Pawn" , 11},
        };
    }

    private void Update()
    {
        
    }

    public void GameOverSeq()
    {
        transform.Find("GameOver").gameObject.SetActive(true);
    }

    public void PreviewPiece(string _name)
    {
        int index = names[_name];
        activeChessman = chessmanPrefabs[index];
    }

    public void DrawSidebar()
    {
        transform.position = DrawnPos;
        GameObject go = Instantiate(activeChessman, spinner.transform.position, spinner.transform.rotation);
        go.transform.SetParent(spinner);
        
        foreach (Transform child in transform)
        {
            if(!child.gameObject.activeSelf && child.gameObject.name != "GameOver")
                child.gameObject.SetActive(true);
        }
    }
    public void CascadeSidebar()
    {
        transform.position = CascadePos;
        foreach (Transform child in spinner)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void SetMorale(float f)
    {
        moraleSlider.value = f;
    }

    public void BacktoMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
