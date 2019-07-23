using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0]")]
	public partial class NetworkPlayerCombatNetworkObject : NetworkObject
	{
		public const int IDENTITY = 13;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private byte _combatmode;
		public event FieldEvent<byte> combatmodeChanged;
		public Interpolated<byte> combatmodeInterpolation = new Interpolated<byte>() { LerpT = 0f, Enabled = false };
		public byte combatmode
		{
			get { return _combatmode; }
			set
			{
				// Don't do anything if the value is the same
				if (_combatmode == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_combatmode = value;
				hasDirtyFields = true;
			}
		}

		public void SetcombatmodeDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_combatmode(ulong timestep)
		{
			if (combatmodeChanged != null) combatmodeChanged(_combatmode, timestep);
			if (fieldAltered != null) fieldAltered("combatmode", _combatmode, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			combatmodeInterpolation.current = combatmodeInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _combatmode);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_combatmode = UnityObjectMapper.Instance.Map<byte>(payload);
			combatmodeInterpolation.current = _combatmode;
			combatmodeInterpolation.target = _combatmode;
			RunChange_combatmode(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _combatmode);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (combatmodeInterpolation.Enabled)
				{
					combatmodeInterpolation.target = UnityObjectMapper.Instance.Map<byte>(data);
					combatmodeInterpolation.Timestep = timestep;
				}
				else
				{
					_combatmode = UnityObjectMapper.Instance.Map<byte>(data);
					RunChange_combatmode(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (combatmodeInterpolation.Enabled && !combatmodeInterpolation.current.UnityNear(combatmodeInterpolation.target, 0.0015f))
			{
				_combatmode = (byte)combatmodeInterpolation.Interpolate();
				//RunChange_combatmode(combatmodeInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public NetworkPlayerCombatNetworkObject() : base() { Initialize(); }
		public NetworkPlayerCombatNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public NetworkPlayerCombatNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
