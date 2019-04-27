using UnityEngine;


public class ItemPickup : Interactable {
    public int item_id; //ujemat se mora z id-jem itema na playerju ce je na playerju al pa nevem
    public int quantity = 1;
    public bool stackable = false;
    //zaenkrat smao pove da je item k se ga lahko pobere



    internal override void interact(NetworkPlayerInteraction networkPlayerInteraction)
    {
        networkPlayerInteraction.HandleItemPickupServerSide(this.item_id, quantity);
    }
}
