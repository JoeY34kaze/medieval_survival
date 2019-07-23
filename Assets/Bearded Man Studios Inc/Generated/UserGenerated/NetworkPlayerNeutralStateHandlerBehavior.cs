using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\"][\"string\", \"int\", \"string\", \"int\"][][\"int\"][\"Vector3\", \"Quaternion\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"index\"][\"item\", \"index_selected\", \"item2\", \"index_selected2\"][][\"tool_id\"][\"pos\", \"rot\"]]")]
	public abstract partial class NetworkPlayerNeutralStateHandlerBehavior : NetworkBehavior
	{
		public const byte RPC_BAR_SLOT_SELECTION_REQUEST = 0 + 5;
		public const byte RPC_BAR_SLOT_SELECTION_RESPONSE = 1 + 5;
		public const byte RPC_TOOL_USAGE_REQUEST = 2 + 5;
		public const byte RPC_TOOL_USAGE_RESPONSE = 3 + 5;
		public const byte RPC_PLACEMENTOF_ITEM_REQUEST = 4 + 5;
		
		public NetworkPlayerNeutralStateHandlerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (NetworkPlayerNeutralStateHandlerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("BarSlotSelectionRequest", BarSlotSelectionRequest, typeof(int));
			networkObject.RegisterRpc("BarSlotSelectionResponse", BarSlotSelectionResponse, typeof(string), typeof(int), typeof(string), typeof(int));
			networkObject.RegisterRpc("ToolUsageRequest", ToolUsageRequest);
			networkObject.RegisterRpc("ToolUsageResponse", ToolUsageResponse, typeof(int));
			networkObject.RegisterRpc("PlacementofItemRequest", PlacementofItemRequest, typeof(Vector3), typeof(Quaternion));

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
			Initialize(new NetworkPlayerNeutralStateHandlerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new NetworkPlayerNeutralStateHandlerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// int index
		/// </summary>
		public abstract void BarSlotSelectionRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// string item
		/// int index_selected
		/// string item2
		/// int index_selected2
		/// </summary>
		public abstract void BarSlotSelectionResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ToolUsageRequest(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void ToolUsageResponse(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void PlacementofItemRequest(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}