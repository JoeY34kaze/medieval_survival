using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"string\", \"string\", \"Color\", \"byte[]\"][\"uint\"][\"uint\", \"string\"][\"uint\", \"bool\"][\"uint\", \"string\", \"string\"][][][\"uint\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"name\", \"tag\", \"GuildColor\", \"Optional_Image\"][\"candidate\"][\"gm\", \"guild_name\"][\"gm\", \"response_guild\"][\"gm\", \"members_string\", \"gName\"][][][\"player\"]]")]
	public abstract partial class NetworkGuildManagerBehavior : NetworkBehavior
	{
		public const byte RPC_CREATE_OR_MODIFY_GUILD_REQUEST = 0 + 5;
		public const byte RPC_REQUEST_SEND_GUILD_INVITE = 1 + 5;
		public const byte RPC_SEND_GUILD_INVITE_TO_CANDIDATE = 2 + 5;
		public const byte RPC_GUILD_INVITE_RESPONSE = 3 + 5;
		public const byte RPC_GUILD_MEMBERS_UPDATE = 4 + 5;
		public const byte RPC_GET_GUILD_MEMBERS_REQUEST = 5 + 5;
		public const byte RPC_LEAVE_GUILD_REQUEST = 6 + 5;
		public const byte RPC_KICK_GUILD_REQUEST = 7 + 5;
		
		public NetworkGuildManagerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkGuildManagerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("CreateOrModifyGuildRequest", CreateOrModifyGuildRequest, typeof(string), typeof(string), typeof(Color), typeof(byte[]));
			networkObject.RegisterRpc("RequestSendGuildInvite", RequestSendGuildInvite, typeof(uint));
			networkObject.RegisterRpc("SendGuildInviteToCandidate", SendGuildInviteToCandidate, typeof(uint), typeof(string));
			networkObject.RegisterRpc("GuildInviteResponse", GuildInviteResponse, typeof(uint), typeof(bool));
			networkObject.RegisterRpc("GuildMembersUpdate", GuildMembersUpdate, typeof(uint), typeof(string), typeof(string));
			networkObject.RegisterRpc("GetGuildMembersRequest", GetGuildMembersRequest);
			networkObject.RegisterRpc("LeaveGuildRequest", LeaveGuildRequest);
			networkObject.RegisterRpc("KickGuildRequest", KickGuildRequest, typeof(uint));

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
			Initialize(new NetworkGuildManagerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkGuildManagerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// string name
		/// string tag
		/// Color GuildColor
		/// byte[] Optional_Image
		/// </summary>
		public abstract void CreateOrModifyGuildRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint candidate
		/// </summary>
		public abstract void RequestSendGuildInvite(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint gm
		/// string guild_name
		/// </summary>
		public abstract void SendGuildInviteToCandidate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint gm
		/// bool response_guild
		/// </summary>
		public abstract void GuildInviteResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint gm
		/// string members_string
		/// string gName
		/// </summary>
		public abstract void GuildMembersUpdate(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void GetGuildMembersRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void LeaveGuildRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void KickGuildRequest(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}