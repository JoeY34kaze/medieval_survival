using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\"][][\"int\"][][][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"state\"][][\"state\"][][][]]")]
	public abstract partial class NetworkTrapBehavior : NetworkBehavior
	{
		public const byte RPC_SET_ANIMATION_STATE = 0 + 5;
		public const byte RPC_NETWORK_REFRESH_REQUEST = 1 + 5;
		public const byte RPC_REFRESH = 2 + 5;
		public const byte RPC_RELOAD = 3 + 5;
		public const byte RPC_PICKUP_REQUEST = 4 + 5;
		public const byte RPC_DESTROY__WRAPPER = 5 + 5;
		
		public NetworkTrapNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkTrapNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("setAnimationState", setAnimationState, typeof(int));
			networkObject.RegisterRpc("NetworkRefreshRequest", NetworkRefreshRequest);
			networkObject.RegisterRpc("Refresh", Refresh, typeof(int));
			networkObject.RegisterRpc("Reload", Reload);
			networkObject.RegisterRpc("PickupRequest", PickupRequest);
			networkObject.RegisterRpc("Destroy_Wrapper", Destroy_Wrapper);

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
			Initialize(new NetworkTrapNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkTrapNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// int state
		/// </summary>
		public abstract void setAnimationState(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkRefreshRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Refresh(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Reload(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void PickupRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Destroy_Wrapper(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}