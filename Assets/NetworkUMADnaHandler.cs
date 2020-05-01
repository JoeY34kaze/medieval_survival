using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.IO;
using UMA.CharacterSystem;

public class NetworkUMADnaHandler : NetworkPlayerUMADNABehavior
{



    #region private constants
    private string _FEMALE = "HumanFemaleDCS";
    private string _MALE = "HumanMaleDCS";
    private DynamicCharacterAvatar avatar;
    private bool avatarCreated = false;
    #endregion

    internal MYUMADATA MyUmaData;
    [SerializeField] private UMAWardrobeRecipe[] male_Hair;
    [SerializeField]  private UMAWardrobeRecipe[] female_Hair;
    

    protected override void  NetworkStart() {
        base.NetworkStart();
        if(this.avatar==null) this.avatar = GetComponent<DynamicCharacterAvatar>();
        if (networkObject.IsOwner)
        {
            send_data_to_server();
        }

        else if (!networkObject.IsOwner && !networkObject.IsServer)
            networkObject.SendRpc(RPC_REQUEST_D_N_A, Receivers.Server);
    }


    private void send_data_to_server()
    {
        //mal je jok ker ni taprav dictionary na zacetku vsaj zaenkrat
        networkObject.SendRpc(RPC_SEND_U_M_A_TO_SERVER, Receivers.Server, update_uma_from_file().ObjectToByteArray());
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

    private MYUMADATA update_uma_from_file()
    {

        string s = ReadString();
        if (s == null)
        {
            Debug.LogError("Error reading character file. Recreate character please! or Contact devs to fix this shit and send character file");
            return null;
        }
        MYUMADATA frankensteins_monster = new MYUMADATA();
        string[] data = s.Split('|');
        if (_MALE.Equals(data[0]))
        {
            frankensteins_monster.is_male = true;  
        }
        string color_data = data[1];
        string[] color_parameters = color_data.Split(',');//[ skin_index  eye_index,hair_index,r,g,b,a]
        ///skin data
        frankensteins_monster.setColorSkin(float.Parse(color_parameters[0]), float.Parse(color_parameters[1]), float.Parse(color_parameters[2]), float.Parse(color_parameters[3]));
        //eyes
        frankensteins_monster.setColorEye(float.Parse(color_parameters[4]), float.Parse(color_parameters[5]), float.Parse(color_parameters[6]), float.Parse(color_parameters[7]));
        //hair
        frankensteins_monster.setColorHair(float.Parse(color_parameters[8]), float.Parse(color_parameters[9]), float.Parse(color_parameters[10]), float.Parse(color_parameters[11]));
        //hair variant
        frankensteins_monster.hair_variant = Int32.Parse(data[2]);
        frankensteins_monster.DNAFloatValues=generateDNAValuesFromString(data[3].Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        return frankensteins_monster;

    }

    private Dictionary<string, float> generateDNAValuesFromString(string[] v)
    {
        Dictionary<string, float> res = new Dictionary<string, float>();
        for (int i = 0; i < v.Length; i++) {
            if (v[i] == null) break;
            if (v[i] == "") break;
            string[] line = v[i].Split(',');//ker je csv
            float value = float.Parse(line[1]);
            res.Add(line[0], value);
        }



        return res; 
    }

    public override void requestDNA(RpcArgs args)
    {
        if (networkObject.IsServer) {
            //prebere podatke in mu vrne nazaj
            networkObject.SendRpc(args.Info.SendingPlayer,RPC_UPDATE_U_M_A, this.MyUmaData.ObjectToByteArray());
        }
            
                
    }

    public override void SendUMAToServer(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId && networkObject.IsServer) {
            MYUMADATA received = args.GetNext<byte[]>().ByteArrayToObject<MYUMADATA>();
            Debug.Log(" UMA Data Received on Server! hair:"+received.hair_variant);
            received=checkReceivedData(received);
            Debug.Log(" UMA Data Normalized / checked / antihack checked or whatever on Server!");
            networkObject.SendRpc(RPC_UPDATE_U_M_A, Receivers.All, received.ObjectToByteArray());



        }
    }

    /// <summary>
    /// pogleda ce so vrednosti v pravih parametrih
    /// </summary>
    /// <param name="u"></param>
    /// <returns></returns>
    private MYUMADATA checkReceivedData(MYUMADATA u)
    {
        return u;
    }

    public override void UpdateUMA(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            StartCoroutine(locally_set_UMA(args.GetNext<byte[]>().ByteArrayToObject<MYUMADATA>()));
        }
    }

