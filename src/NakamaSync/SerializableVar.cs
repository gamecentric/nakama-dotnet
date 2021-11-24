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

using System.Runtime.Serialization;

namespace NakamaSync
{
    /// <summary>
    /// A data-transfer object for a sync var.
    /// </summary>
    internal class SerializableVar<T> : ISerializableVar<T>
    {
        [DataMember(Name="value"), Preserve]
        public T Value { get; set; }

        [DataMember(Name="lock_version"), Preserve]
        public int LockVersion { get; set; }

        [DataMember(Name="validation_status"), Preserve]
        public ValidationStatus Status { get; set; }

        [DataMember(Name="ack_type"), Preserve]
        public AckType AckType { get; set; }
    }

    internal enum AckType
    {
        None = 1,
        // other client acknowledging new client
        Handshake = 2,
        // host acknowledging value that needs validation
        Validation = 3,
    }
}