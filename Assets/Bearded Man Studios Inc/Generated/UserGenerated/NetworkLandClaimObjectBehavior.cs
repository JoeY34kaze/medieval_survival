using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"byte[]\"][\"byte[]\"][\"int\", \"int\", \"int\", \"int\", \"int\", \"int\", \"int\"][][][][\"byte[]\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"flag_image\"][\"img\"][\"year\", \"month\", \"day\", \"hour\", \"min\", \"s\", \"ms\"][][][][\"playerlist\"]]")]
	public abstract partial class NetworkLandClaimObjectBehavior : NetworkBehavior
	{
		public const byte RPC_SEND_FLAG_TEXTURE_TO_SERVER = 0 + 5;
		public const byte RPC_UPDATE_FLAG_TEXTURE_ON_CLIENTS = 1 + 5;
		public const byte RPC_SEND_DATE_TIME_PLACED = 2 + 5;
		public const byte RPC_AUTHORIZATION_REQUEST = 3 + 5;
		public const byte RPC_REMOVE_AUTHORIZATION_REQUEST = 4 + 5;
		public const byte RPC_CLEAR_ALL_AUTHORIZED_REQUEST = 5 + 5;
		public const byte RPC_UPDATE_AUTHORIZED_LIST = 6 + 5;
		
		public NetworkLandClaimObjectNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkLandClaimObjectNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("SendFlagTextureToServer", SendFlagTextureToServer, typeof(byte[]));
			networkObject.RegisterRpc("UpdateFlagTextureOnClients", UpdateFlagTextureOnClients, typeof(byte[]));
			networkObject.RegisterRpc("SendDateTimePlaced", SendDateTimePlaced, typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));
			networkObject.RegisterRpc("Authorization_request", Authorization_request);
			networkObject.RegisterRpc("Remove_authorization_request", Remove_authorization_request);
			networkObject.RegisterRpc("Clear_all_authorized_request", Clear_all_authorized_request);
			networkObject.RegisterRpc("UpdateAuthorizedList", UpdateAuthorizedList, typeof(byte[]));

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
			Initialize(new NetworkLandClaimObjectNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkLandClaimObjectNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// byte[] flag_image
		/// </summary>
		public abstract void SendFlagTextureToServer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// byte[] img
		/// </summary>
		public abstract void UpdateFlagTextureOnClients(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int year
		/// int month
		/// int day
		/// int hour
		/// int min
		/// int s
		/// int ms
		/// </summary>
		public abstract void SendDateTimePlaced(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Authorization_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Remove_authorization_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Clear_all_authorized_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void UpdateAuthorizedList(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}