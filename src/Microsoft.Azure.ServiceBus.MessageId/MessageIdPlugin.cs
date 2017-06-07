// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.MessageId
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus.Core;

    /// <summary>Generate Azure Service Bus <see cref="Message"/> <see cref="Message.MessageId"/> for outgoing messages.
    /// <example>
    /// var plugin = new MessageIdPlugin(() =&gt; Guid.NewGuid().ToString(N));
    /// queueClient.RegisterPlugin(plugin);
    /// </example>
    /// </summary>
    /// <remarks>If a message ID is assigned, the value will not be replaced by the plugin.</remarks>
    public class MessageIdPlugin : ServiceBusPlugin
    {
        private Func<string> messageIdGenerator;

        /// <summary>
        /// <inheritdoc cref="Name"/>
        /// </summary>
        public override string Name => "Microsoft.Azure.ServiceBus.MessageId";

        /// <summary>
        /// Create a new instance of <see cref="MessageIdPlugin"/>
        /// </summary>
        /// <param name="messageIdGenerator">Message ID generator to use.</param>
        public MessageIdPlugin(Func<string> messageIdGenerator)
        {
            this.messageIdGenerator = messageIdGenerator;
        }

        /// <summary>
        /// Assign message id if it's not already assigned.
        /// <param name="message">The <see cref="Message"/> to assign id to.</param>
        /// </summary>
        /// <returns><see cref="Message"/> with id.</returns>
        public override Task<Message> BeforeMessageSend(Message message)
        {
            if (!string.IsNullOrEmpty(message.MessageId))
            {
                return base.BeforeMessageSend(message);
            }

            message.MessageId = messageIdGenerator();

            return Task.FromResult(message);
        }
    }
}
