// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    internal static class KeyVaultMessageHeaders
    {
        internal const string InitializationVectorPropertyName = "KeyVault-IV";
        internal const string KeyNamePropertyName = "KeyVault-KeyName";
        internal const string KeyVersionPropertyName = "KeyVault-KeyVersion";
    }
}
