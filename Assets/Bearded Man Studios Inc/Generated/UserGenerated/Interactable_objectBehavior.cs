using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[][][\"Vector3\", \"Vector3\"][\"uint\"][][\"int\"][\"bool\"][\"string\"][\"Vector3\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[][][\"pos\", \"dir\"][\"helper_id\"][][\"tip\"][\"Closed\"][\"p\"][\"position\"]]")]
	public abstract partial class Interactable_objectBehavior : NetworkBehavior
	{
		public const byte RPC_DESTROY_WRAPPER = 0 + 5;
		public const byte RPC_HANDLE_ITEM_PICKUP_SERVER_SIDE = 1 + 5;
		public const byte RPC_APPLY_FORCE_ON_INSTANTIATION = 2 + 5;
		public const byte RPC_REVIVE_DOWNED_PLAYER_REQUEST = 3 + 5;
		public const byte RPC_REVIVE_DOWNED_PLAYER_RESPONSE = 4 + 5;
		public const byte RPC_DOOR_INTERACTION_REQUEST = 5 + 5;
		public const byte RPC_DOOR_STATE_UPDATE = 6 + 5;
		public const byte RPC_SET_PREDMET_FOR_PICKUP = 7 + 5;
		public const byte RPC_ON_CLIENT_AFTER_PICKUP = 8 + 5;
		
		public Interactable_objectNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (Interactable_objectNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("DestroyWrapper", DestroyWrapper);
			networkObject.RegisterRpc("HandleItemPickupServerSide", HandleItemPickupServerSide);
			networkObject.RegisterRpc("ApplyForceOnInstantiation", ApplyForceOnInstantiation, typeof(Vector3), typeof(Vector3));
			networkObject.RegisterRpc("ReviveDownedPlayerRequest", ReviveDownedPlayerRequest, typeof(uint));
			networkObject.RegisterRpc("ReviveDownedPlayerResponse", ReviveDownedPlayerResponse);
			networkObject.RegisterRpc("DoorInteractionRequest", DoorInteractionRequest, typeof(int));
			networkObject.RegisterRpc("DoorStateUpdate", DoorStateUpdate, typeof(bool));
			networkObject.RegisterRpc("SetPredmetForPickup", SetPredmetForPickup, typeof(string));
			networkObject.RegisterRpc("OnClientAfterPickup", OnClientAfterPickup, typeof(Vector3));

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
			Initialize(new Interactable_objectNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new Interactable_objectNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DestroyWrapper(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void HandleItemPickupServerSide(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ApplyForceOnInstantiation(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReviveDownedPlayerRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReviveDownedPlayerResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DoorInteractionRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void DoorStateUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SetPredmetForPickup(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void OnClientAfterPickup(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}