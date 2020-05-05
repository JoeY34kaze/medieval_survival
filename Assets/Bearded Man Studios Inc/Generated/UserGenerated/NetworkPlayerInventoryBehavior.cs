using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"byte[]\", \"byte[]\"][\"byte[]\"][][\"byte[]\", \"Vector3\", \"Vector3\"][\"int\", \"Vector3\", \"Vector3\"][\"string\", \"int\", \"Vector3\", \"Vector3\"][\"int\", \"string\", \"int\"][\"int\", \"string\", \"int\"][\"int\", \"int\"][][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\", \"int\"][\"string\", \"int\"][\"int\", \"int\"][][\"int\"][\"int\", \"int\"][\"int\", \"int\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"personal_predmeti\", \"hotbar_predmeti\"][\"loadout_predmeti\"][][\"predmet\", \"pos\", \"dir\"][\"inventorySlotIndex\", \"camera_vector\", \"camera_forward\"][\"item_type\", \"index\", \"camera_vector\", \"camera_forward\"][\"index\", \"type\", \"index_inv\"][\"inv_index\", \"type\", \"loadout_index\"][\"a\", \"b\"][][\"a\", \"b\"][\"c\", \"d\"][\"e\", \"f\"][\"item_id\", \"quantity\", \"skin_id\"][\"item_ids\", \"remaining_crating_time_oif_first_item\"][\"index_itema\", \"index_sibling\"][][\"index\"][\"index\", \"amount\"][\"index\", \"quant\"]]")]
	public abstract partial class NetworkPlayerInventoryBehavior : NetworkBehavior
	{
		public const byte RPC_SEND_PERSONAL_INVENTORY_UPDATE = 0 + 5;
		public const byte RPC_SEND_LOADOUT_UPDATE = 1 + 5;
		public const byte RPC_REQUEST_LOADOUT_ON_CONNECT = 2 + 5;
		public const byte RPC_NETWORK_INSTANTIATION_SERVER_REQUEST = 3 + 5;
		public const byte RPC_DROP_ITEM_FROM_PERSONAL_INVENTORY_REQUEST = 4 + 5;
		public const byte RPC_DROP_ITEM_FROM_LOADOUT_REQUEST = 5 + 5;
		public const byte RPC_INVENTORY_TO_LOADOUT_REQUEST = 6 + 5;
		public const byte RPC_LOADOUT_TO_INVENTORY_REQUEST = 7 + 5;
		public const byte RPC_INVENTORY_TO_INVENTORY_REQUEST = 8 + 5;
		public const byte RPC_LOADOUT_TO_LOADOUT_REQUEST = 9 + 5;
		public const byte RPC_PERSONAL_TO_BAR_REQUEST = 10 + 5;
		public const byte RPC_BAR_TO_PERSONAL_REQUEST = 11 + 5;
		public const byte RPC_BAR_TO_BAR_REQUEST = 12 + 5;
		public const byte RPC_ITEM_CRAFTING_REQUEST = 13 + 5;
		public const byte RPC_ITEM_CRAFTING_RESPONSE = 14 + 5;
		public const byte RPC_ITEM_CRAFTING_CANCEL_REQUEST = 15 + 5;
		public const byte RPC_CRAFTING_QUEUE_UPDATE_REQUEST = 16 + 5;
		public const byte RPC_DROP_ITEM_FROM_BAR = 17 + 5;
		public const byte RPC_SPLIT_REQUEST_PERSONAL_INVENTORY = 18 + 5;
		public const byte RPC_SPLIT_REQUEST_BAR_INVENTORY = 19 + 5;
		
		public NetworkPlayerInventoryNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlayerInventoryNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("SendPersonalInventoryUpdate", SendPersonalInventoryUpdate, typeof(byte[]), typeof(byte[]));
			networkObject.RegisterRpc("SendLoadoutUpdate", SendLoadoutUpdate, typeof(byte[]));
			networkObject.RegisterRpc("RequestLoadoutOnConnect", RequestLoadoutOnConnect);
			networkObject.RegisterRpc("NetworkInstantiationServerRequest", NetworkInstantiationServerRequest, typeof(byte[]), typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("DropItemFromPersonalInventoryRequest", DropItemFromPersonalInventoryRequest, typeof(int), typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("DropItemFromLoadoutRequest", DropItemFromLoadoutRequest, typeof(string), typeof(int), typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("InventoryToLoadoutRequest", InventoryToLoadoutRequest, typeof(int), typeof(string), typeof(int));
			networkObject.RegisterRpc("LoadoutToInventoryRequest", LoadoutToInventoryRequest, typeof(int), typeof(string), typeof(int));
			networkObject.RegisterRpc("InventoryToInventoryRequest", InventoryToInventoryRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("LoadoutToLoadoutRequest", LoadoutToLoadoutRequest);
			networkObject.RegisterRpc("PersonalToBarRequest", PersonalToBarRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("BarToPersonalRequest", BarToPersonalRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("BarToBarRequest", BarToBarRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("ItemCraftingRequest", ItemCraftingRequest, typeof(int), typeof(int), typeof(int));
			networkObject.RegisterRpc("ItemCraftingResponse", ItemCraftingResponse, typeof(string), typeof(int));
			networkObject.RegisterRpc("ItemCraftingCancelRequest", ItemCraftingCancelRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("CraftingQueueUpdateRequest", CraftingQueueUpdateRequest);
			networkObject.RegisterRpc("DropItemFromBar", DropItemFromBar, typeof(int));
			networkObject.RegisterRpc("SplitRequestPersonalInventory", SplitRequestPersonalInventory, typeof(int), typeof(int));
			networkObject.RegisterRpc("SplitRequestBarInventory", SplitRequestBarInventory, typeof(int), typeof(int));

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new NetworkPlayerInventoryNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkPlayerInventoryNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// byte[] personal_predmeti
		/// byte[] hotbar_predmeti
		/// </summary>
		public abstract void SendPersonalInventoryUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// byte[] loadout_predmeti
		/// </summary>
		public abstract void SendLoadoutUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RequestLoadoutOnConnect(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkInstantiationServerRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DropItemFromPersonalInventoryRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DropItemFromLoadoutRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void InventoryToLoadoutRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void LoadoutToInventoryRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void InventoryToInventoryRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void LoadoutToLoadoutRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void PersonalToBarRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BarToPersonalRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BarToBarRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ItemCraftingRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ItemCraftingResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ItemCraftingCancelRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void CraftingQueueUpdateRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DropItemFromBar(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SplitRequestPersonalInventory(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SplitRequestBarInventory(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}