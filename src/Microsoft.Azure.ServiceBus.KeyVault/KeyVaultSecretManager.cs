// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Text;

    internal class KeyVaultSecretManager
    {
        private static Dictionary<string, byte[]> secretCache;
        private string azureClientId;
        private string azureClientSecret;

        internal string KeyVaultUrl { get; private set; }

        internal KeyVaultSecretManager(string keyVaultUrl, string azureClientId, string azureClientSecret)
        {
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl));
            }

            if (string.IsNullOrEmpty(azureClientId))
            {
                throw new ArgumentNullException(nameof(azureClientId));
            }

            if (string.IsNullOrEmpty(azureClientSecret))
            {
                throw new ArgumentNullException(nameof(azureClientSecret));
            }

            secretCache = new Dictionary<string, byte[]>();
            this.KeyVaultUrl = keyVaultUrl;
            this.azureClientId = azureClientId;
            this.azureClientSecret = azureClientSecret;
        }

        internal async Task<byte[]> GetHashedSecret(string secretName)
        {
            if (secretCache.ContainsKey(secretName))
            {
                return secretCache[secretName];
            }

            var secret = await GetSecretFromKeyVault(secretName);
            using (var sha256 = SHA256.Create())
            {
                var secretAsBytes = Encoding.UTF8.GetBytes(secret);
                var hashedSecret = sha256.ComputeHash(secretAsBytes);
                secretCache.Add(secretName, hashedSecret);
                return hashedSecret;
            }
        }

        private async Task<string> GetSecretFromKeyVault(string secretName)
        {
            using (var keyVaultClient = new KeyVaultClient(GetAccessToken))
            {
                string secret;
                try
                {
                    var secretResult = await keyVaultClient.GetSecretAsync(this.KeyVaultUrl, secretName);
                    secret = secretResult.Value;
                }
                catch (Exception ex)
                {
                    throw new KeyVaultPluginException(string.Format(Resources.KeyVaultKeyAcquisitionFailure, secretName, KeyVaultUrl), ex);
                }

                return secret;
            }

            // ToDo: Add support for KeyVault service side encryption/decryption

            //var encryptedMessage = await keyVaultClient.EncryptAsync(
            //    KeyVaultUrl,
            //    SecretName,
            //    "secretVersion",
            //    JsonWebKeyEncryptionAlgorithm.RSA15,
            //    message);
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var credential = new ClientCredential(this.azureClientId, this.azureClientSecret);

            var ctx = new AuthenticationContext(new Uri(authority).AbsoluteUri, false);

            AuthenticationResult result;
            try
            {
                result = await ctx.AcquireTokenAsync(resource, credential);
            }
            catch (Exception ex)
            {
                throw new KeyVaultPluginException(string.Format(Resources.AzureAdTokenAcquisitionFailure, KeyVaultUrl), ex);
            }
            return result.AccessToken;
        }
    }
}
