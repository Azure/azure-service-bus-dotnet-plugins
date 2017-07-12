// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault.Test
{
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Plugins;

    internal class MockSecretManager : ISecretManager
    {
        private Dictionary<string, byte[]> secretCache = new Dictionary<string, byte[]>();

        public Task<byte[]> GetHashedSecret(string secretName, string secretVersion)
        {
            var combinedNameAndVersion = FormatSecretNameAndVersion(secretName, secretVersion);
            if (secretCache.ContainsKey(combinedNameAndVersion))
            {
                return Task.FromResult(secretCache[combinedNameAndVersion]);
            }

            var secret = GenerateSecret();

            using (var sha256 = SHA256.Create())
            {
                var secretAsBytes = Encoding.UTF8.GetBytes(secret);
                var hashedSecret = sha256.ComputeHash(secretAsBytes);
                secretCache.Add(combinedNameAndVersion, hashedSecret);
                return Task.FromResult(hashedSecret);
            }
        }

        private string FormatSecretNameAndVersion(string secretName, string secretVersion)
        {
            if (string.IsNullOrWhiteSpace(secretVersion))
            {
                return secretName;
            }
            return string.Concat(secretName, "_", secretVersion);
        }

        private string GenerateSecret()
        {
            return "password";
        }
    }
}
