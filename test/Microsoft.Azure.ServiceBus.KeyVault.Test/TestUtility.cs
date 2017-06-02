// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Core;

    static class TestUtility
    {
        static TestUtility()
        {
            var envConnectionString = Environment.GetEnvironmentVariable(TestConstants.ConnectionStringEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(envConnectionString))
            {
                throw new InvalidOperationException($"'{TestConstants.ConnectionStringEnvironmentVariable}' environment variable was not found!");
            }

            // Validate the connection string
            NamespaceConnectionString = new ServiceBusConnectionStringBuilder(envConnectionString).ToString();
        }

        internal static string NamespaceConnectionString { get; }

        internal static void Log(string message)
        {
            var formattedMessage = $"{DateTime.Now.TimeOfDay}: {message}";
            Debug.WriteLine(formattedMessage);
            Console.WriteLine(formattedMessage);
        }
    }
}