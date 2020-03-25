using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"byte\"][\"uint\"][\"int\", \"Vector3\", \"Vector3\"][\"int\", \"int\"][\"int\", \"int\"][\"string\", \"int\", \"int\"][][][\"string\"][\"int\"][\"int\", \"int\"][\"int\", \"int\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"tip\"][\"newOwner\"][\"index\", \"pos\", \"dir\"][\"i\", \"j\"][\"backpack_index\", \"inventory_index\"][\"loadout_type\", \"loadout_index\", \"inventory_index\"][][][\"data\"][\"index\"][\"backpack_index\", \"bar_index\"][\"backInd\", \"BarInd\"]]")]
	public abstract partial class NetworkBackpackBehavior : NetworkBehavior
	{
		public const byte RPC_BACKPACK_INTERACTION_REQUEST = 0 + 5;
		public const byte RPC_OWNERSHIP_CHANGE_RESPONSE = 1 + 5;
		public const byte RPC_BACKPACK_DROP_ITEM_REQUEST = 2 + 5;
		public const byte RPC_BACKPACK_SWAP_ITEMS_REQUEST = 3 + 5;
		public const byte RPC_INVENTORY_TO_BACKPACK_SWAP_REQUEST = 4 + 5;
		public const byte RPC_LOADOUT_TO_BACKPACK_REQUEST = 5 + 5;
		public const byte RPC_BACKPACK_UNEQUIP_REQUEST = 6 + 5;
		public const byte RPC_BACKPACK_UNEQUIP_RESPONSE = 7 + 5;
		public const byte RPC_BACKPACK_ITEMS_OWNER_RESPONSE = 8 + 5;
		public const byte RPC_BACKPACK_TO_LOADOUT_REQUEST = 9 + 5;
		public const byte RPC_BACKPACK_TO_BAR_REQUEST = 10 + 5;
		public const byte RPC_BAR_TO_BACKPACK_REQUEST = 11 + 5;
		
		public NetworkBackpackNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkBackpackNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("BackpackInteractionRequest", BackpackInteractionRequest, typeof(byte));
			networkObject.RegisterRpc("OwnershipChangeResponse", OwnershipChangeResponse, typeof(uint));
			networkObject.RegisterRpc("BackpackDropItemRequest", BackpackDropItemRequest, typeof(int), typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("BackpackSwapItemsRequest", BackpackSwapItemsRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("InventoryToBackpackSwapRequest", InventoryToBackpackSwapRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("LoadoutToBackpackRequest", LoadoutToBackpackRequest, typeof(string), typeof(int), typeof(int));
			networkObject.RegisterRpc("BackpackUnequipRequest", BackpackUnequipRequest);
			networkObject.RegisterRpc("BackpackUnequipResponse", BackpackUnequipResponse);
			networkObject.RegisterRpc("BackpackItemsOwnerResponse", BackpackItemsOwnerResponse, typeof(string));
			networkObject.RegisterRpc("BackpackToLoadoutRequest", BackpackToLoadoutRequest, typeof(int));
			networkObject.RegisterRpc("BackpackToBarRequest", BackpackToBarRequest, typeof(int), typeof(int));
			networkObject.RegisterRpc("BarToBackpackRequest", BarToBackpackRequest, typeof(int), typeof(int));

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
			Initialize(new NetworkBackpackNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkBackpackNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// byte tip
		/// </summary>
		public abstract void BackpackInteractionRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint newOwner
		/// </summary>
		public abstract void OwnershipChangeResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int index
		/// Vector3 pos
		/// Vector3 dir
		/// </summary>
		public abstract void BackpackDropItemRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int i
		/// int j
		/// </summary>
		public abstract void BackpackSwapItemsRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int backpack_index
		/// int inventory_index
		/// </summary>
		public abstract void InventoryToBackpackSwapRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// string loadout_type
		/// int loadout_index
		/// int inventory_index
		/// </summary>
		public abstract void LoadoutToBackpackRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BackpackUnequipRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BackpackUnequipResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BackpackItemsOwnerResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BackpackToLoadoutRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BackpackToBarRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void BarToBackpackRequest(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}