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

using Nakama.TinyJson;

/// <summary>
/// An interface that must be implemented in order for clients to provide their own JSON parser.
/// Please note that if your parser requires member annotations, you must modify the code generator
/// with the new annotation scheme.
/// </summary>
namespace Nakama
{
    public interface IJsonSerializer
    {
        string ToJson(object obj);
        T FromJson<T>(string json);
    }

    public static class JsonSerializer
    {
        private static readonly IJsonSerializer defaultSerializer = new TinyJsonSerializer();
        private static IJsonSerializer assignedSerialier;

        public static IJsonSerializer GetCurrent()
        {
            return assignedSerialier ?? defaultSerializer;
        }

        public static void SetCurrent(IJsonSerializer serializer)
        {
            assignedSerialier = serializer;
        }
    }
}