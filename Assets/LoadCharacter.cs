using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.IO;
using UMA.CharacterSystem;
using static NetworkUMADnaHandler;

public class LoadCharacter : MonoBehaviour
{



    #region private constants
    private string _FEMALE = "HumanFemaleDCS";
    private string _MALE = "HumanMaleDCS";
    private DynamicCharacterAvatar avatar;
    private bool avatarCreated = false;
    #endregion

    internal MYUMADATA MyUmaData;
    [SerializeField] private UMAWardrobeRecipe[] male_Hair;
    [SerializeField] private UMAWardrobeRecipe[] female_Hair;


    private void Start()
    {
        if (this.avatar == null) this.avatar = GetComponent<DynamicCharacterAvatar>();
        update_uma_from_file();
    }


    public void AvatarCreated() {
        this.avatarCreated = true;
    }

    static string ReadString()
    {
        string path = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + "\\Medieval Survival\\characters\\current.txt";
        if (File.Exists(path))
        {


            //Read the text from directly from the test.txt file
            StreamReader reader = new StreamReader(path);
            string s = reader.ReadToEnd();
            reader.Close();
            return s;
        }
        return null;
    }

    private void update_uma_from_file()
    {

        string s = ReadString();
        if (s == null)
        {
            Debug.LogError("Error reading character file. Recreate character please! or Contact devs to fix this shit and send character file");
            return ;
        }
        MYUMADATA frankensteins_monster = new MYUMADATA();
        string[] data = s.Split('|');
        if (_MALE.Equals(data[0]))
        {
            frankensteins_monster.is_male = true;
        }
        string color_data = data[1];
        string[] color_parameters = color_data.Split('-');//[ skin_index  eye_index,hair_index,r,g,b,a]
        ///skin data
        frankensteins_monster.setColorSkin(float.Parse(color_parameters[0]), float.Parse(color_parameters[1]), float.Parse(color_parameters[2]), float.Parse(color_parameters[3]));
        //eyes
        frankensteins_monster.setColorEye(float.Parse(color_parameters[4]), float.Parse(color_parameters[5]), float.Parse(color_parameters[6]), float.Parse(color_parameters[7]));
        //hair
        frankensteins_monster.setColorHair(float.Parse(color_parameters[8]), float.Parse(color_parameters[9]), float.Parse(color_parameters[10]), float.Parse(color_parameters[11]));
        //hair variant
        frankensteins_monster.hair_variant = Int32.Parse(data[2]);
        frankensteins_monster.DNAFloatValues = generateDNAValuesFromString(data[3].Split(new[] { Environment.NewLine }, StringSplitOptions.None));

        StartCoroutine(locally_set_UMA(frankensteins_monster));

    }

    private Dictionary<string, float> generateDNAValuesFromString(string[] v)
    {
        Dictionary<string, float> res = new Dictionary<string, float>();
        for (int i = 0; i < v.Length; i++)
        {
            if (v[i] == null) break;
            if (v[i] == "") break;
            string[] line = v[i].Split('-');//ker je csv
            float value = float.Parse(line[1]);
            res.Add(line[0], value);
        }



        return res;
    }

    public void OnUmaCreated()
    {
        this.avatarCreated = true;
    }

    private IEnumerator locally_set_UMA(MYUMADATA load_data)
    {
        this.MyUmaData = load_data;

        Debug.Log("Updating UMADATA Locally");
        while (this.avatar == null)
        {
            this.avatar = GetComponent<DynamicCharacterAvatar>();
            Debug.Log("avatar was null waiting a fixedUpdate");
            yield return new WaitForFixedUpdate();
        }

        while (!this.avatarCreated)
        {
            Debug.Log("Avatar was not yet created. waiting a fixedUpdate.");
            yield return new WaitForFixedUpdate();
        }

        if (load_data.is_male)
        {
            avatar.ChangeRace(this._MALE);
            yield return new WaitForFixedUpdate();//Da UMA System nardi refresh rase traja neki casa..
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        ///skin data
        ChangeSkinRuntimeSilent(load_data.skin_color);
        //eyes
        change_eye_color_runtime_silent(load_data.eye_color);
        //hair
        change_hair_color_runtime_silent(load_data.hair_color);

        avatar.UpdateColors(true);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        //--------------------GENDER SPECIFIC PARAMETERS!----------------------

        //setup dna
        while (avatar.GetDNA() == null) { yield return new WaitForFixedUpdate(); }

        Dictionary<string, DnaSetter> dna = avatar.GetDNA();

        while (dna.Count != load_data.DNAFloatValues.Count)
        {
            Debug.LogWarning("dictionaries are not the same size! avatar :" + dna.Count + " , expected :" + load_data.DNAFloatValues.Count);
            dna = avatar.GetDNA();
            yield return new WaitForEndOfFrame();
        }

        foreach (KeyValuePair<string, DnaSetter> AvatarChromosome in dna)
        {



            foreach (KeyValuePair<string, float> loadChromosome in load_data.DNAFloatValues)
            {
                if (loadChromosome.Key.Equals(AvatarChromosome.Key))
                {
                    dna[AvatarChromosome.Key].Set(loadChromosome.Value);
                    //Debug.Log("Set " + AvatarChromosome.Key + " to value " + loadChromosome.Value);
                    break;
                }
            }


        }
        avatar.BuildCharacter();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();


        // avatar.ClearSlot("Hair");

        //hair variant
        if (load_data.is_male)
        { //male hair
            setHairSilent(this.male_Hair[load_data.hair_variant]);
        }
        else
        { //female hair
            setHairSilent(this.female_Hair[load_data.hair_variant]);
        }
        yield return new WaitForFixedUpdate();
        avatar.BuildCharacter();
    }

    private void setHairSilent(UMAWardrobeRecipe r)
    {
        avatar.SetSlot(r);
    }

    private void ChangeSkinRuntimeSilent(float[] r)
    {
        avatar.SetColor("Skin", new Color(r[0], r[1], r[2], r[3]), default, 0.25f, false);

    }

    private void change_eye_color_runtime_silent(float[] r)
    {
        avatar.SetColor("Eyes", new Color(r[0], r[1], r[2], r[3]), default, 0.25f, false);

    }

    private void change_hair_color_runtime_silent(float[] r)
    {
        avatar.SetColor("Hair", new Color(r[0], r[1], r[2], r[3]), default, 0.25f, false);
    }

}
