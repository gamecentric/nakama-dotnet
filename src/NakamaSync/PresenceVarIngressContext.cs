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

using System.Collections.Generic;

namespace NakamaSync
{
    internal class PresenceVarIngressContext<T>
    {
        public PresenceVar<T> Var { get; }
        public PresenceValue<T> Value { get; }
        public PresenceVarAccessor<T> VarAccessor { get; }
        public AckAccessor AckAccessor { get; }

        public PresenceVarIngressContext(PresenceVar<T> var, PresenceValue<T> value, PresenceVarAccessor<T> accessor, AckAccessor ackAccessor)
        {
            Var = var;
            Value = value;
            VarAccessor = accessor;
            AckAccessor = ackAccessor;
        }
    }

    internal class UserIngressContext
    {
        public static List<PresenceVarIngressContext<bool>> FromBoolValues(Envelope envelope, VarRegistry registry)
        {
            return UserIngressContext.FromValues<bool>(envelope.PresenceBools, registry.PresenceBools, env => env.PresenceBools, env => env.PresenceBoolAcks);
        }

        public static List<PresenceVarIngressContext<float>> FromFloatValues(Envelope envelope, VarRegistry registry)
        {
            return UserIngressContext.FromValues<float>(envelope.PresenceFloats, registry.PresenceFloats, env => env.PresenceFloats, env => env.PresenceFloatAcks);
        }

        public static List<PresenceVarIngressContext<int>> FromIntValues(Envelope envelope, VarRegistry registry)
        {
            return UserIngressContext.FromValues<int>(envelope.PresenceInts, registry.PresenceInts, env => env.PresenceInts, env => env.PresenceIntAcks);
        }

        public static List<PresenceVarIngressContext<string>> FromStringValues(Envelope envelope, VarRegistry registry)
        {
            return UserIngressContext.FromValues<string>(envelope.PResenceStrings, registry.PresenceStrings, env => env.PResenceStrings, env => env.PresenceStringAcks);
        }

        private static List<PresenceVarIngressContext<T>> FromValues<T>(List<PresenceValue<T>> values, Dictionary<string, PresenceVar<T>> vars, PresenceVarAccessor<T> varAccessor, AckAccessor ackAccessor)
        {
            var contexts = new List<PresenceVarIngressContext<T>>();

            foreach (PresenceValue<T> value in values)
            {
                var context = new PresenceVarIngressContext<T>(vars[value.Key], value, varAccessor, ackAccessor);
                contexts.Add(context);
            }

            return contexts;
        }
    }
}