    public void OnUmaCreated() {
        this.avatarCreated = true;
    }

    private IEnumerator locally_set_UMA(MYUMADATA load_data)
    {
        this.MyUmaData = load_data;

        Debug.Log("Updating UMADATA Locally");
        while (this.avatar == null) {
            this.avatar = GetComponent<DynamicCharacterAvatar>();
            Debug.Log("avatar was null waiting a fixedUpdate");
            yield return new WaitForFixedUpdate();
        }

        while (!this.avatarCreated) {
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

        while (dna.Count != load_data.DNAFloatValues.Count) {
            Debug.LogWarning("dictionaries are not the same size! avatar :" + dna.Count + " , expected :" + load_data.DNAFloatValues.Count);
            dna = avatar.GetDNA();
            yield return new WaitForEndOfFrame();
        }

        foreach (KeyValuePair<string, DnaSetter> AvatarChromosome in dna)
        {
            
            

            foreach (KeyValuePair<string, float> loadChromosome in load_data.DNAFloatValues)
            {
                if (loadChromosome.Key.Equals(AvatarChromosome.Key)) {
                    dna[AvatarChromosome.Key].Set(loadChromosome.Value);
                    Debug.Log("Set " + AvatarChromosome.Key + " to value " + loadChromosome.Value);
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

    //za kasnejso implementacijo kko bo server handlov dna - breeding and stuff
    internal void requestDNAFromPlayer()
    {
        if (networkObject.IsServer)
            networkObject.SendRpc(RPC_REQUEST_D_N_A, Receivers.Owner);
    }


    [Serializable] public class MYUMADATA {




        public Dictionary<string, float> DNAFloatValues;

        public float[] skin_color;
        public float[] eye_color;
        public float[] hair_color;

        public int hair_variant;

        public bool is_male = false;

        public MYUMADATA() { 
        
        }

        public MYUMADATA(bool is_male,Dictionary<string, float> dna) {
            this.is_male = is_male;
            this.DNAFloatValues = dna;
        }

        #region COLOR SETTERS GETTERS
        public void setColorSkin(Color c) {
            this.skin_color = new float[4];
            this.skin_color[0] = c.r;
            this.skin_color[1] = c.g;
            this.skin_color[2] = c.b;
            this.skin_color[3] = c.a;
        }

        public void setColorSkin(float r, float g,float b, float a)
        {
            this.skin_color = new float[4];
            this.skin_color[0] = r;
            this.skin_color[1] = g;
            this.skin_color[2] = b;
            this.skin_color[3] = a;
        }

        public Color getColorSkin()
        {
            return new Color(this.skin_color[0], this.skin_color[1], this.skin_color[2], this.skin_color[3]);
        }

        public void setColorEye(Color c)
        {
            this.eye_color = new float[4];
            this.eye_color[0] = c.r;
            this.eye_color[1] = c.g;
            this.eye_color[2] = c.b;
            this.eye_color[3] = c.a;
        }
        public void setColorEye(float r, float g, float b, float a)
        {
            this.eye_color = new float[4];
            this.eye_color[0] = r;
            this.eye_color[1] = g;
            this.eye_color[2] = b;
            this.eye_color[3] = a;
        }

        public Color getColorEye()
        {
            return new Color(this.eye_color[0], this.eye_color[1], this.eye_color[2], this.eye_color[3]);
        }

        public void setColorHair(Color c)
        {
            this.hair_color = new float[4];
            this.hair_color[0] = c.r;
            this.hair_color[1] = c.g;
            this.hair_color[2] = c.b;
            this.hair_color[3] = c.a;
        }

        public void setColorHair(float r, float g, float  b, float a)
        {
            this.hair_color = new float[4];
            this.hair_color[0] = r;
            this.hair_color[1] = g;
            this.hair_color[2] = b;
            this.hair_color[3] = a;
        }

        public Color getColorHair()
        {
            return new Color(this.hair_color[0], this.hair_color[1], this.hair_color[2], this.hair_color[3]);
        }
        #endregion
    }
}
