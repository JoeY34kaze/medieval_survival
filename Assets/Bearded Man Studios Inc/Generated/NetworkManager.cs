using BeardedManStudios.Forge.Networking.Generated;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Unity
{
	public partial class NetworkManager : MonoBehaviour
	{
		public delegate void InstantiateEvent(INetworkBehavior unityGameObject, NetworkObject obj);
		public event InstantiateEvent objectInitialized;
		protected BMSByte metadata = new BMSByte();

		public GameObject[] ChatManagerNetworkObject = null;
		public GameObject[] CubeForgeGameNetworkObject = null;
		public GameObject[] ExampleProximityPlayerNetworkObject = null;
		public GameObject[] Interactable_objectNetworkObject = null;
		public GameObject[] NetworkArmorStandNetworkObject = null;
		public GameObject[] NetworkBackpackNetworkObject = null;
		public GameObject[] NetworkCameraNetworkObject = null;
		public GameObject[] NetworkContainerItemsNetworkObject = null;
		public GameObject[] NetworkContainerNetworkObject = null;
		public GameObject[] NetworkCraftingStationNetworkObject = null;
		public GameObject[] NetworkedSiegeProjectileNetworkObject = null;
		public GameObject[] NetworkedSiegeWeaponNetworkObject = null;
		public GameObject[] NetworkGuildManagerNetworkObject = null;
		public GameObject[] NetworkItemSpawnerNetworkObject = null;
		public GameObject[] NetworkLandClaimObjectNetworkObject = null;
		public GameObject[] NetworkPlaceableNetworkObject = null;
		public GameObject[] NetworkPlayerAnimationNetworkObject = null;
		public GameObject[] NetworkPlayerBedNetworkObject = null;
		public GameObject[] NetworkPlayerCameraHandlerNetworkObject = null;
		public GameObject[] NetworkPlayerCombatNetworkObject = null;
		public GameObject[] NetworkPlayerInteractionNetworkObject = null;
		public GameObject[] NetworkPlayerInventoryNetworkObject = null;
		public GameObject[] NetworkPlayerMovementNetworkObject = null;
		public GameObject[] NetworkPlayerNeutralStateHandlerNetworkObject = null;
		public GameObject[] NetworkPlayerStatsNetworkObject = null;
		public GameObject[] NetworkPlayerUMADNANetworkObject = null;
		public GameObject[] NetworkResourceNetworkObject = null;
		public GameObject[] NetworkTrapNetworkObject = null;
		public GameObject[] NetworkWorldManagerNetworkObject = null;
		public GameObject[] Network_bodyNetworkObject = null;
		public GameObject[] TestNetworkObject = null;

		protected virtual void SetupObjectCreatedEvent()
		{
			Networker.objectCreated += CaptureObjects;
		}

		protected virtual void OnDestroy()
		{
		    if (Networker != null)
				Networker.objectCreated -= CaptureObjects;
		}
		
		private void CaptureObjects(NetworkObject obj)
		{
			if (obj.CreateCode < 0)
				return;
				
			if (obj is ChatManagerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (ChatManagerNetworkObject.Length > 0 && ChatManagerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(ChatManagerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<ChatManagerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is CubeForgeGameNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (CubeForgeGameNetworkObject.Length > 0 && CubeForgeGameNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(CubeForgeGameNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<CubeForgeGameBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is ExampleProximityPlayerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (ExampleProximityPlayerNetworkObject.Length > 0 && ExampleProximityPlayerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(ExampleProximityPlayerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<ExampleProximityPlayerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is Interactable_objectNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (Interactable_objectNetworkObject.Length > 0 && Interactable_objectNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(Interactable_objectNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<Interactable_objectBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkArmorStandNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkArmorStandNetworkObject.Length > 0 && NetworkArmorStandNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkArmorStandNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkArmorStandBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkBackpackNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkBackpackNetworkObject.Length > 0 && NetworkBackpackNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkBackpackNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkBackpackBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkCameraNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkCameraNetworkObject.Length > 0 && NetworkCameraNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkCameraNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkCameraBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkContainerItemsNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkContainerItemsNetworkObject.Length > 0 && NetworkContainerItemsNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkContainerItemsNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkContainerItemsBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkContainerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkContainerNetworkObject.Length > 0 && NetworkContainerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkContainerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkContainerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkCraftingStationNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkCraftingStationNetworkObject.Length > 0 && NetworkCraftingStationNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkCraftingStationNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkCraftingStationBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkedSiegeProjectileNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkedSiegeProjectileNetworkObject.Length > 0 && NetworkedSiegeProjectileNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkedSiegeProjectileNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkedSiegeProjectileBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkedSiegeWeaponNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkedSiegeWeaponNetworkObject.Length > 0 && NetworkedSiegeWeaponNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkedSiegeWeaponNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkedSiegeWeaponBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkGuildManagerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkGuildManagerNetworkObject.Length > 0 && NetworkGuildManagerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkGuildManagerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkGuildManagerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkItemSpawnerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkItemSpawnerNetworkObject.Length > 0 && NetworkItemSpawnerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkItemSpawnerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkItemSpawnerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkLandClaimObjectNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkLandClaimObjectNetworkObject.Length > 0 && NetworkLandClaimObjectNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkLandClaimObjectNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkLandClaimObjectBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlaceableNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlaceableNetworkObject.Length > 0 && NetworkPlaceableNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlaceableNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlaceableBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerAnimationNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerAnimationNetworkObject.Length > 0 && NetworkPlayerAnimationNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerAnimationNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerAnimationBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerBedNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerBedNetworkObject.Length > 0 && NetworkPlayerBedNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerBedNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerBedBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerCameraHandlerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerCameraHandlerNetworkObject.Length > 0 && NetworkPlayerCameraHandlerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerCameraHandlerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerCameraHandlerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerCombatNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerCombatNetworkObject.Length > 0 && NetworkPlayerCombatNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerCombatNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerCombatBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerInteractionNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerInteractionNetworkObject.Length > 0 && NetworkPlayerInteractionNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerInteractionNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerInteractionBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerInventoryNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerInventoryNetworkObject.Length > 0 && NetworkPlayerInventoryNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerInventoryNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerInventoryBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerMovementNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerMovementNetworkObject.Length > 0 && NetworkPlayerMovementNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerMovementNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerMovementBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerNeutralStateHandlerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerNeutralStateHandlerNetworkObject.Length > 0 && NetworkPlayerNeutralStateHandlerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerNeutralStateHandlerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerNeutralStateHandlerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerStatsNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerStatsNetworkObject.Length > 0 && NetworkPlayerStatsNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerStatsNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerStatsBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkPlayerUMADNANetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkPlayerUMADNANetworkObject.Length > 0 && NetworkPlayerUMADNANetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkPlayerUMADNANetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkPlayerUMADNABehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkResourceNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkResourceNetworkObject.Length > 0 && NetworkResourceNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkResourceNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkResourceBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkTrapNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkTrapNetworkObject.Length > 0 && NetworkTrapNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkTrapNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkTrapBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is NetworkWorldManagerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (NetworkWorldManagerNetworkObject.Length > 0 && NetworkWorldManagerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(NetworkWorldManagerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<NetworkWorldManagerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is Network_bodyNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (Network_bodyNetworkObject.Length > 0 && Network_bodyNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(Network_bodyNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<Network_bodyBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is TestNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (TestNetworkObject.Length > 0 && TestNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(TestNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<TestBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
		}

		protected virtual void InitializedObject(INetworkBehavior behavior, NetworkObject obj)
		{
			if (objectInitialized != null)
				objectInitialized(behavior, obj);

			obj.pendingInitialized -= InitializedObject;
		}

		[Obsolete("Use InstantiateChatManager instead, its shorter and easier to type out ;)")]
		public ChatManagerBehavior InstantiateChatManagerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ChatManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<ChatManagerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<ChatManagerBehavior>().networkObject = (ChatManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateCubeForgeGame instead, its shorter and easier to type out ;)")]
		public CubeForgeGameBehavior InstantiateCubeForgeGameNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(CubeForgeGameNetworkObject[index]);
			var netBehavior = go.GetComponent<CubeForgeGameBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<CubeForgeGameBehavior>().networkObject = (CubeForgeGameNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateExampleProximityPlayer instead, its shorter and easier to type out ;)")]
		public ExampleProximityPlayerBehavior InstantiateExampleProximityPlayerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ExampleProximityPlayerNetworkObject[index]);
			var netBehavior = go.GetComponent<ExampleProximityPlayerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<ExampleProximityPlayerBehavior>().networkObject = (ExampleProximityPlayerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateInteractable_object instead, its shorter and easier to type out ;)")]
		public Interactable_objectBehavior InstantiateInteractable_objectNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(Interactable_objectNetworkObject[index]);
			var netBehavior = go.GetComponent<Interactable_objectBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<Interactable_objectBehavior>().networkObject = (Interactable_objectNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkArmorStand instead, its shorter and easier to type out ;)")]
		public NetworkArmorStandBehavior InstantiateNetworkArmorStandNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkArmorStandNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkArmorStandBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkArmorStandBehavior>().networkObject = (NetworkArmorStandNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkBackpack instead, its shorter and easier to type out ;)")]
		public NetworkBackpackBehavior InstantiateNetworkBackpackNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkBackpackNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkBackpackBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkBackpackBehavior>().networkObject = (NetworkBackpackNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkCamera instead, its shorter and easier to type out ;)")]
		public NetworkCameraBehavior InstantiateNetworkCameraNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkCameraNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkCameraBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkCameraBehavior>().networkObject = (NetworkCameraNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkContainerItems instead, its shorter and easier to type out ;)")]
		public NetworkContainerItemsBehavior InstantiateNetworkContainerItemsNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkContainerItemsNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkContainerItemsBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkContainerItemsBehavior>().networkObject = (NetworkContainerItemsNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkContainer instead, its shorter and easier to type out ;)")]
		public NetworkContainerBehavior InstantiateNetworkContainerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkContainerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkContainerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkContainerBehavior>().networkObject = (NetworkContainerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkCraftingStation instead, its shorter and easier to type out ;)")]
		public NetworkCraftingStationBehavior InstantiateNetworkCraftingStationNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkCraftingStationNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkCraftingStationBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkCraftingStationBehavior>().networkObject = (NetworkCraftingStationNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkedSiegeProjectile instead, its shorter and easier to type out ;)")]
		public NetworkedSiegeProjectileBehavior InstantiateNetworkedSiegeProjectileNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkedSiegeProjectileNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkedSiegeProjectileBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkedSiegeProjectileBehavior>().networkObject = (NetworkedSiegeProjectileNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkedSiegeWeapon instead, its shorter and easier to type out ;)")]
		public NetworkedSiegeWeaponBehavior InstantiateNetworkedSiegeWeaponNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkedSiegeWeaponNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkedSiegeWeaponBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkedSiegeWeaponBehavior>().networkObject = (NetworkedSiegeWeaponNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkGuildManager instead, its shorter and easier to type out ;)")]
		public NetworkGuildManagerBehavior InstantiateNetworkGuildManagerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkGuildManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkGuildManagerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkGuildManagerBehavior>().networkObject = (NetworkGuildManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkItemSpawner instead, its shorter and easier to type out ;)")]
		public NetworkItemSpawnerBehavior InstantiateNetworkItemSpawnerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkItemSpawnerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkItemSpawnerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkItemSpawnerBehavior>().networkObject = (NetworkItemSpawnerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkLandClaimObject instead, its shorter and easier to type out ;)")]
		public NetworkLandClaimObjectBehavior InstantiateNetworkLandClaimObjectNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkLandClaimObjectNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkLandClaimObjectBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkLandClaimObjectBehavior>().networkObject = (NetworkLandClaimObjectNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlaceable instead, its shorter and easier to type out ;)")]
		public NetworkPlaceableBehavior InstantiateNetworkPlaceableNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlaceableNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlaceableBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlaceableBehavior>().networkObject = (NetworkPlaceableNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerAnimation instead, its shorter and easier to type out ;)")]
		public NetworkPlayerAnimationBehavior InstantiateNetworkPlayerAnimationNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerAnimationNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerAnimationBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerAnimationBehavior>().networkObject = (NetworkPlayerAnimationNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerBed instead, its shorter and easier to type out ;)")]
		public NetworkPlayerBedBehavior InstantiateNetworkPlayerBedNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerBedNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerBedBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerBedBehavior>().networkObject = (NetworkPlayerBedNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerCameraHandler instead, its shorter and easier to type out ;)")]
		public NetworkPlayerCameraHandlerBehavior InstantiateNetworkPlayerCameraHandlerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerCameraHandlerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerCameraHandlerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerCameraHandlerBehavior>().networkObject = (NetworkPlayerCameraHandlerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerCombat instead, its shorter and easier to type out ;)")]
		public NetworkPlayerCombatBehavior InstantiateNetworkPlayerCombatNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerCombatNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerCombatBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerCombatBehavior>().networkObject = (NetworkPlayerCombatNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerInteraction instead, its shorter and easier to type out ;)")]
		public NetworkPlayerInteractionBehavior InstantiateNetworkPlayerInteractionNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerInteractionNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerInteractionBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerInteractionBehavior>().networkObject = (NetworkPlayerInteractionNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerInventory instead, its shorter and easier to type out ;)")]
		public NetworkPlayerInventoryBehavior InstantiateNetworkPlayerInventoryNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerInventoryNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerInventoryBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerInventoryBehavior>().networkObject = (NetworkPlayerInventoryNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerMovement instead, its shorter and easier to type out ;)")]
		public NetworkPlayerMovementBehavior InstantiateNetworkPlayerMovementNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerMovementNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerMovementBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerMovementBehavior>().networkObject = (NetworkPlayerMovementNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerNeutralStateHandler instead, its shorter and easier to type out ;)")]
		public NetworkPlayerNeutralStateHandlerBehavior InstantiateNetworkPlayerNeutralStateHandlerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerNeutralStateHandlerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerNeutralStateHandlerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerNeutralStateHandlerBehavior>().networkObject = (NetworkPlayerNeutralStateHandlerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerStats instead, its shorter and easier to type out ;)")]
		public NetworkPlayerStatsBehavior InstantiateNetworkPlayerStatsNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerStatsNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerStatsBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerStatsBehavior>().networkObject = (NetworkPlayerStatsNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkPlayerUMADNA instead, its shorter and easier to type out ;)")]
		public NetworkPlayerUMADNABehavior InstantiateNetworkPlayerUMADNANetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerUMADNANetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerUMADNABehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkPlayerUMADNABehavior>().networkObject = (NetworkPlayerUMADNANetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkResource instead, its shorter and easier to type out ;)")]
		public NetworkResourceBehavior InstantiateNetworkResourceNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkResourceNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkResourceBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkResourceBehavior>().networkObject = (NetworkResourceNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkTrap instead, its shorter and easier to type out ;)")]
		public NetworkTrapBehavior InstantiateNetworkTrapNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkTrapNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkTrapBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkTrapBehavior>().networkObject = (NetworkTrapNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetworkWorldManager instead, its shorter and easier to type out ;)")]
		public NetworkWorldManagerBehavior InstantiateNetworkWorldManagerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkWorldManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkWorldManagerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<NetworkWorldManagerBehavior>().networkObject = (NetworkWorldManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateNetwork_body instead, its shorter and easier to type out ;)")]
		public Network_bodyBehavior InstantiateNetwork_bodyNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(Network_bodyNetworkObject[index]);
			var netBehavior = go.GetComponent<Network_bodyBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<Network_bodyBehavior>().networkObject = (Network_bodyNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateTest instead, its shorter and easier to type out ;)")]
		public TestBehavior InstantiateTestNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(TestNetworkObject[index]);
			var netBehavior = go.GetComponent<TestBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<TestBehavior>().networkObject = (TestNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}

		/// <summary>
		/// Instantiate an instance of ChatManager
		/// </summary>
		/// <returns>
		/// A local instance of ChatManagerBehavior
		/// </returns>
		/// <param name="index">The index of the ChatManager prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public ChatManagerBehavior InstantiateChatManager(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ChatManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<ChatManagerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<ChatManagerBehavior>().networkObject = (ChatManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of CubeForgeGame
		/// </summary>
		/// <returns>
		/// A local instance of CubeForgeGameBehavior
		/// </returns>
		/// <param name="index">The index of the CubeForgeGame prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public CubeForgeGameBehavior InstantiateCubeForgeGame(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(CubeForgeGameNetworkObject[index]);
			var netBehavior = go.GetComponent<CubeForgeGameBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<CubeForgeGameBehavior>().networkObject = (CubeForgeGameNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of ExampleProximityPlayer
		/// </summary>
		/// <returns>
		/// A local instance of ExampleProximityPlayerBehavior
		/// </returns>
		/// <param name="index">The index of the ExampleProximityPlayer prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public ExampleProximityPlayerBehavior InstantiateExampleProximityPlayer(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ExampleProximityPlayerNetworkObject[index]);
			var netBehavior = go.GetComponent<ExampleProximityPlayerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<ExampleProximityPlayerBehavior>().networkObject = (ExampleProximityPlayerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of Interactable_object
		/// </summary>
		/// <returns>
		/// A local instance of Interactable_objectBehavior
		/// </returns>
		/// <param name="index">The index of the Interactable_object prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public Interactable_objectBehavior InstantiateInteractable_object(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(Interactable_objectNetworkObject[index]);
			var netBehavior = go.GetComponent<Interactable_objectBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<Interactable_objectBehavior>().networkObject = (Interactable_objectNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkArmorStand
		/// </summary>
		/// <returns>
		/// A local instance of NetworkArmorStandBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkArmorStand prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkArmorStandBehavior InstantiateNetworkArmorStand(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkArmorStandNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkArmorStandBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkArmorStandBehavior>().networkObject = (NetworkArmorStandNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkBackpack
		/// </summary>
		/// <returns>
		/// A local instance of NetworkBackpackBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkBackpack prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkBackpackBehavior InstantiateNetworkBackpack(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkBackpackNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkBackpackBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkBackpackBehavior>().networkObject = (NetworkBackpackNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkCamera
		/// </summary>
		/// <returns>
		/// A local instance of NetworkCameraBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkCamera prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkCameraBehavior InstantiateNetworkCamera(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkCameraNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkCameraBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkCameraBehavior>().networkObject = (NetworkCameraNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkContainerItems
		/// </summary>
		/// <returns>
		/// A local instance of NetworkContainerItemsBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkContainerItems prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkContainerItemsBehavior InstantiateNetworkContainerItems(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkContainerItemsNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkContainerItemsBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkContainerItemsBehavior>().networkObject = (NetworkContainerItemsNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkContainer
		/// </summary>
		/// <returns>
		/// A local instance of NetworkContainerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkContainer prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkContainerBehavior InstantiateNetworkContainer(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkContainerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkContainerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkContainerBehavior>().networkObject = (NetworkContainerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkCraftingStation
		/// </summary>
		/// <returns>
		/// A local instance of NetworkCraftingStationBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkCraftingStation prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkCraftingStationBehavior InstantiateNetworkCraftingStation(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkCraftingStationNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkCraftingStationBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkCraftingStationBehavior>().networkObject = (NetworkCraftingStationNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkedSiegeProjectile
		/// </summary>
		/// <returns>
		/// A local instance of NetworkedSiegeProjectileBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkedSiegeProjectile prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkedSiegeProjectileBehavior InstantiateNetworkedSiegeProjectile(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkedSiegeProjectileNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkedSiegeProjectileBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkedSiegeProjectileBehavior>().networkObject = (NetworkedSiegeProjectileNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkedSiegeWeapon
		/// </summary>
		/// <returns>
		/// A local instance of NetworkedSiegeWeaponBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkedSiegeWeapon prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkedSiegeWeaponBehavior InstantiateNetworkedSiegeWeapon(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkedSiegeWeaponNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkedSiegeWeaponBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkedSiegeWeaponBehavior>().networkObject = (NetworkedSiegeWeaponNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkGuildManager
		/// </summary>
		/// <returns>
		/// A local instance of NetworkGuildManagerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkGuildManager prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkGuildManagerBehavior InstantiateNetworkGuildManager(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkGuildManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkGuildManagerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkGuildManagerBehavior>().networkObject = (NetworkGuildManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkItemSpawner
		/// </summary>
		/// <returns>
		/// A local instance of NetworkItemSpawnerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkItemSpawner prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkItemSpawnerBehavior InstantiateNetworkItemSpawner(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkItemSpawnerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkItemSpawnerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkItemSpawnerBehavior>().networkObject = (NetworkItemSpawnerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkLandClaimObject
		/// </summary>
		/// <returns>
		/// A local instance of NetworkLandClaimObjectBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkLandClaimObject prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkLandClaimObjectBehavior InstantiateNetworkLandClaimObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkLandClaimObjectNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkLandClaimObjectBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkLandClaimObjectBehavior>().networkObject = (NetworkLandClaimObjectNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlaceable
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlaceableBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlaceable prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlaceableBehavior InstantiateNetworkPlaceable(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlaceableNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlaceableBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlaceableBehavior>().networkObject = (NetworkPlaceableNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerAnimation
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerAnimationBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerAnimation prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerAnimationBehavior InstantiateNetworkPlayerAnimation(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerAnimationNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerAnimationBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerAnimationBehavior>().networkObject = (NetworkPlayerAnimationNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerBed
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerBedBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerBed prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerBedBehavior InstantiateNetworkPlayerBed(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerBedNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerBedBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerBedBehavior>().networkObject = (NetworkPlayerBedNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerCameraHandler
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerCameraHandlerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerCameraHandler prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerCameraHandlerBehavior InstantiateNetworkPlayerCameraHandler(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerCameraHandlerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerCameraHandlerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerCameraHandlerBehavior>().networkObject = (NetworkPlayerCameraHandlerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerCombat
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerCombatBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerCombat prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerCombatBehavior InstantiateNetworkPlayerCombat(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerCombatNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerCombatBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerCombatBehavior>().networkObject = (NetworkPlayerCombatNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerInteraction
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerInteractionBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerInteraction prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerInteractionBehavior InstantiateNetworkPlayerInteraction(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerInteractionNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerInteractionBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerInteractionBehavior>().networkObject = (NetworkPlayerInteractionNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerInventory
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerInventoryBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerInventory prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerInventoryBehavior InstantiateNetworkPlayerInventory(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerInventoryNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerInventoryBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerInventoryBehavior>().networkObject = (NetworkPlayerInventoryNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerMovement
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerMovementBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerMovement prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerMovementBehavior InstantiateNetworkPlayerMovement(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerMovementNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerMovementBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerMovementBehavior>().networkObject = (NetworkPlayerMovementNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerNeutralStateHandler
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerNeutralStateHandlerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerNeutralStateHandler prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerNeutralStateHandlerBehavior InstantiateNetworkPlayerNeutralStateHandler(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerNeutralStateHandlerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerNeutralStateHandlerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerNeutralStateHandlerBehavior>().networkObject = (NetworkPlayerNeutralStateHandlerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerStats
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerStatsBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerStats prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerStatsBehavior InstantiateNetworkPlayerStats(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerStatsNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerStatsBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerStatsBehavior>().networkObject = (NetworkPlayerStatsNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkPlayerUMADNA
		/// </summary>
		/// <returns>
		/// A local instance of NetworkPlayerUMADNABehavior
		/// </returns>
		/// <param name="index">The index of the NetworkPlayerUMADNA prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkPlayerUMADNABehavior InstantiateNetworkPlayerUMADNA(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkPlayerUMADNANetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkPlayerUMADNABehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkPlayerUMADNABehavior>().networkObject = (NetworkPlayerUMADNANetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkResource
		/// </summary>
		/// <returns>
		/// A local instance of NetworkResourceBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkResource prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkResourceBehavior InstantiateNetworkResource(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkResourceNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkResourceBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkResourceBehavior>().networkObject = (NetworkResourceNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkTrap
		/// </summary>
		/// <returns>
		/// A local instance of NetworkTrapBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkTrap prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkTrapBehavior InstantiateNetworkTrap(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkTrapNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkTrapBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkTrapBehavior>().networkObject = (NetworkTrapNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of NetworkWorldManager
		/// </summary>
		/// <returns>
		/// A local instance of NetworkWorldManagerBehavior
		/// </returns>
		/// <param name="index">The index of the NetworkWorldManager prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public NetworkWorldManagerBehavior InstantiateNetworkWorldManager(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(NetworkWorldManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<NetworkWorldManagerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<NetworkWorldManagerBehavior>().networkObject = (NetworkWorldManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of Network_body
		/// </summary>
		/// <returns>
		/// A local instance of Network_bodyBehavior
		/// </returns>
		/// <param name="index">The index of the Network_body prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public Network_bodyBehavior InstantiateNetwork_body(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(Network_bodyNetworkObject[index]);
			var netBehavior = go.GetComponent<Network_bodyBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<Network_bodyBehavior>().networkObject = (Network_bodyNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of Test
		/// </summary>
		/// <returns>
		/// A local instance of TestBehavior
		/// </returns>
		/// <param name="index">The index of the Test prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public TestBehavior InstantiateTest(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(TestNetworkObject[index]);
			var netBehavior = go.GetComponent<TestBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<TestBehavior>().networkObject = (TestNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
	}
}
