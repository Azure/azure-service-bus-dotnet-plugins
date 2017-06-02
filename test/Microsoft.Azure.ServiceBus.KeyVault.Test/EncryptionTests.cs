// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault.Test
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.KeyVault;
    using Xunit;

    public class EncryptionTests
    {
        [Fact]
        public async Task Encryption_smoke_test()
        {
            var payload = Encoding.UTF8.GetBytes("hello");
            var password = "password";

            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            var iV = KeyVaultPlugin.GenerateInitializationVector();

            var encryptedPayload = await KeyVaultPlugin.Encrypt(payload, hash, iV);

            var decryptedPayload = await KeyVaultPlugin.Decrypt(encryptedPayload, hash, iV);

            Assert.Equal(payload, decryptedPayload);
        }
    }
}
