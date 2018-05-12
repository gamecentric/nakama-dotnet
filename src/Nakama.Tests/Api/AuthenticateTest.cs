﻿/**
 * Copyright 2018 The Nakama Authors
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

// ReSharper disable RedundantArgumentDefaultValue
namespace Nakama.Tests.Api
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class AuthenticateTest
    {
        [Test]
        public async Task ShouldAuthenticateWithCustomId()
        {
            var customid = Guid.NewGuid();
            IClient client = new Client("defaultkey", "127.0.0.1", 7350, false);
            var session = await client.AuthenticateCustomAsync(customid.ToString());

            Console.WriteLine(session);
            Assert.IsNotNull(session);
            Assert.IsNotNull(session.UserId);
            Assert.IsNotNull(session.Username);
        }
    }
}