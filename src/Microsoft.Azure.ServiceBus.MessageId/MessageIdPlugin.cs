using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;

namespace Microsoft.Azure.ServiceBus.MessageId
{
    /// <summary>Generate Azure Service Bus <see cref="Message"/> id.</summary>
    public class MessageIdPlugin : ServiceBusPlugin
    {
        private readonly Func<string> messageIdGenerator;

        /// <summary>
        /// <inheritdoc cref="Name"/>
        /// </summary>
        public override string Name => "Microsoft.Azure.ServiceBus.MessageId";

        /// <summary>
        /// Create a new instance of <see cref="MessageIdPlugin"/>
        /// </summary>
        /// <param name="messageIdGenerator"></param>
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
