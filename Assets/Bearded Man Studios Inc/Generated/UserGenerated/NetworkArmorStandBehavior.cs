using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"uint\", \"int\"][\"string\", \"string\", \"string\", \"string\", \"string\", \"string\", \"string\", \"string\"][][\"uint\", \"byte\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"server_id\", \"collider\"][\"head\", \"chest\", \"hands\", \"legs\", \"feet\", \"weapon\", \"shield\", \"ranged\"][][\"server_id\", \"tip_interakcije\"]]")]
	public abstract partial class NetworkArmorStandBehavior : NetworkBehavior
	{
		public const byte RPC_ARMOR_STAND_INTERACTION_REQUEST = 0 + 5;
		public const byte RPC_ARMOR_STAND_REFRESH = 1 + 5;
		public const byte RPC_NETWORK_REFRESH_REQUEST = 2 + 5;
		public const byte RPC_ARMOR_STAND_BULK_INTERACTION_REQUEST = 3 + 5;
		
		public NetworkArmorStandNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkArmorStandNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("ArmorStandInteractionRequest", ArmorStandInteractionRequest, typeof(uint), typeof(int));
			networkObject.RegisterRpc("ArmorStandRefresh", ArmorStandRefresh, typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
			networkObject.RegisterRpc("NetworkRefreshRequest", NetworkRefreshRequest);
			networkObject.RegisterRpc("ArmorStandBulkInteractionRequest", ArmorStandBulkInteractionRequest, typeof(uint), typeof(byte));

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
			Initialize(new NetworkArmorStandNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkArmorStandNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// uint server_id
		/// int collider
		/// </summary>
		public abstract void ArmorStandInteractionRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// string head
		/// string chest
		/// string hands
		/// string legs
		/// string feet
		/// string weapon
		/// string shield
		/// string ranged
		/// </summary>
		public abstract void ArmorStandRefresh(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkRefreshRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ArmorStandBulkInteractionRequest(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}