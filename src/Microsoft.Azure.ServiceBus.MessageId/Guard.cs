// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.MessageId
{
    using System;

    internal static class Guard
    {
        public static void AgainstEmpty(string argumentName, string value)
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
        public static void AgainstNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}