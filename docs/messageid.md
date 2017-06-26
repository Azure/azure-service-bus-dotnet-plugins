# Message ID plugin for Azure Service Bus

The Message ID plugin for Azure Service Bus allows for the message ID on outgoing messages to be set using custom logic.

## How to use

In order to use this plugin you will need to setup the following:

1. An Azure subscription
1. A Service Bus namespace

## Example

Below is a simple example of how to use the plugin.

```csharp
var messageIdPlugin = new MessageIdPlugin((msg) => Guid.NewGuid().ToString("N"));

var queueClient = new QueueClient("{ServiceBusConnectionString}", "{ServiceBusEntityName}");
queueClient.RegisterPlugin(messageIdPlugin);

var message = new Message(Encoding.UTF8.GetBytes("Message with GUID message ID"));

await queueClient.SendAsync(message);

// message.MessageId will be assigned a GUID in a 32 digit format w/o hyphens or braces
```
