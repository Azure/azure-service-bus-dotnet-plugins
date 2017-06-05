// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.MessageId.Test
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;
    using Microsoft.Azure.ServiceBus.Test.Shared;

    public class When_MessageIdPlugin_is_used
    {
        [Fact]
        [DisplayTestMethodName]
        public async Task Should_assign_message_id()
        {
            var message = new Message();
            var generatedMessageId = Guid.Empty;
            var plugin = new MessageIdPlugin(() =>
            {
                generatedMessageId = Guid.NewGuid();
                return generatedMessageId.ToString("N");
            });

            var result = await plugin.BeforeMessageSend(message);

            Assert.Equal(generatedMessageId, Guid.Parse(result.MessageId));
        }

        [Fact]
        [DisplayTestMethodName]
        public async Task Should_not_assign_message_id_if_it_already_exists()
        {
            var originalMessageId = Guid.NewGuid().ToString();
            var message = new Message
            {
                MessageId = originalMessageId
            };
            var plugin = new MessageIdPlugin(() => "this id should never be assigned");

            var result = await plugin.BeforeMessageSend(message);

            Assert.Equal(originalMessageId, result.MessageId);
        }

        [Fact]
        [DisplayTestMethodName]
        public void Should_return_correct_plugin_name()
        {
            var plugin = new MessageIdPlugin(() => "");

            Assert.Equal("Microsoft.Azure.ServiceBus.MessageId", plugin.Name);
        }

    }
}
