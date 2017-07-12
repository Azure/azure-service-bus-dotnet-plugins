// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVaultPlugin
{
    using System;

    /// <summary>
    /// The endpoint settings used to create a new <see cref="KeyVaultPlugin"/>.
    /// </summary>
    public class KeyVaultPluginSettings
    {
        /// <summary>
        /// Creates a new <see cref="KeyVaultPluginSettings"/> object and validates the settings.
        /// </summary>
        /// <param name="clientId">The endpoint should be a guid.</param>
        /// <param name="endpoint">The endpoint should resemble: https://{keyvault-name}.vault.azure.net/</param>
        /// <param name="clientSecret">The secret should be a token.</param>
        public KeyVaultPluginSettings(string clientId, string endpoint, string clientSecret)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            this.ClientId = clientId;
            this.Endpoint = endpoint;
            this.ClientSecret = clientSecret;
        }

        /// <summary>
        /// Gets the KeyVault endpoint.
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Gets the KeyVault ClientId.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the KeyVaultClientSecret.
        /// </summary>
        public string ClientSecret { get; }
    }
}
