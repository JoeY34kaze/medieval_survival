using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][\"int\", \"int\"][][][\"int\", \"string\"][\"int\", \"int\"][\"int\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"p\", \"c\"][\"c\", \"p\"][\"b\", \"c\"][\"c\", \"b\"][\"b\", \"c\"][\"c\", \"b\"][\"l\", \"c\"][\"c\", \"l\"][][][\"succ\", \"items\"][\"f\", \"t\"][\"index\"]]")]
	public abstract partial class NetworkContainerBehavior : NetworkBehavior
	{
		public const byte RPC_PERSONAL_TO_CONTAINER = 0 + 5;
		public const byte RPC_CONTAINER_TO_PERSONAL = 1 + 5;
		public const byte RPC_BAR_TO_CONTAINER = 2 + 5;
		public const byte RPC_CONTAINER_TO_BAR = 3 + 5;
		public const byte RPC_BACKPACK_TO_CONTAINER = 4 + 5;
		public const byte RPC_CONTAINER_TO_BACKPACK = 5 + 5;
		public const byte RPC_LOADOUT_TO_CONTAINER = 6 + 5;
		public const byte RPC_CONTAINER_TO_LOADOUT = 7 + 5;
		public const byte RPC_PICKUP_REQUEST = 8 + 5;
		public const byte RPC_OPEN_REQUEST = 9 + 5;
		public const byte RPC_OPEN_RESPONSE = 10 + 5;
		public const byte RPC_CONTAINER_TO_CONTAINER = 11 + 5;
		public const byte RPC_DROP_ITEM = 12 + 5;
		
		public NetworkContainerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkContainerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("PersonalToContainer", PersonalToContainer, typeof(int), typeof(int));
			networkObject.RegisterRpc("ContainerToPersonal", ContainerToPersonal, typeof(int), typeof(int));
			networkObject.RegisterRpc("BarToContainer", BarToContainer, typeof(int), typeof(int));
			networkObject.RegisterRpc("ContainerToBar", ContainerToBar, typeof(int), typeof(int));
			networkObject.RegisterRpc("BackpackToContainer", BackpackToContainer, typeof(int), typeof(int));
			networkObject.RegisterRpc("ContainerToBackpack", ContainerToBackpack, typeof(int), typeof(int));
			networkObject.RegisterRpc("LoadoutToContainer", LoadoutToContainer, typeof(int), typeof(int));
			networkObject.RegisterRpc("ContainerToLoadout", ContainerToLoadout, typeof(int), typeof(int));
			networkObject.RegisterRpc("pickupRequest", pickupRequest);
			networkObject.RegisterRpc("openRequest", openRequest);
			networkObject.RegisterRpc("openResponse", openResponse, typeof(int), typeof(string));
			networkObject.RegisterRpc("ContainerToContainer", ContainerToContainer, typeof(int), typeof(int));
			networkObject.RegisterRpc("dropItem", dropItem, typeof(int));

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
			Initialize(new NetworkContainerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkContainerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// int p
		/// int c
		/// </summary>
		public abstract void PersonalToContainer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int c
		/// int p
		/// </summary>
		public abstract void ContainerToPersonal(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int b
		/// int c
		/// </summary>
		public abstract void BarToContainer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int c
		/// int b
		/// </summary>
		public abstract void ContainerToBar(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int b
		/// int c
		/// </summary>
		public abstract void BackpackToContainer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int c
		/// int b
		/// </summary>
		public abstract void ContainerToBackpack(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int l
		/// int c
		/// </summary>
		public abstract void LoadoutToContainer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int c
		/// int l
		/// </summary>
		public abstract void ContainerToLoadout(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void pickupRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void openRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void openResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ContainerToContainer(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void dropItem(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}