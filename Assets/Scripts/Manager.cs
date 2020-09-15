using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Manager : MonoBehaviour
{
    public GameObject parent;

    public GameObject textprefab;
    void ShowText(string txt){
        GameObject go=Instantiate(textprefab,new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        go.transform.SetParent(parent.transform);
        Text textHol=go.GetComponent<Text>();
        textHol.text=txt;

        Debug.Log("here");
    }

    void ShowList(List<string> list){
        foreach(string s in list){
            ShowText(s);
        }
    }

    void Update(){
        ShowText("test");
    }

}
