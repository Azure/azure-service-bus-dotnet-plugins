// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVault
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Core;
    using System.Security.Cryptography;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides Azure KeyVault functionality for Azure Service Bus.
    /// </summary>
    public class KeyVaultPlugin : ServiceBusPlugin
    {
        private readonly string secretName;
        private readonly string keyVaultEndpoint;
        private KeyVaultSecretManager secretManager;

        /// <summary>
        /// Gets the name that is used to identify this plugin.
        /// </summary>
        public override string Name => "KeyVaultPlugin";

        /// <summary>
        /// Creates a new instance of an <see cref="KeyVaultPlugin"/>.
        /// </summary>
        /// <param name="encryptionSecretName">The name of the secret used to encrypt / decrypt messages.</param>
        /// <param name="options">The <see cref="KeyVaultPluginOptions"/> used to create a new instance.</param>
        public KeyVaultPlugin(string encryptionSecretName, KeyVaultPluginOptions options)
        {
            if (string.IsNullOrEmpty(encryptionSecretName))
            {
                throw new ArgumentNullException(nameof(encryptionSecretName));
            }

            if (options != null)
            {
                if (string.IsNullOrEmpty(options.KeyVaultClientId))
                {
                    throw new ArgumentOutOfRangeException(nameof(options.KeyVaultClientId));
                }
                if (string.IsNullOrEmpty(options.KeyVaultEndpoint))
                {
                    throw new ArgumentOutOfRangeException(nameof(options.KeyVaultEndpoint));
                }
                if (string.IsNullOrEmpty(options.KeyVaultClientSecret))
                {
                    throw new ArgumentOutOfRangeException(nameof(options.KeyVaultClientSecret));
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.secretName = encryptionSecretName;
            this.keyVaultEndpoint = options.KeyVaultEndpoint;
            this.secretManager = new KeyVaultSecretManager(options.KeyVaultEndpoint, options.KeyVaultClientId, options.KeyVaultClientSecret);
        }

        /// <summary>
        /// The action performed before sending a message to Service Bus. This method will load the KeyVault key and encrypt messages.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to be encrypted.</param>
        /// <returns>The encrypted <see cref="Message"/>.</returns>
        public override async Task<Message> BeforeMessageSend(Message message)
        {
            try
            {
                if (message.UserProperties.ContainsKey(Constants.InitializationVectorPropertyName) || message.UserProperties.ContainsKey(Constants.KeyNamePropertyName))
                {
                    return message;
                }

                var iV = await KeyVaultPlugin.GenerateInitializationVector();
                var secret = await secretManager.GetHashedSecret(secretName);

                message.UserProperties.Add(Constants.InitializationVectorPropertyName, Convert.ToBase64String(iV));
                message.UserProperties.Add(Constants.KeyNamePropertyName, secretName);

                message.Body = await KeyVaultPlugin.Encrypt(Encoding.UTF8.GetString(message.Body), secret, iV);
                return message;
            }
            catch (Exception ex)
            {
                throw new KeyVaultPluginException(Resources.BeforeMessageSendException, ex);
            }
        }

        /// <summary>
        /// The action performed after receiving a message from Service Bus. This method will load the KeyVault key and decrypt messages.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to be decrypted.</param>
        /// <returns>The decrypted <see cref="Message"/>.</returns>
        public override async Task<Message> AfterMessageReceive(Message message)
        {
            try
            {
                if (!message.UserProperties.ContainsKey(Constants.InitializationVectorPropertyName) || !message.UserProperties.ContainsKey(Constants.KeyNamePropertyName))
                {
                    return message;
                }

                var iVString = message.UserProperties[Constants.InitializationVectorPropertyName] as string;
                var iV = Convert.FromBase64String(iVString);
                var secretName = message.UserProperties[Constants.KeyNamePropertyName] as string;

                var secret = await secretManager.GetHashedSecret(secretName);

                var decryptedMessage = await KeyVaultPlugin.Decrypt(message.Body, secret, iV);

                message.Body = Encoding.UTF8.GetBytes(decryptedMessage);
                return message;
            }
            catch (Exception ex)
            {
                throw new KeyVaultPluginException(Resources.AfterMessageReceiveException, ex);
            }
        }

        internal static Task<byte[]> GenerateInitializationVector()
        {
            byte[] initializationVector = null;
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                initializationVector = aes.IV;
            }
            return Task.FromResult(initializationVector);
        }

        // Taken from the examples here: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes
        internal static Task<byte[]> Encrypt(string payload, byte[] key, byte[] initializationVector)
        {
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = initializationVector;

                // Create an encryptor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(payload);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return Task.FromResult(encrypted);
        }

        // Taken from the examples here: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes
        internal static Task<string> Decrypt(byte[] payload, byte[] key, byte[] initializationVector)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = initializationVector;

                // Create a decrytor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(payload))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    // Read the decrypted bytes from the decrypting stream
                    // and place them in a string.
                    plaintext = srDecrypt.ReadToEnd();
                }
                return Task.FromResult(plaintext);
            }
        }

    }
}
