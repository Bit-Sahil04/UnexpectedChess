using System.Collections.Generic;
using UnityEngine;

public class TraitPreset
{
    public List<Trait> allTraits;

    public TraitPreset(){
        allTraits=new List<Trait>();
        LoadTraits();
    }


    //in order: S:Name, F:Threshold, Duration, Cooldown, SM,CM ,CA, DM, DA, CFA, DFA, FA, CH, DH
    void LoadTraits()
    {
        //Loaidng the text file
        //Create a Resource folder under assets and place traits.csv file there

        TextAsset traitsData = Resources.Load("traits") as TextAsset;
        //Spliting text data into lines
        string[] lines = traitsData.text.Split('\n');

        //Ignore the first and the last line
        for (int i = 1; i < lines.Length - 1; i++)
        {
            string line = lines[i];
            string[] lineData = line.Split(',');

            string name = lineData[0];
            float threshold = float.Parse(lineData[1]);
            int dur = int.Parse(lineData[2]);
            int cd = int.Parse(lineData[3]);

            int sm = int.Parse(lineData[4]);
            int cm = int.Parse(lineData[5]);
            int ca = int.Parse(lineData[6]);
            int dm = int.Parse(lineData[7]);
            int da = int.Parse(lineData[8]);
            int cfa = int.Parse(lineData[9]);
            int dfa = int.Parse(lineData[10]);
            int fa = int.Parse(lineData[11]);
            int ch = int.Parse(lineData[12]);
            int dh = int.Parse(lineData[13]);
            


            Trait trait = new Trait(name, threshold, dur, cd, sm, cm, ca, dm,da, cfa, dfa,fa, ch, dh);
            allTraits.Add(trait);
        }
        
    }

    void AddTrait(Trait trait){
        allTraits.Add(trait);
    }

    public Trait GetTrait(int ind){
        return allTraits[ind];
    }
}
