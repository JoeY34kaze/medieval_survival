using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0]")]
	public partial class NetworkPlayerAnimationNetworkObject : NetworkObject
	{
		public const int IDENTITY = 12;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private Quaternion _chestRotation;
		public event FieldEvent<Quaternion> chestRotationChanged;
		public InterpolateQuaternion chestRotationInterpolation = new InterpolateQuaternion() { LerpT = 0f, Enabled = false };
		public Quaternion chestRotation
		{
			get { return _chestRotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_chestRotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_chestRotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetchestRotationDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_chestRotation(ulong timestep)
		{
			if (chestRotationChanged != null) chestRotationChanged(_chestRotation, timestep);
			if (fieldAltered != null) fieldAltered("chestRotation", _chestRotation, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			chestRotationInterpolation.current = chestRotationInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _chestRotation);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_chestRotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			chestRotationInterpolation.current = _chestRotation;
			chestRotationInterpolation.target = _chestRotation;
			RunChange_chestRotation(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _chestRotation);

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
				if (chestRotationInterpolation.Enabled)
				{
					chestRotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					chestRotationInterpolation.Timestep = timestep;
				}
				else
				{
					_chestRotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_chestRotation(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (chestRotationInterpolation.Enabled && !chestRotationInterpolation.current.UnityNear(chestRotationInterpolation.target, 0.0015f))
			{
				_chestRotation = (Quaternion)chestRotationInterpolation.Interpolate();
				//RunChange_chestRotation(chestRotationInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public NetworkPlayerAnimationNetworkObject() : base() { Initialize(); }
		public NetworkPlayerAnimationNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public NetworkPlayerAnimationNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
