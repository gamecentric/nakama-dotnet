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

using Nakama;
using System;
using System.Threading.Tasks;

namespace NakamaSync
{
    // todo entire concurrency pass on all this
    // enforce key ids should be unique across types
    // TODO what if someone changes the type collection that the key is in, will try to send to the incorrect type
    // between clients and may pass handshake.
    // removed client -- flush values from Replicated<T> after some time.
    // catch all exceptions and route them through the OnError event?
    // todo protobuf support when that is merged.
    // todo think about allowing user to not stomp socket events if they so choose, or to sequence as they see fit.
    // you will need to not pass in the socket in order to do this.
    // todo synced composite object?
    // todo synced list
    // todo potential race when creating and joining a match between the construction of this object
    // and the dispatching of presence objects off the socket.
    // TODO restore the default getvalue call with self
    // ~destructor definitely doesn't work, think about end match flow.
    // fix OnHostValdiate so that you have a better way of signalling intent that you want a var to be validated.
    // to string calls
    // expose interfaces, not concrete classes.
    // todo rename this class?
    // todo handle host changed
    // todo handle guest left
    // todo migrate pending values when host chanes
    // override tostring
    // todo remove any event subscriptions in constructors.
    public class SyncMatch
    {
        private readonly Handshaker _handshaker;
        private readonly SyncSocket _socket;
        private readonly RolePresenceTracker _rolePresenceTracker;
        private readonly PresenceTracker _presenceTracker;

        private readonly SharedVarRegistry _sharedRegistry;
        private readonly UserVarRegistry _userRegistry;

        internal SyncMatch(ISession session, SyncSocket socket, SyncVarRegistry registry, PresenceTracker presenceTracker, RolePresenceTracker rolePresenceTracker)
        {
            _presenceTracker = presenceTracker;
            _rolePresenceTracker = rolePresenceTracker;
            _socket = socket;

            var sharedVars = new SharedVars();
            var userVars = new UserVars();

            var keys = new SyncVarKeys();

            var guestEgress = new GuestEgress(socket, keys);
            var hostEgress = new HostEgress(socket, keys, presenceTracker);

            var egress = new RoleEgress(guestEgress, hostEgress, rolePresenceTracker);
            egress.Subscribe(sharedVars, userVars);

            var guestIngress = new GuestIngress(keys, rolePresenceTracker);
            var hostIngress = new HostIngress(socket, keys);

            var ingress = new RoleIngress(guestIngress, hostIngress, rolePresenceTracker, sharedVars, userVars);
            ingress.Subscribe(socket);

            _sharedRegistry = new SharedVarRegistry(session, sharedVars, keys);
            _sharedRegistry.Register(registry);

            _userRegistry = new UserVarRegistry(session, userVars, keys);
            _userRegistry.Register(registry);

            var hostHandshaker = new HostHandshaker(keys, sharedVars, userVars, presenceTracker);
            hostHandshaker.ListenForHandshakes(socket);

            var guestHandshaker = new GuestHandshaker(keys, ingress, rolePresenceTracker);

            rolePresenceTracker.OnHostChanged += HandleHostChanged;

            _handshaker = new Handshaker(guestHandshaker, hostHandshaker, rolePresenceTracker);
        }

        private void HandleHostChanged(HostChangedEvent evt)
        {
            if (evt.NewHost == _presenceTracker.GetSelf())
            {
                // pick up where the old host left off in terms of validating values.
                new HostMigration().Migrate(_sharedRegistry, _userRegistry);
            }
        }

        public Task Handshake()
        {
            return _handshaker.Handshake(_socket);
        }

    }
}
