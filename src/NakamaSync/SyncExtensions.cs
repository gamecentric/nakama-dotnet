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

using System.Threading.Tasks;
using Nakama;

// todo entire concurrency pass on all this
// enforce key ids should be unique across types
// TODO what if someone changes the type collection that the key is in, will try to send to the incorrect type
// between clients and may pass handshake.
// if user leaves and then rejoins do their values come back? they are still in collection but
// are they received by that user on initial sync? I think so.
// catch all exceptions and route them through the OnError event?
// todo protobuf support when that is merged.
// todo think about fact that user can still attach to presence handler without being able to sequence presence events as they see fit.
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
// todo parameter ordering
// todo internalize the registry so that users can't change it from the outside?
// otherwise find some other way to prevent users from messing with it.
// todo error handling checks particularly on dictionary accessing etc.
// todo clean this class up and use explicit returns values where needed for some of these private methods
// should user vars have acks on a uesr by user basis?
namespace NakamaSync
{
    public static class SyncExtensions
    {
        // todo don't require session as a parameter here since we pass it to socket.

        // todo put reflection version here?
        public static async Task<IMatch> CreateSyncMatch(this ISocket socket, ISession session, SyncOpcodes opcodes, VarRegistry registry)
        {
            var keys = new VarKeys();
            var presenceTracker = new RolePresenceTracker(session);
            var syncSocket = new SyncSocket(socket, opcodes, presenceTracker);
            var builder = new EnvelopeBuilder(syncSocket);
            var ingresses = new Ingresses(keys, registry, builder, presenceTracker);
            var guestHandshaker = new GuestHandshaker(keys, ingresses, presenceTracker, syncSocket);
            var hostHandshaker = new HostHandshaker(keys, registry, presenceTracker);
            var handshaker = new Handshaker(guestHandshaker, hostHandshaker, presenceTracker);
            var guestEgress = new GuestEgress(keys, builder);
            var hostEgress = new HostEgress(keys, builder, presenceTracker);
            var egress = new RoleEgress(guestEgress, hostEgress, presenceTracker);

            var migrater = new HostMigrater(registry, builder);

            presenceTracker.Subscribe(socket);
            migrater.Subscribe(presenceTracker);
            ingresses.SharedRoleIngress.Subscribe(syncSocket, presenceTracker);
            ingresses.UserRoleIngress.Subscribe(syncSocket, presenceTracker);
            egress.Subscribe(registry);
            handshaker.Subscribe(syncSocket);

            IMatch match = await socket.CreateMatchAsync();

            registry.ReceiveMatch(keys, match);
            syncSocket.ReceiveMatch(match);
            presenceTracker.ReceiveMatch(match);

            await handshaker.WaitForHandshake();

            return match;
        }

        public static async Task<IMatch> JoinSyncMatch(this ISocket socket, ISession session, SyncOpcodes opcodes, string matchId, VarRegistry registry)
        {
            var keys = new VarKeys();
            var presenceTracker = new RolePresenceTracker(session);
            var syncSocket = new SyncSocket(socket, opcodes, presenceTracker);
            var builder = new EnvelopeBuilder(syncSocket);
            var ingresses = new Ingresses(keys, registry, builder, presenceTracker);
            var guestHandshaker = new GuestHandshaker(keys, ingresses, presenceTracker, syncSocket);
            var hostHandshaker = new HostHandshaker(keys, registry, presenceTracker);
            var handshaker = new Handshaker(guestHandshaker, hostHandshaker, presenceTracker);
            var guestEgress = new GuestEgress(keys, builder);
            var hostEgress = new HostEgress(keys, builder, presenceTracker);
            var egress = new RoleEgress(guestEgress, hostEgress, presenceTracker);
            var migrater = new HostMigrater(registry, builder);

            presenceTracker.Subscribe(socket);
            migrater.Subscribe(presenceTracker);
            ingresses.SharedRoleIngress.Subscribe(syncSocket, presenceTracker);
            ingresses.UserRoleIngress.Subscribe(syncSocket, presenceTracker);
            egress.Subscribe(registry);
            handshaker.Subscribe(syncSocket);

            IMatch match = await socket.JoinMatchAsync(matchId);

            registry.ReceiveMatch(keys, match);
            syncSocket.ReceiveMatch(match);
            presenceTracker.ReceiveMatch(match);

            await handshaker.WaitForHandshake();

            return match;
        }
    }
}