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
        [DisplayTestMethodName]
        public async Task SmokeTest()
        {
            var payload = Encoding.UTF8.GetBytes("hello");
            var password = "password";

            byte[] hash = null;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }            

            var iV = KeyVaultPlugin.GenerateInitializationVector();
            var encryptedPayload = await KeyVaultPlugin.Encrypt(payload, hash, iV);
            var decryptedPayload = await KeyVaultPlugin.Decrypt(encryptedPayload, hash, iV);

            Assert.Equal(payload, decryptedPayload);
        }

        [Fact]
        [DisplayTestMethodName]
        public async Task MockSecretManager()
        {
            var secretManager = new MockSecretManager();

            var keyVaultPlugin = new KeyVaultPlugin("service-bus", secretManager);

            var messageBody = Encoding.UTF8.GetBytes("hi");

            var message = new Message(messageBody);

            var encryptedMessage = await keyVaultPlugin.BeforeMessageSend(message);
            var decryptedMessage = await keyVaultPlugin.AfterMessageReceive(encryptedMessage);
            Assert.Equal(messageBody, decryptedMessage.Body);
        }
    }
}
