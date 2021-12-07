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
using Nakama;

namespace NakamaSync
{
    public interface ISyncMatch : IMatch
    {
        event Action<IHostChangedEvent> OnHostChanged;

        IUserPresence GetHostPresence();
        List<IUserPresence> GetGuestPresences();
        List<IUserPresence> GetAllPresences();
        bool IsSelfHost();
        void SetHost(IUserPresence newHost);
        void SendRpc(string rpcId, string targetId, IEnumerable<IUserPresence> targetPresences = null, object[] requiredParameters = null, object[] optionalParameters = null);
    }
}