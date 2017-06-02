// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    /// <summary>
    /// The endpoint options used to create a new <see cref="KeyVaultPlugin"/>.
    /// </summary>
    public class KeyVaultPluginOptions
    {
        /// <summary>
        /// Gets or sets the KeyVault endpoint.
        /// </summary>
        /// <remarks>The endpoint should resemble: https://{keyvault-name}.vault.azure.net/</remarks>
        public string KeyVaultEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the KeyVaultClientId.
        /// </summary>
        /// <remarks>The endpoint should be a guid.</remarks>
        public string KeyVaultClientId { get; set; }

        /// <summary>
        /// Gets or sets the KeyVaultClientSecret.
        /// </summary>
        public string KeyVaultClientSecret { get; set; }
    }
}
