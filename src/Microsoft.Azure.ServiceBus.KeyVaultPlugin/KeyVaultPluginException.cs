// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Plugins
{
    using System;

    /// <summary>
    /// Represents errors that occur within the <see cref="KeyVaultPlugin"/>.
    /// </summary>
    public class KeyVaultPluginException : Exception
    {
        internal KeyVaultPluginException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
