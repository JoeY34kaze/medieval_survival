using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[][\"int\"][][\"float\"][\"Quaternion\"][][\"Vector3\", \"Quaternion\", \"Quaternion\", \"byte\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[][\"fire_state\"][][\"y_rot\"][\"rot\"][][\"pos\", \"rot\", \"platform_rot\", \"state\"]]")]
	public abstract partial class NetworkedSiegeWeaponBehavior : NetworkBehavior
	{
		public const byte RPC_ADVANCE_STATE_REQUEST = 0 + 5;
		public const byte RPC_ATRIBUTE_UPDATE = 1 + 5;
		public const byte RPC_WEAPON_CHANGE_TRAJECTORY_REQUEST = 2 + 5;
		public const byte RPC_SIEGE_WEAPON_ROTATE_HORIZONTALLY = 3 + 5;
		public const byte RPC_SIEGE_WEAPON_ROTATION_UPDATE = 4 + 5;
		public const byte RPC_PICKUP_REQUEST = 5 + 5;
		public const byte RPC_INITIALIZATION = 6 + 5;
		
		public NetworkedSiegeWeaponNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkedSiegeWeaponNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("advance_state_request", advance_state_request);
			networkObject.RegisterRpc("atribute_update", atribute_update, typeof(int));
			networkObject.RegisterRpc("weapon_change_trajectory_request", weapon_change_trajectory_request);
			networkObject.RegisterRpc("siege_weapon_rotate_horizontally", siege_weapon_rotate_horizontally, typeof(float));
			networkObject.RegisterRpc("siege_weapon_rotation_update", siege_weapon_rotation_update, typeof(Quaternion));
			networkObject.RegisterRpc("pickup_request", pickup_request);
			networkObject.RegisterRpc("initialization", initialization, typeof(Vector3), typeof(Quaternion), typeof(Quaternion), typeof(byte));

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
			Initialize(new NetworkedSiegeWeaponNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkedSiegeWeaponNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void advance_state_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void atribute_update(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void weapon_change_trajectory_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void siege_weapon_rotate_horizontally(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void siege_weapon_rotation_update(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void pickup_request(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void initialization(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}