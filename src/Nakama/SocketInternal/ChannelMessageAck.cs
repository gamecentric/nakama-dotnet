/**
* Copyright 2020 The Nakama Authors
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

namespace Nakama.SocketInternal
{
    /// <inheritdoc cref="IChannelMessageAck"/>
    public class ChannelMessageAck : IChannelMessageAck
    {
        [DataMember(Name = "channel_id"), Preserve] public string ChannelId { get; set; }

        [DataMember(Name = "code"), Preserve] public int Code { get; set; }

        [DataMember(Name = "create_time"), Preserve] public string CreateTime { get; set; }

        [DataMember(Name = "message_id"), Preserve] public string MessageId { get; set; }

        [DataMember(Name = "persistent"), Preserve] public bool Persistent { get; set; }

        [DataMember(Name = "update_time"), Preserve] public string UpdateTime { get; set; }

        [DataMember(Name = "username"), Preserve] public string Username { get; set; }

        [DataMember(Name="room_name"), Preserve] public string RoomName { get; set; }

        [DataMember(Name="group_id"), Preserve] public string GroupId { get; set; }

        [DataMember(Name="user_id_one"), Preserve] public string UserIdOne { get; set; }

        [DataMember(Name="user_id_two"), Preserve] public string UserIdTwo { get; set; }

        public override string ToString()
        {
            return
                $"ChannelMessageAck(ChannelId='{ChannelId}', Code={Code}, CreateTime={CreateTime}, MessageId='{MessageId}', Persistent={Persistent}, UpdateTime={UpdateTime}, Username='{Username}', RoomName='{RoomName}', GroupId='{GroupId}', UserIdOne='{UserIdOne}', UserIdTwo='{UserIdTwo}')";
        }
    }
}