using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterCreationHandler : MonoBehaviour
{

    #region Variables
    private CharacterCreationHandler _instance;
    public CharacterCreationHandler Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<CharacterCreationHandler>();
            return _instance;
        }
    }
    public static bool mouse_is_over_canvas = false;
    private EventSystem _eventSystem;
    private EventSystem EventSystem {
        get {
            if (_eventSystem == null)
                _eventSystem = GameObject.FindObjectOfType<EventSystem>();
            return _eventSystem;
        }
    }
    [SerializeField] private DynamicCharacterAvatar avatar;
    private Dictionary<string, DnaSetter> dna;

    [Header("general sliders ")]
    public Slider basic_height_slider;
    public Slider basic_weight_slider;
    public Slider basic_muscle_slider;

    [Header("head sliders")]
    public Slider headSize;
    public Slider headWidth;
    public Slider neckThickness;
    public Slider earsSize;
    public Slider earsPosition;
    public Slider earsRotation;
    public Slider noseSize;
    public Slider noseCurve;
    public Slider noseWidth;
    public Slider noseInclination;
    public Slider nosePosition;
    public Slider nosePronounced;
    public Slider noseFlatten;
    public Slider chinSize;
    public Slider chinPronounced;
    public Slider chinPosition;
    public Slider mandibleSize;
    public Slider jawsSize;
    public Slider jawsPosition;
    public Slider cheekSize;
    public Slider cheekPosition;
    public Slider lowCheekPronounced;
    public Slider lowCheekPosition;
    public Slider foreheadSize;
    public Slider foreheadPosition;
    public Slider lipsSize;
    public Slider mouthSize;
    public Slider eyeRotation;
    public Slider eyeSize;
    public Slider eyeSpacing;

    [Header("torso sliders ")]
    public Slider armLength;
    public Slider forearmLength;
    public Slider handsSize;
    public Slider breastSize;
    public Slider breastCleavage;

    [Header("legs sliders ")]

    public Slider feetSize;
    public Slider legSeparation;
    public Slider legsSize;
    public Slider gluteusSize;

    [Header(" panels ")]
    public GameObject ResetDialogue;
    public GameObject SaveDialogue;
    public GameObject OverLay;

    #endregion

    private void Start()
    {
        if (PlayerPrefs.HasKey("effectsVolume"))
            set_effects_volume(PlayerPrefs.GetFloat("effectsVolume"));
    }

    void set_effects_volume(float v)
    {
        GetComponent<AudioSource>().volume = v;
    }
    public void OnUMACreated()
    {
        this.dna = avatar.GetDNA();
    }

    private void Update()
    {
        CharacterCreationHandler.mouse_is_over_canvas = EventSystem.IsPointerOverGameObject();
    }


    #region UPDATING DNA

    public void OnUmaDataModified()
    {
        float min = 0;
        float max = 1f;
        //----------------MUSCLE
        if (this.basic_muscle_slider.value < max && this.basic_muscle_slider.value > min)
        {
            dna["upperMuscle"].Set(this.basic_muscle_slider.value);
            dna["lowerMuscle"].Set(this.basic_muscle_slider.value);
        }

        min = 0;
        max = 1f;
        //-----------------WEIGHT
        if (this.basic_weight_slider.value < max && this.basic_weight_slider.value > min)
        {
            dna["neckThickness"].Set(this.basic_weight_slider.value);
            dna["armWidth"].Set(this.basic_weight_slider.value);
            dna["forearmWidth"].Set(this.basic_weight_slider.value);
            dna["upperWeight"].Set(this.basic_weight_slider.value);
            dna["lowerWeight"].Set(this.basic_weight_slider.value);
            dna["belly"].Set(this.basic_weight_slider.value);
            dna["waist"].Set(this.basic_weight_slider.value);
        }
        //_-----------------HEIGHT
        min = 0.4f;
        max = 0.55f;
        if (this.basic_height_slider.value < max && this.basic_height_slider.value > min)
        {
            dna["height"].Set(this.basic_height_slider.value);
        }

        //----------------- HEAD OPTIONS
        min = 0.45f;
        max = 0.55f;
        if (this.headSize.value < max && this.headSize.value > min)
        {
            dna["headSize"].Set(this.headSize.value);
        }
        min = 0.25f;
        max = 0.75f;
        if (this.headWidth.value < max && this.headWidth.value > min)
        {
            dna["headWidth"].Set(this.headWidth.value);
        }


        min = 0;
        max = 1f;
        if (this.neckThickness.value < max && this.neckThickness.value > min)
        {
            dna["neckThickness"].Set(this.neckThickness.value);
        }

        min = 0;
        max = 1f;
        if (this.earsSize.value < max && this.earsSize.value > min)
        {
            dna["earsSize"].Set(this.earsSize.value);
        }

        min = 0;
        max = 1f;
        if (this.earsPosition.value < max && this.earsPosition.value > min)
        {
            dna["earsPosition"].Set(this.earsPosition.value);
        }

        min = 0;
        max = 1f;
        if (this.earsRotation.value < max && this.earsRotation.value > min)
        {
            dna["earsRotation"].Set(this.earsRotation.value);
        }

        min = 0;
        max = 1f;
        if (this.noseSize.value < max && this.noseSize.value > min)
        {
            dna["noseSize"].Set(this.noseSize.value);
        }

        min = 0;
        max = 1f;
        if (this.noseCurve.value < max && this.noseCurve.value > min)
        {
            dna["noseCurve"].Set(this.noseCurve.value);
        }

        min = 0;
        max = 1f;
        if (this.noseWidth.value < max && this.noseWidth.value > min)
        {
            dna["noseWidth"].Set(this.noseWidth.value);
        }

        min = 0;
        max = 1f;
        if (this.noseInclination.value < max && this.noseInclination.value > min)
        {
            dna["noseInclination"].Set(this.noseInclination.value);
        }

        min = 0;
        max = 1f;
        if (this.nosePosition.value < max && this.nosePosition.value > min)
        {
            dna["nosePosition"].Set(this.nosePosition.value);


        }

        min = 0;
        max = 1f;
        if (this.nosePronounced.value < max && this.nosePronounced.value > min)
        {
            dna["nosePronounced"].Set(this.nosePronounced.value);


        }
        min = 0;
        max = 1f;
        if (this.noseFlatten.value < max && this.noseFlatten.value > min)
        {
            dna["noseFlatten"].Set(this.noseFlatten.value);


        }

        min = 0;
        max = 1f;
        if (this.chinSize.value < max && this.chinSize.value > min)
        {
            dna["chinSize"].Set(this.chinSize.value);

        }

        min = 0;
        max = 1f;
        if (this.chinPronounced.value < max && this.chinPronounced.value > min)
        {
            dna["chinPronounced"].Set(this.chinPronounced.value);

        }

        min = 0;
        max = 1f;
        if (this.chinPosition.value < max && this.chinPosition.value > min)
        {
            dna["chinPosition"].Set(this.chinPosition.value);

        }

        min = 0;
        max = 1f;
        if (this.mandibleSize.value < max && this.mandibleSize.value > min)
        {
            dna["mandibleSize"].Set(this.mandibleSize.value);

        }

        min = 0;
        max = 1f;
        if (this.jawsSize.value < max && this.jawsSize.value > min)
        {
            dna["jawsSize"].Set(this.jawsSize.value);

        }

        min = 0;
        max = 1f;
        if (this.jawsPosition.value < max && this.jawsPosition.value > min)
        {
            dna["jawsPosition"].Set(this.jawsPosition.value);

        }

        min = 0;
        max = 1f;
        if (this.cheekSize.value < max && this.cheekSize.value > min)
        {
            dna["cheekSize"].Set(this.cheekSize.value);

        }
        min = 0;
        max = 1f;
        if (this.cheekPosition.value < max && this.cheekPosition.value > min) {

            dna["cheekPosition"].Set(this.cheekPosition.value);

        }

        min = 0;
        max = 1f;
        if (this.lowCheekPronounced.value < max && this.lowCheekPronounced.value > min) {

            dna["lowCheekPronounced"].Set(this.lowCheekPronounced.value);
        }


        min = 0;
        max = 1f;
        if (this.lowCheekPosition.value < max && this.lowCheekPosition.value > min)
        {

            dna["lowCheekPosition"].Set(this.lowCheekPosition.value);

        }

        min = 0;
        max = 1f;
        if (this.foreheadSize.value < max && this.foreheadSize.value > min)
        {

            dna["foreheadSize"].Set(this.foreheadSize.value);

        }
        min = 0;
        max = 1f;
        if (this.foreheadPosition.value < max && this.foreheadPosition.value > min)
        {

            dna["foreheadPosition"].Set(this.foreheadPosition.value);

        }
        min = 0;
        max = 1f;
        if (this.lipsSize.value < max && this.lipsSize.value > min)
        {

            dna["lipsSize"].Set(this.lipsSize.value);

        }
        min = 0;
        max = 1f;
        if (this.mouthSize.value < max && this.mouthSize.value > min)
        {

            dna["mouthSize"].Set(this.mouthSize.value);

        }

        min = 0;
        max = 1f;
        if (this.eyeRotation.value < max && this.eyeRotation.value > min)
        {

            dna["eyeRotation"].Set(this.eyeRotation.value);

        }
        min = 0;
        max = 1f;
        if (this.eyeSize.value < max && this.eyeSize.value > min)
        {

            dna["eyeSize"].Set(this.eyeSize.value);

        }

        min = 0;
        max = 1;
        if (this.eyeSpacing.value < max && this.eyeSpacing.value > min)
        {

            dna["eyeSpacing"].Set(this.eyeSpacing.value);

        }

        //----------------- TORSO OPTIONS

        min = 0.45f;
        max = 0.55f;
        if (this.armLength.value < max && this.armLength.value > min)
        {

            dna["armLength"].Set(this.armLength.value);

        }
        min = 0.45f;
        max = 0.55f;
        if (this.forearmLength.value < max && this.forearmLength.value > min)
        {

            dna["forearmLength"].Set(this.forearmLength.value);

        }
        min = 0;
        max = 1;
        if (this.handsSize.value < max && this.handsSize.value > min)
        {

            dna["handsSize"].Set(this.handsSize.value);

        }
        min = 0;
        max = 1;
        if (this.breastSize.value < max && this.breastSize.value > min)
        {

            dna["breastSize"].Set(this.breastSize.value);

        }
        min = 0;
        max = 1;
        if (this.breastCleavage.value < max && this.breastCleavage.value > min)
        {

            dna["breastCleavage"].Set(this.breastCleavage.value);

        }



        //----------------- LEGS OPTIONS


        min = 0.35f;
        max = 0.65f;
        if (this.feetSize.value < max && this.feetSize.value > min)
        {

            dna["feetSize"].Set(this.feetSize.value);

        }
        min = 0;
        max = 1;
        if (this.legSeparation.value < max && this.legSeparation.value > min)
        {

            dna["legSeparation"].Set(this.legSeparation.value);

        }
        min = 0.35f;
        max = 0.65f;
        if (this.legsSize.value < max && this.legsSize.value > min)
        {

            dna["legsSize"].Set(this.legsSize.value);

        }
        min = 0;
        max = 1;
        if (this.gluteusSize.value < max && this.gluteusSize.value > min)
        {

            dna["gluteusSize"].Set(this.gluteusSize.value);

        }

        avatar.BuildCharacter();

    }




    #endregion

    public void randomizeCharacter()
    {


    }

    public void openResetDialog() {
        this.ResetDialogue.SetActive(true);
    }

    public void openSaveDialogue() {
        this.SaveDialogue.SetActive(true);
    }

    public void SaveCharacter() {
        this.SaveDialogue.SetActive(false);
        this.ResetDialogue.SetActive(false);
    }

    public void CancelDialogue() {
        this.SaveDialogue.SetActive(false);
        this.ResetDialogue.SetActive(false);
    }

    public void ReturnToMainMenu() {
        StartCoroutine(FadeToBlack("MainMenu"));
    }

    public void ResetCharacter() {
        this.SaveDialogue.SetActive(false);
        this.ResetDialogue.SetActive(false);
    }
    
    private IEnumerator FadeToBlack(string v)
    {

        
        Image im = this.OverLay.GetComponent<Image>();
        im.raycastTarget = true;
        while (im.color.a < 1.0f)
        {
            if (im.color.a + 0.2f * Time.deltaTime > 1.0f)
                im.color = new Color(0, 0, 0, 1);
            else
                im.color = new Color(0, 0, 0, im.color.a + 1f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene(v);
    }
}
