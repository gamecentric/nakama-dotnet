/**
* Copyright 2021 The Nakama Authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace Nakama.Replicated
{
    public delegate bool HostValidationHandler<T>(T oldValue, T newValue);
    public delegate void ValueChangedHandler<T>(T oldValue, T newValue);

    /// <summary>
    /// A variable whose value is synchronized across all clients connected to the same match.
    /// </summary>
    public class ReplicatedVar<T>
    {
        /// <summary>
        /// If this delegate is set and the current client is a guest, then
        /// when a replicated value is set, this client will reach out to the
        /// host who will validate and if it's validated the host will send to all clients
        /// otherwise a ReplicationValidationException will be thrown on this device.
        /// </summary>
        public HostValidationHandler<T> OnHostValidate;
        public ValueChangedHandler<T> OnValueChangedLocal;
        public ValueChangedHandler<T> OnValueChangedRemote;
        public KeyValidationStatus KeyValidationStatus => _validationStatus;

        private KeyValidationStatus _validationStatus;
        private readonly Dictionary<string, T> _values = new Dictionary<string, T>();

        internal IUserPresence Self { get; set; }

        private readonly object _valueLock = new object();

        public void SetValue(IUserPresence presence, T value)
        {
            AssertSelf();
            SetValue(presence, value, ReplicatedClientType.Local, _validationStatus);
        }

        public void SetValue(T value)
        {
            AssertSelf();
            SetValue(Self, value);
        }

        public T GetValue()
        {
            AssertSelf();

            lock (_valueLock)
            {
                return _values[Self.UserId];
            }
        }

        public T GetValue(IUserPresence presence)
        {
            AssertSelf();

            lock (_valueLock)
            {
                if (_values.ContainsKey(presence.UserId))
                {
                    return _values[presence.UserId];
                }
                else
                {
                    throw new InvalidOperationException($"Tried retrieving a replicated value from an unrecognized user id: {presence.UserId}");
                }
            }
        }

        internal void SetValue(IUserPresence presence, T value, ReplicatedClientType source, KeyValidationStatus validationStatus)
        {
            AssertSelf();

            lock (_valueLock)
            {
                T oldValue = _values.ContainsKey(presence.UserId) ? _values[presence.UserId] : default(T);

                if (oldValue.Equals(value))
                {
                    return;
                }

                _values[presence.UserId] = value;
                _validationStatus = validationStatus;

                switch (source)
                {
                    case ReplicatedClientType.Local:
                    OnValueChangedLocal?.Invoke(oldValue, value);
                    break;
                    default:
                    OnValueChangedRemote?.Invoke(oldValue, value);
                    break;
                }
            }
        }

        internal void Clear()
        {
            Self = null;

            lock (_valueLock)
            {
                _values.Clear();
            }

            OnHostValidate = null;
            OnValueChangedLocal = null;
            OnValueChangedRemote = null;
        }

        private void AssertSelf()
        {
            if (Self == null)
            {
                throw new InvalidOperationException("Replicated variable must be registered to a match before use.");
            }
        }
    }
}