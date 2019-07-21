using BeardedManStudios.Forge.Networking.Frame;
using System;
using MainThreadManager = BeardedManStudios.Forge.Networking.Unity.MainThreadManager;

namespace BeardedManStudios.Forge.Networking.Generated
{
	public partial class NetworkObjectFactory : NetworkObjectFactoryBase
	{
		public override void NetworkCreateObject(NetWorker networker, int identity, uint id, FrameStream frame, Action<NetworkObject> callback)
		{
			if (networker.IsServer)
			{
				if (frame.Sender != null && frame.Sender != networker.Me)
				{
					if (!ValidateCreateRequest(networker, identity, id, frame))
						return;
				}
			}
			
			bool availableCallback = false;
			NetworkObject obj = null;
			MainThreadManager.Run(() =>
			{
				switch (identity)
				{
					case ChatManagerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new ChatManagerNetworkObject(networker, id, frame);
						break;
					case CubeForgeGameNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new CubeForgeGameNetworkObject(networker, id, frame);
						break;
					case ExampleProximityPlayerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new ExampleProximityPlayerNetworkObject(networker, id, frame);
						break;
					case Interactable_objectNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new Interactable_objectNetworkObject(networker, id, frame);
						break;
					case NetworkArmorStandNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkArmorStandNetworkObject(networker, id, frame);
						break;
					case NetworkBackpackNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkBackpackNetworkObject(networker, id, frame);
						break;
					case NetworkCameraNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkCameraNetworkObject(networker, id, frame);
						break;
					case NetworkContainerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkContainerNetworkObject(networker, id, frame);
						break;
					case NetworkGuildManagerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkGuildManagerNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerAnimationNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerAnimationNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerCameraHandlerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerCameraHandlerNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerCombatNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerCombatNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerInteractionNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerInteractionNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerInventoryNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerInventoryNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerMovementNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerMovementNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerNeutralStateHandlerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerNeutralStateHandlerNetworkObject(networker, id, frame);
						break;
					case NetworkPlayerStatsNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkPlayerStatsNetworkObject(networker, id, frame);
						break;
					case NetworkResourceNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkResourceNetworkObject(networker, id, frame);
						break;
					case NetworkStartupSynchronizerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkStartupSynchronizerNetworkObject(networker, id, frame);
						break;
					case NetworkWorldManagerNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new NetworkWorldManagerNetworkObject(networker, id, frame);
						break;
					case Network_bodyNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new Network_bodyNetworkObject(networker, id, frame);
						break;
					case TestNetworkObject.IDENTITY:
						availableCallback = true;
						obj = new TestNetworkObject(networker, id, frame);
						break;
				}

				if (!availableCallback)
					base.NetworkCreateObject(networker, identity, id, frame, callback);
				else if (callback != null)
					callback(obj);
			});
		}

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}