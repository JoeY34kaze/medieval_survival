using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"Quaternion\", \"int\"][\"int\", \"bool\"][][\"string\"][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"rotation\", \"sibling_index_of_attachment_point\"][\"sib\", \"status\"][][\"predmet_string\"][]]")]
	public abstract partial class NetworkPlaceableBehavior : NetworkBehavior
	{
		public const byte RPC_NETWORK_PLACEABLE_ATTACHMENT_REQUEST = 0 + 5;
		public const byte RPC_NETWORK_ATTACHMENT_UPDATE = 1 + 5;
		public const byte RPC_CLIENT_DURABILITY_REQUEST = 2 + 5;
		public const byte RPC_SERVER_UPDATE_PREDMET = 3 + 5;
		public const byte RPC_CLIENT_REQUEST_PREDMET_UPDATE = 4 + 5;
		
		public NetworkPlaceableNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlaceableNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("NetworkPlaceableAttachmentRequest", NetworkPlaceableAttachmentRequest, typeof(Quaternion), typeof(int));
			networkObject.RegisterRpc("NetworkAttachmentUpdate", NetworkAttachmentUpdate, typeof(int), typeof(bool));
			networkObject.RegisterRpc("ClientDurabilityRequest", ClientDurabilityRequest);
			networkObject.RegisterRpc("ServerUpdatePredmet", ServerUpdatePredmet, typeof(string));
			networkObject.RegisterRpc("clientRequestPredmetUpdate", clientRequestPredmetUpdate);

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
			Initialize(new NetworkPlaceableNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkPlaceableNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// Quaternion rotation
		/// int sibling_index_of_attachment_point
		/// </summary>
		public abstract void NetworkPlaceableAttachmentRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int sib
		/// bool status
		/// </summary>
		public abstract void NetworkAttachmentUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ClientDurabilityRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ServerUpdatePredmet(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void clientRequestPredmetUpdate(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}