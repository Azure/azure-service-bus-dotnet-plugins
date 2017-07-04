// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    using System;
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Azure.KeyVault.Models;

    internal class KeyVaultSecretManager : ISecretManager
    {
        private static ConcurrentDictionary<string, byte[]> secretCache;
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

            secretCache = new ConcurrentDictionary<string, byte[]>();
            this.KeyVaultUrl = keyVaultUrl;
            this.azureClientId = azureClientId;
            this.azureClientSecret = azureClientSecret;
        }

        public async Task<byte[]> GetHashedSecret(string secretName, string secretVersion)
        {
            var combinedNameAndVersion = FormatSecretNameAndVersion(secretName, secretVersion);
            if (secretCache.ContainsKey(combinedNameAndVersion))
            {
                return secretCache[combinedNameAndVersion];
            }

            var secret = await GetSecretFromKeyVault(secretName, secretVersion).ConfigureAwait(false);
            using (var sha256 = SHA256.Create())
            {
                var secretAsBytes = Encoding.UTF8.GetBytes(secret);
                var hashedSecret = sha256.ComputeHash(secretAsBytes);
                secretCache.GetOrAdd(combinedNameAndVersion, hashedSecret);
                return hashedSecret;
            }
        }

        internal string FormatSecretNameAndVersion(string secretName, string secretVersion)
        {
            if (string.IsNullOrWhiteSpace(secretVersion))
            {
                return secretName;
            }
            return string.Concat(secretName, "_", secretVersion);
        }

        private async Task<string> GetSecretFromKeyVault(string secretName, string secretVersion)
        {
            using (var keyVaultClient = new KeyVaultClient(GetAccessToken))
            {
                try
                {
                    SecretBundle secretResult;
                    if (string.IsNullOrWhiteSpace(secretVersion))
                    {
                        secretResult = await keyVaultClient.GetSecretAsync(this.KeyVaultUrl, secretName).ConfigureAwait(false);
                    }
                    else
                    {
                        secretResult = await keyVaultClient.GetSecretAsync(this.KeyVaultUrl, secretName, secretVersion).ConfigureAwait(false);
                    }
                    return secretResult.Value;
                }
                catch (Exception ex)
                {
                    throw new KeyVaultPluginException(string.Format(Resources.KeyVaultKeyAcquisitionFailure, secretName, secretVersion, KeyVaultUrl), ex);
                }
            }
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var credential = new ClientCredential(this.azureClientId, this.azureClientSecret);
            var ctx = new AuthenticationContext(new Uri(authority).AbsoluteUri, false);

            try
            {
                var result = await ctx.AcquireTokenAsync(resource, credential).ConfigureAwait(false);
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                throw new KeyVaultPluginException(string.Format(Resources.AzureAdTokenAcquisitionFailure, KeyVaultUrl), ex);
            }
        }
    }
}
