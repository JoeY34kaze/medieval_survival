using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"float\", \"string\"][\"float\", \"string\"][\"Vector3\"][][\"string\"][\"uint\"][\"uint\"][\"uint\", \"bool\"][][\"uint\"][][\"string\", \"string\", \"Color\", \"byte[]\"][\"Vector3\", \"Quaternion\", \"string\", \"bool\", \"float\", \"string\", \"string\", \"string\", \"string\", \"string\", \"string\"][][\"string\", \"bool\", \"bool\"][\"float\"][\"uint\", \"float\"][][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"health\", \"tag\"][\"damage\", \"tag\"][\"pos\"][][\"teamData\"][\"server_id\"][\"sendingPlayer\"][\"other\", \"resp\"][][\"other\"][][\"name_g\", \"tag_g\", \"color_g\", \"image_g\"][\"pos\", \"rot\", \"name_p\", \"dead\", \"hp\", \"head\", \"chest\", \"hands\", \"legs\", \"feet\", \"backpack\"][][\"player_name\", \"downed\", \"dead\"][\"hp\"][\"agresor\", \"cas_animacije\"][][]]")]
	public abstract partial class NetworkPlayerStatsBehavior : NetworkBehavior
	{
		public const byte RPC_SET_HEALTH = 0 + 5;
		public const byte RPC_RECEIVE_NOTIFICATION_FOR_DAMAGE_DEALT = 1 + 5;
		public const byte RPC_RESPAWN_SIGNAL = 2 + 5;
		public const byte RPC_ON_PLAYER_DEATH = 3 + 5;
		public const byte RPC_UPDATE_TEAM = 4 + 5;
		public const byte RPC_TEAM_INVITE_REQUEST = 5 + 5;
		public const byte RPC_TEAM_INVITE_REQUEST_TO_OTHER = 6 + 5;
		public const byte RPC_TEAM_INVITE_OTHER_RESPONSE = 7 + 5;
		public const byte RPC_TEAM_INVITE_NEGATIVE_RESPONSE = 8 + 5;
		public const byte RPC_TEAM_INVITE_ALREADY_IN_PARTY_NOTIFICATION = 9 + 5;
		public const byte RPC_TEAM_LEAVE_REQUEST = 10 + 5;
		public const byte RPC_GUILD_UPDATE = 11 + 5;
		public const byte RPC_RECEIVE_PERSONAL_DATA_ON_CONNECTION = 12 + 5;
		public const byte RPC_GET_ALL = 13 + 5;
		public const byte RPC_SEND_ALL = 14 + 5;
		public const byte RPC_REFRESH_HEALTH = 15 + 5;
		public const byte RPC_EXECUTION_REQUEST = 16 + 5;
		public const byte RPC_ON_BLOCKED = 17 + 5;
		public const byte RPC_RESPAWN_WITHOUT_BED_REQUEST = 18 + 5;
		
		public NetworkPlayerStatsNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlayerStatsNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("setHealth", setHealth, typeof(float), typeof(string));
			networkObject.RegisterRpc("ReceiveNotificationForDamageDealt", ReceiveNotificationForDamageDealt, typeof(float), typeof(string));
			networkObject.RegisterRpc("respawnSignal", respawnSignal, typeof(Vector3));
			networkObject.RegisterRpc("OnPlayerDeath", OnPlayerDeath);
			networkObject.RegisterRpc("UpdateTeam", UpdateTeam, typeof(string));
			networkObject.RegisterRpc("teamInviteRequest", teamInviteRequest, typeof(uint));
			networkObject.RegisterRpc("teamInviteRequestToOther", teamInviteRequestToOther, typeof(uint));
			networkObject.RegisterRpc("teamInviteOtherResponse", teamInviteOtherResponse, typeof(uint), typeof(bool));
			networkObject.RegisterRpc("teamInviteNegativeResponse", teamInviteNegativeResponse);
			networkObject.RegisterRpc("teamInviteAlreadyInPartyNotification", teamInviteAlreadyInPartyNotification, typeof(uint));
			networkObject.RegisterRpc("teamLeaveRequest", teamLeaveRequest);
			networkObject.RegisterRpc("GuildUpdate", GuildUpdate, typeof(string), typeof(string), typeof(Color), typeof(byte[]));
			networkObject.RegisterRpc("ReceivePersonalDataOnConnection", ReceivePersonalDataOnConnection, typeof(Vector3), typeof(Quaternion), typeof(string), typeof(bool), typeof(float), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
			networkObject.RegisterRpc("GetAll", GetAll);
			networkObject.RegisterRpc("SendAll", SendAll, typeof(string), typeof(bool), typeof(bool));
			networkObject.RegisterRpc("RefreshHealth", RefreshHealth, typeof(float));
			networkObject.RegisterRpc("ExecutionRequest", ExecutionRequest, typeof(uint), typeof(float));
			networkObject.RegisterRpc("OnBlocked", OnBlocked);
			networkObject.RegisterRpc("RespawnWithoutBedRequest", RespawnWithoutBedRequest);

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
			Initialize(new NetworkPlayerStatsNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkPlayerStatsNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// float health
		/// string tag
		/// </summary>
		public abstract void setHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// float damage
		/// string tag
		/// </summary>
		public abstract void ReceiveNotificationForDamageDealt(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// Vector3 pos
		/// </summary>
		public abstract void respawnSignal(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void OnPlayerDeath(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void UpdateTeam(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamInviteRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamInviteRequestToOther(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamInviteOtherResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamInviteNegativeResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamInviteAlreadyInPartyNotification(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void teamLeaveRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void GuildUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ReceivePersonalDataOnConnection(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void GetAll(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SendAll(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RefreshHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ExecutionRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void OnBlocked(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RespawnWithoutBedRequest(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}