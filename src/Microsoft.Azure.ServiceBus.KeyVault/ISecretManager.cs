// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    using System.Threading.Tasks;

    internal interface ISecretManager
    {
        Task<byte[]> GetHashedSecret(string secretName, string secretVersion);
    }
}