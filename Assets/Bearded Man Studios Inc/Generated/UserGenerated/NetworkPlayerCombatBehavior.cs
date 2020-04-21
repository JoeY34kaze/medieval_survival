using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\"][\"int\", \"bool\", \"byte[]\", \"byte[]\", \"byte\", \"bool\", \"bool\", \"bool\"][\"byte\"][\"bool\", \"byte\"][][\"bool\"][\"byte\"][\"byte\"][][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"mode\"][\"remote_combat_mode\", \"remote_blocking\", \"equipped_weapon\", \"equipped_shield\", \"remote_direction\", \"readying_attack\", \"ready_attack\", \"executing_attack\"][\"direction\"][\"readying\", \"direction\"][][\"executing_attack\"][\"dir\"][\"dir\"][][]]")]
	public abstract partial class NetworkPlayerCombatBehavior : NetworkBehavior
	{
		public const byte RPC_CHANGE_COMBAT_MODE_RESPONSE = 0 + 5;
		public const byte RPC_SEND_ALL = 1 + 5;
		public const byte RPC_START_ATTACK_REQUEST = 2 + 5;
		public const byte RPC_START_ATTACK_RESPONSE = 3 + 5;
		public const byte RPC_EXECUTE_ATTACK_REQUEST = 4 + 5;
		public const byte RPC_EXECUTE_ATTACK_RESPONSE = 5 + 5;
		public const byte RPC_START_BLOCK_REQUEST = 6 + 5;
		public const byte RPC_START_BLOCK_RESPONSE = 7 + 5;
		public const byte RPC_STOP_BLOCK_REQ = 8 + 5;
		public const byte RPC_STOP_BLOCK_RESP = 9 + 5;
		
		public NetworkPlayerCombatNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlayerCombatNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("ChangeCombatModeResponse", ChangeCombatModeResponse, typeof(int));
			networkObject.RegisterRpc("SendAll", SendAll, typeof(int), typeof(bool), typeof(byte[]), typeof(byte[]), typeof(byte), typeof(bool), typeof(bool), typeof(bool));
			networkObject.RegisterRpc("start_attack_request", start_attack_request, typeof(byte));
			networkObject.RegisterRpc("start_attack_response", start_attack_response, typeof(bool), typeof(byte));
			networkObject.RegisterRpc("execute_attack_request", execute_attack_request);
			networkObject.RegisterRpc("execute_attack_response", execute_attack_response, typeof(bool));
			networkObject.RegisterRpc("start_block_request", start_block_request, typeof(byte));
			networkObject.RegisterRpc("start_block_response", start_block_response, typeof(byte));
			networkObject.RegisterRpc("stop_block_req", stop_block_req);
			networkObject.RegisterRpc("stop_block_resp", stop_block_resp);

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
		/// int mode
		/// </summary>
		public abstract void ChangeCombatModeResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int remote_combat_mode
		/// bool remote_blocking
		/// byte[] equipped_weapon
		/// byte[] equipped_shield
		/// byte remote_direction
		/// bool readying_attack
		/// bool ready_attack
		/// bool executing_attack
		/// </summary>
		public abstract void SendAll(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// byte direction
		/// </summary>
		public abstract void start_attack_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// bool readying
		/// byte direction
		/// </summary>
		public abstract void start_attack_response(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void execute_attack_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void execute_attack_response(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void start_block_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void start_block_response(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void stop_block_req(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void stop_block_resp(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}