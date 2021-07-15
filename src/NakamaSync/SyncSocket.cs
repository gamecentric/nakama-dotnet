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
using System.Linq;
using Nakama;

namespace NakamaSync
{
    internal class SyncSocket : ISyncService
    {
        public delegate void SyncEnvelopeHandler(IUserPresence source, Envelope envelope);
        public delegate void HandshakeRequestHandler(IUserPresence source, HandshakeRequest request);
        public delegate void HandshakeResponseHandler(IUserPresence source, HandshakeResponse response);

        public event SyncEnvelopeHandler OnSyncEnvelope;
        public event HandshakeRequestHandler OnHandshakeRequest;
        public event HandshakeResponseHandler OnHandshakeResponse;

        public SyncErrorHandler ErrorHandler { get; set; }
        public ILogger Logger { get; set; }

        private readonly SyncEncoding _encoding = new SyncEncoding();
        private readonly ISocket _socket;
        private readonly SyncOpcodes _opcodes;
        private readonly PresenceTracker _presenceTracker;
        private IMatch _match;
        private readonly object _lock = new object();

        public SyncSocket(ISocket socket, SyncOpcodes opcodes, PresenceTracker presenceTracker)
        {
            _socket = socket;
            _opcodes = opcodes;
            _presenceTracker = presenceTracker;
            _socket.ReceivedMatchState += HandleReceivedMatchState;
        }

        public void ReceiveMatch(IMatch match)
        {
            _match = match;
        }

        public void SendHandshakeRequest(HandshakeRequest request)
        {
            Logger?.InfoFormat($"User id {_match.Self.UserId} sending handshake request.");

            lock (_lock)
            {
                IUserPresence requestTarget = _presenceTracker.GetOthers().FirstOrDefault();

                if (requestTarget == null)
                {
                    ErrorHandler?.Invoke(new InvalidOperationException($"User {_match.Self.UserId} could not find user presence to send handshake to."));
                }

                _socket.SendMatchStateAsync(_match.Id, _opcodes.HandshakeRequestOpcode, _encoding.Encode(request), new IUserPresence[]{});
            }
        }

        public void SendHandshakeResponse(IUserPresence target, HandshakeResponse response)
        {
            Logger?.InfoFormat($"User id {_match.Self.UserId} sending handshake response.");
            lock (_lock)
            {
                _socket.SendMatchStateAsync(_match.Id, _opcodes.HandshakeResponseOpcode, _encoding.Encode(response), new IUserPresence[]{target});
            }
        }

        public void SendSyncDataToAll(Envelope envelope)
        {
            Logger?.DebugFormat($"User id {_match.Self.UserId} sending data to all");

            lock (_lock)
            {
                System.Console.WriteLine("sending match state async");
                _socket.SendMatchStateAsync(_match.Id, _opcodes.DataOpcode, _encoding.Encode(envelope));
            }
        }

        private void HandleReceivedMatchState(IMatchState state)
        {
            lock (_lock)
            {
                if (state.OpCode == _opcodes.DataOpcode)
                {
                    Logger?.InfoFormat($"Socket received sync envelope.");
                    Envelope envelope = _encoding.Decode<Envelope>(state.State);
                    OnSyncEnvelope(state.UserPresence, envelope);
                }
                else if (state.OpCode == _opcodes.HandshakeRequestOpcode)
                {
                    Logger?.InfoFormat("Socket received handshake request.");
                    OnHandshakeRequest?.Invoke(state.UserPresence, _encoding.Decode<HandshakeRequest>(state.State));
                }
                else if (state.OpCode == _opcodes.HandshakeResponseOpcode)
                {
                    Logger?.InfoFormat("Socket received handshake response.");
                    OnHandshakeResponse?.Invoke(state.UserPresence, _encoding.Decode<HandshakeResponse>(state.State));
                }
            }
        }
    }
}
