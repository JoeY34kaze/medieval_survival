using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[][][][\"int\"][\"int\", \"bool\", \"string\", \"string\"][][\"bool\"][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[][][][\"mode\"][\"remote_combat_mode\", \"remote_blocking\", \"equipped_weapon\", \"equipped_shield\"][][\"blocking\"][]]")]
	public abstract partial class NetworkPlayerCombatBehavior : NetworkBehavior
	{
		public const byte RPC_NETWORK_FIRE2 = 0 + 5;
		public const byte RPC_NETWORK_FIRE1 = 1 + 5;
		public const byte RPC_NETWORK_FEIGN = 2 + 5;
		public const byte RPC_CHANGE_COMBAT_MODE_RESPONSE = 3 + 5;
		public const byte RPC_SEND_ALL = 4 + 5;
		public const byte RPC_NETWORK_FIRE1_RESPONSE = 5 + 5;
		public const byte RPC_NETWORK_FIRE2_RESPONSE = 6 + 5;
		public const byte RPC_NETWORK_FEIGN_RESPONSE = 7 + 5;
		
		public NetworkPlayerCombatNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlayerCombatNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("NetworkFire2", NetworkFire2);
			networkObject.RegisterRpc("NetworkFire1", NetworkFire1);
			networkObject.RegisterRpc("NetworkFeign", NetworkFeign);
			networkObject.RegisterRpc("ChangeCombatModeResponse", ChangeCombatModeResponse, typeof(int));
			networkObject.RegisterRpc("SendAll", SendAll, typeof(int), typeof(bool), typeof(string), typeof(string));
			networkObject.RegisterRpc("NetworkFire1Response", NetworkFire1Response);
			networkObject.RegisterRpc("NetworkFire2Response", NetworkFire2Response, typeof(bool));
			networkObject.RegisterRpc("NetworkFeignResponse", NetworkFeignResponse);

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
			Initialize(new NetworkPlayerCombatNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkPlayerCombatNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFire2(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFire1(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFeign(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ChangeCombatModeResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SendAll(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFire1Response(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFire2Response(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void NetworkFeignResponse(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}