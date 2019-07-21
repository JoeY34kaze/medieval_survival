using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class craftingPanelHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private NetworkPlayerStats stats;

    public Transform craftingTypeTabsList;
    public GameObject crafting_tab_button_prefab;

    public Transform recipe_item_list;
    public GameObject recipse_item_prefab;

    public Text recipeProductName;
    public Text recipeProductAmount;
    public Text recipeProductDuration;

    public Image recipeProductImage;
    public Text productDescription;

    public GameObject material_cost_prefab;
    public Transform material_cost_panel;

    public InputField craftOrder;
    private Item currentlySelectedItem;

    public Transform queue_list;
    public GameObject queue_element_prefab;

    public Text timer;
    private List<PredmetRecepie> queueRecepieList;
    private IEnumerator countingRoutine;
    private int current_craft_remaining_time;

    void OnEnable()
    {
        stats = transform.root.GetComponent<NetworkPlayerStats>();

        foreach (Transform child in this.craftingTypeTabsList) Destroy(child.gameObject);

        
        foreach (Item.Type tip in Item.Type.GetValues(typeof(Item.Type))) {
            

            //za vsak tip itema nared svoj tab - ish
            if (tip == Item.Type.head)
            {//za armorje
                GameObject btn = GameObject.Instantiate(crafting_tab_button_prefab);
                btn.transform.SetParent(this.craftingTypeTabsList);
                btn.transform.GetChild(0).GetComponent<Text>().text = "Armors";
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();

                List<Item.Type> allowed_types = new List<Item.Type>();
                allowed_types.Add(Item.Type.head);
                allowed_types.Add(Item.Type.chest);
                allowed_types.Add(Item.Type.hands);
                allowed_types.Add(Item.Type.legs);
                allowed_types.Add(Item.Type.feet);

                button.onClick.AddListener(delegate
                {
                    showRecipesForType(allowed_types);
                });

            }


            else if (tip == Item.Type.chest || tip == Item.Type.hands || tip == Item.Type.feet || tip == Item.Type.legs || tip == Item.Type.shield || tip == Item.Type.ranged || tip == Item.Type.backpack)
            {
                //nared nc ker tile tipi bojo ukluceni drugje
            }
            else if(tip==Item.Type.weapon)
            {
                GameObject btn = GameObject.Instantiate(crafting_tab_button_prefab);
                btn.transform.SetParent(this.craftingTypeTabsList);
                btn.transform.GetChild(0).GetComponent<Text>().text = "Weapons";
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                List<Item.Type> allowed_types = new List<Item.Type>();
                allowed_types.Add(Item.Type.weapon);
                allowed_types.Add(Item.Type.ranged);
                allowed_types.Add(Item.Type.shield);
                button.onClick.AddListener(
                    delegate
                    {
                        showRecipesForType(allowed_types);
                    });
            }
            else if (tip == Item.Type.tool)
            {
                GameObject btn = GameObject.Instantiate(crafting_tab_button_prefab);
                btn.transform.SetParent(this.craftingTypeTabsList);
                btn.transform.GetChild(0).GetComponent<Text>().text = "Tools";
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                List<Item.Type> allowed_types = new List<Item.Type>();
                allowed_types.Add(Item.Type.tool);
                allowed_types.Add(Item.Type.backpack);
                button.onClick.AddListener(
                    delegate
                    {
                        showRecipesForType(allowed_types);
                    });
            }
            else
            {
                GameObject btn = GameObject.Instantiate(crafting_tab_button_prefab);
                btn.transform.SetParent(this.craftingTypeTabsList);
                btn.transform.GetChild(0).GetComponent<Text>().text = tip.ToString();
                Button button = btn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                List<Item.Type> allowed_types = new List<Item.Type>();
                allowed_types.Add(tip);
                button.onClick.AddListener(
                    delegate
                    {
                        showRecipesForType(allowed_types);
                    });
            }
        }
    }
    private void Start()
    {
        this.queueRecepieList = new List<PredmetRecepie>();
    }

    /// <summary>
    /// called onbtnclick event. clears current recipes shown on panel and inputs new ones
    /// </summary>
    /// <param name="allowed_types"></param>
    private void showRecipesForType(List<Item.Type> allowed_types)
    {
        clearRecepiePanel();
        foreach (Item.Type t in allowed_types) {
            Debug.Log("adding recepies for type "+t.ToString());
            List<PredmetRecepie> recepti = Mapper.instance.getRecepiesForType(t);
            foreach (PredmetRecepie p in recepti) {
                AddRecepieToPanel(p);
            }
        }
    }

    private void AddRecepieToPanel(PredmetRecepie p) {
        GameObject btn = GameObject.Instantiate(recipse_item_prefab);//button


        btn.transform.SetParent(this.recipe_item_list);
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.GetComponent<Image>().sprite = p.Product.icon;
        Button button = btn.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            delegate
            {
                drawSelectedRecipe(p);
            });
    }
    private void clearRecepiePanel()
    {
        foreach (Transform child in this.recipe_item_list)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// v panelo izbranga recepta izrise parametre tega recepta. to je tretja panela z leve
    /// </summary>
    /// <param name="p"></param>
    private void drawSelectedRecipe(PredmetRecepie p)
    {
        this.currentlySelectedItem = p.Product;

        Debug.Log("selected recipe " + p.Product.Display_name);
        foreach (Transform child in this.material_cost_panel) Destroy(child.gameObject);

        this.recipeProductName.text=p.Product.Display_name;
        this.recipeProductAmount.text = "amount: "+p.final_quantity;
        this.recipeProductDuration.text = "time: " + p.crafting_time + "s";

        this.recipeProductImage.sprite = p.Product.icon;
        this.productDescription.text = p.Product.description;


        for (int i = 0; i < p.ingredients.Length;i++) {
            GameObject mat = GameObject.Instantiate(this.material_cost_prefab);

            mat.transform.SetParent(this.material_cost_panel);
            mat.transform.localScale = new Vector3(1, 1, 1);

            mat.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = p.ingredients[i].Display_name;
            mat.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "x"+p.ingredient_quantities[i];
        }


        this.craftOrder.text = "1";

    }

    public void onBtnClickReduceCraftOrder() {
        int current = 1;
        if (!this.craftOrder.text.Equals(""))
            current = int.Parse(this.craftOrder.text);
        if (current > 0)
            current -= 1;
        this.craftOrder.text = (current) + "";

        Debug.Log("Reducing craft order to " + this.craftOrder.text);
    }
    public void onBtnClickIncreaseCraftOrder()
    {
        int current = 1;
        if (!this.craftOrder.text.Equals(""))
            current = int.Parse(this.craftOrder.text);
        if (current > 0)
            current += 1;
            this.craftOrder.text = (current) + "";

        Debug.Log("Increasing craft order to " + this.craftOrder.text);
    }

    public void startCraftOrder() {
        int current = 1;
        if (!this.craftOrder.text.Equals(""))
            current = int.Parse(this.craftOrder.text);
        this.craftOrder.text = "1";
        Item i = this.currentlySelectedItem;
        int skin = 0;
        transform.root.GetComponent<NetworkPlayerInventory>().localStartCraftingRequest(i, current, skin);
    }


    internal void updateCraftingQueueWithServerData(List<PredmetRecepie> r, int time_remaining_of_current_craft)
    {
        if (r == null) {
            foreach (Transform c in this.queue_list) Destroy(c.gameObject);
            return;
        }else if (r.Count==0)
        {
            foreach (Transform c in this.queue_list) Destroy(c.gameObject);
            return;
        }

        this.queueRecepieList = r;
        //mas recepte k se trenutno craftajo na serverju, za vsazga mas tud timer in vse, zmer to na un panel, na vsazga nabij opcijo da skensla, rpc za skenslanje, logika za kenslanje je pa prakticno ze napisana. ene 3 ure in je done
        foreach (Transform c in this.queue_list) Destroy(c.gameObject);

        foreach (PredmetRecepie p in r) {
            GameObject btn = GameObject.Instantiate(queue_element_prefab, this.queue_list);

            btn.transform.localScale = new Vector3(1, 1, 1);
            btn.GetComponent<Image>().sprite = p.Product.icon;
            Button button = btn.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(
                delegate
                {
                    localCancelCraftRequest(p, btn.transform.GetSiblingIndex());
                });
        }
        this.current_craft_remaining_time = time_remaining_of_current_craft;
        if(this.countingRoutine!=null)StopCoroutine(this.countingRoutine);
        this.countingRoutine = Counter(time_remaining_of_current_craft);
            StartCoroutine(this.countingRoutine);//updejtamo timer z serverja
    }

    private void localCancelCraftRequest(PredmetRecepie p, int index_sibling)
    {
        transform.root.GetComponent<NetworkPlayerInventory>().localCancelCraftRequest(p, index_sibling);
    }

    /// <summary>
    /// samo lokalno se rihta, to je samo maska za playerja, vsa logika je na serverju
    /// </summary>
    internal void OnTimerEnd() {
        if (this.queue_list.childCount > 0)
        {
            Destroy(this.queue_list.GetChild(0).gameObject);
            if (this.queueRecepieList.Count > 1)
            {
                PredmetRecepie next = this.queueRecepieList[1];
                this.current_craft_remaining_time = next.crafting_time;
                this.queueRecepieList.Remove(this.queueRecepieList[0]);
                this.countingRoutine = null;
                this.countingRoutine = Counter(next.crafting_time);
                StartCoroutine(this.countingRoutine);
            }
            else
            {
                this.timer.text = "";
            }
        }
    }

    internal IEnumerator Counter(int seconds) {
        for (int i = seconds; i > 0; i--) {
            this.current_craft_remaining_time = i;
            this.timer.text = current_craft_remaining_time + "";
            yield return new WaitForSecondsRealtime(1);
        }
        OnTimerEnd();
    }
}


