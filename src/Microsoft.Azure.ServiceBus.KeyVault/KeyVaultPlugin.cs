// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.KeyVaultPlugin
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Core;

    /// <summary>
    /// Provides Azure KeyVault functionality for Azure Service Bus.
    /// </summary>
    public class KeyVaultPlugin : ServiceBusPlugin
    {
        private readonly string secretName;
        private readonly string secretVersion;
        private ISecretManager secretManager;
        private byte[] initializationVector;
        private string base64InitializationVector;

        /// <summary>
        /// Gets the name that is used to identify this plugin.
        /// </summary>
        public override string Name => "Microsoft.Azure.ServiceBus.KeyVault.KeyVaultPlugin";

        /// <summary>
        /// Creates a new instance of an <see cref="KeyVaultPlugin"/>.
        /// </summary>
        /// <param name="encryptionSecretName">The name of the secret used to encrypt / decrypt messages.</param>
        /// <param name="keyVaultSettings">The <see cref="KeyVaultPluginSettings"/> used to create a new instance.</param>
        public KeyVaultPlugin(string encryptionSecretName, KeyVaultPluginSettings keyVaultSettings)
            : this(encryptionSecretName, string.Empty, keyVaultSettings)
        {
        }

        /// <summary>
        /// Creates a new instance of an <see cref="KeyVaultPlugin"/>.
        /// </summary>
        /// <param name="encryptionSecretName">The name of the secret used to encrypt / decrypt messages.</param>
        /// <param name="encryptionSecretVersion">The version of the secret</param>
        /// <param name="keyVaultSettings">The <see cref="KeyVaultPluginSettings"/> used to create a new instance.</param>
        public KeyVaultPlugin(string encryptionSecretName, string encryptionSecretVersion, KeyVaultPluginSettings keyVaultSettings)
        {
            if (string.IsNullOrEmpty(encryptionSecretName))
            {
                throw new ArgumentNullException(nameof(encryptionSecretName));
            }
            if (keyVaultSettings == null)
            {
                throw new ArgumentNullException(nameof(keyVaultSettings));
            }

            this.secretName = encryptionSecretName;
            this.secretVersion = encryptionSecretVersion;
            this.secretManager = new KeyVaultSecretManager(keyVaultSettings.Endpoint, keyVaultSettings.ClientId, keyVaultSettings.ClientSecret);
            this.initializationVector = KeyVaultPlugin.GenerateInitializationVector();
            this.base64InitializationVector = Convert.ToBase64String(this.initializationVector);
        }

        internal KeyVaultPlugin(string encryptionSecretName, ISecretManager secretManager)
        {
            this.secretName = encryptionSecretName;
            this.secretManager = secretManager;
            this.initializationVector = KeyVaultPlugin.GenerateInitializationVector();
            this.base64InitializationVector = Convert.ToBase64String(this.initializationVector);
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
                // Skip encryption if message properties are already set
                if (message.UserProperties.ContainsKey(KeyVaultMessageHeaders.InitializationVectorPropertyName)
                    || message.UserProperties.ContainsKey(KeyVaultMessageHeaders.KeyNamePropertyName)
                    || message.UserProperties.ContainsKey(KeyVaultMessageHeaders.KeyVersionPropertyName))
                {
                    return message;
                }

                var secret = await secretManager.GetHashedSecret(secretName, secretVersion).ConfigureAwait(false);

                message.UserProperties.Add(KeyVaultMessageHeaders.InitializationVectorPropertyName, base64InitializationVector);
                message.UserProperties.Add(KeyVaultMessageHeaders.KeyNamePropertyName, secretName);
                message.UserProperties.Add(KeyVaultMessageHeaders.KeyVersionPropertyName, secretVersion);

                message.Body = await KeyVaultPlugin.Encrypt(message.Body, secret, this.initializationVector).ConfigureAwait(false);
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
                // Skip decryption we are missing properties for the key and IV.
                if (!message.UserProperties.ContainsKey(KeyVaultMessageHeaders.InitializationVectorPropertyName)
                    || !message.UserProperties.ContainsKey(KeyVaultMessageHeaders.KeyNamePropertyName))
                {
                    return message;
                }

                var iVString = message.UserProperties[KeyVaultMessageHeaders.InitializationVectorPropertyName] as string;
                var iV = Convert.FromBase64String(iVString);
                var secretName = message.UserProperties[KeyVaultMessageHeaders.KeyNamePropertyName] as string;

                // Remove properties before giving the message back
                message.UserProperties.Remove(KeyVaultMessageHeaders.InitializationVectorPropertyName);
                message.UserProperties.Remove(KeyVaultMessageHeaders.KeyNamePropertyName);

                string secretVersion = string.Empty;
                if (message.UserProperties.ContainsKey(KeyVaultMessageHeaders.KeyVersionPropertyName))
                {
                    secretVersion = message.UserProperties[KeyVaultMessageHeaders.KeyVersionPropertyName] as string;
                    message.UserProperties.Remove(KeyVaultMessageHeaders.KeyVersionPropertyName);
                }

                var secret = await secretManager.GetHashedSecret(secretName, secretVersion).ConfigureAwait(false);

                var decryptedMessage = await KeyVaultPlugin.Decrypt(message.Body, secret, iV).ConfigureAwait(false);

                message.Body = decryptedMessage;
                return message;
            }
            catch (Exception ex)
            {
                throw new KeyVaultPluginException(Resources.AfterMessageReceiveException, ex);
            }
        }

        internal static byte[] GenerateInitializationVector()
        {
            byte[] initializationVector = null;
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                initializationVector = aes.IV;
            }
            return initializationVector;
        }

        // Taken from the examples here: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes
        internal static async Task<byte[]> Encrypt(byte[] payload, byte[] key, byte[] initializationVector)
        {
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = initializationVector;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                return await PerformCryptography(encryptor, payload).ConfigureAwait(false);
            }
        }

        // Taken from the examples here: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aes
        internal static async Task<byte[]> Decrypt(byte[] payload, byte[] key, byte[] initializationVector)
        {
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = initializationVector;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                return await PerformCryptography(decryptor, payload).ConfigureAwait(false);
            }
        }

        private static async Task<byte[]> PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            // Create the streams used for encryption.
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                // Write all data to the memory stream.
                await cryptoStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }
    }
}
