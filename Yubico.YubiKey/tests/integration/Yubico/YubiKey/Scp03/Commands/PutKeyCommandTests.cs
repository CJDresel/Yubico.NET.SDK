// Copyright 2023 Yubico AB
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Xunit;
using Yubico.YubiKey.TestUtilities;

namespace Yubico.YubiKey.Scp03.Commands
{
    public class PutKeyCommandTests
    {
        [Fact]
        public void ChangeDefaultKey_Succeeds()
        {
            byte[] key1 = {
                0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff
            };
            byte[] key2 = {
                0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x11
            };
            byte[] key3 = {
                0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x11, 0x22
            };
            byte[] applicationId = {
                0xA0, 0x00, 0x00, 0x01, 0x51, 0x00, 0x00, 0x00
            };

            var currentKeys = new StaticKeys();
            var newKeys = new StaticKeys(key2, key1, key3);

            IYubiKeyDevice testDevice = IntegrationTestDeviceEnumeration.GetScp03TestDevice(currentKeys);

            bool isValid = testDevice.TryConnect(applicationId, out IYubiKeyConnection? connection);
            Assert.True(isValid);
            Assert.NotNull(connection);

            var cmd = new PutKeyCommand(currentKeys, newKeys);
            PutKeyResponse rsp = connection!.SendCommand(cmd);
            Assert.Equal(ResponseStatus.Success, rsp.Status);
            ReadOnlyMemory<byte> checksum = rsp.GetData();
            bool isEqual = checksum.Span.SequenceEqual(cmd.ExpectedChecksum.Span);
            Assert.True(isEqual);
        }

        [Fact]
        public void AddNewKeySet_Succeeds()
        {
            byte[] key1 = {
                0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff
            };
            byte[] key2 = {
                0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x11
            };
            byte[] key3 = {
                0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x11, 0x22
            };
            byte[] newKey1 = {
                0xff, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee,
            };
            byte[] newKey2 = {
                0xee, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xff, 0x11
            };
            byte[] newKey3 = {
                0xdd, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xee, 0xff, 0x11, 0x22
            };
            byte[] applicationId = {
                0xA0, 0x00, 0x00, 0x01, 0x51, 0x00, 0x00, 0x00
            };

            var currentKeys = new StaticKeys(key2, key1, key3);
            var newKeys = new StaticKeys(newKey2, newKey1, newKey3)
            {
                KeyVersionNumber = 2
            };

            IYubiKeyDevice testDevice = IntegrationTestDeviceEnumeration.GetScp03TestDevice(currentKeys);

            bool isValid = testDevice.TryConnect(applicationId, out IYubiKeyConnection? connection);
            Assert.True(isValid);
            Assert.NotNull(connection);

            var cmd = new PutKeyCommand(currentKeys, newKeys);
            PutKeyResponse rsp = connection!.SendCommand(cmd);
            Assert.Equal(ResponseStatus.Success, rsp.Status);
            ReadOnlyMemory<byte> checksum = rsp.GetData();
            bool isEqual = checksum.Span.SequenceEqual(cmd.ExpectedChecksum.Span);
            Assert.True(isEqual);
        }

        [Fact]
        public void AddThirdKeySet_Succeeds()
        {
            byte[] key1 = {
                0xff, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xee,
            };
            byte[] key2 = {
                0xee, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xff, 0x11
            };
            byte[] key3 = {
                0xdd, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xee, 0xff, 0x11, 0x22
            };
            byte[] newKey1 = {
                0x33, 0xff, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd,
            };
            byte[] newKey2 = {
                0x33, 0xee, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0xff,
            };
            byte[] newKey3 = {
                0x33, 0xdd, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00, 0xaa, 0xbb, 0xcc, 0xee, 0xff, 0x11,
            };
            byte[] applicationId = {
                0xA0, 0x00, 0x00, 0x01, 0x51, 0x00, 0x00, 0x00
            };

            var currentKeys = new StaticKeys(key2, key1, key3)
            {
                KeyVersionNumber = 2
            };
            var newKeys = new StaticKeys(newKey2, newKey1, newKey3)
            {
                KeyVersionNumber = 3
            };

            IYubiKeyDevice testDevice = IntegrationTestDeviceEnumeration.GetScp03TestDevice(currentKeys);

            bool isValid = testDevice.TryConnect(applicationId, out IYubiKeyConnection? connection);
            Assert.True(isValid);
            Assert.NotNull(connection);

            var cmd = new PutKeyCommand(currentKeys, newKeys);
            PutKeyResponse rsp = connection!.SendCommand(cmd);
            Assert.Equal(ResponseStatus.Success, rsp.Status);
            ReadOnlyMemory<byte> checksum = rsp.GetData();
            bool isEqual = checksum.Span.SequenceEqual(cmd.ExpectedChecksum.Span);
            Assert.True(isEqual);
        }
    }
}
