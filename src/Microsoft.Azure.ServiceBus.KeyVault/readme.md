# Key Vault plugin for Azure Service Bus

The Key Vault plugin for Azure Service Bus allows for the message body to be encrypted and decrypted by registering a plugin and its accompanying settings.

## How to use

In order to use this plugin you will need to setup the following:

1. An Azure subscription

1. A Service Bus namespace

1. [A Key Vault instance](https://docs.microsoft.com/azure/key-vault/key-vault-get-started)

1. [An Azure Active Directory application](https://docs.microsoft.com/azure/key-vault/key-vault-get-started#a-idregisteraregister-an-application-with-azure-active-directory)

## Example

Below is a simple example of how to use the plugin.

```csharp
var keyVaultSettings = new KeyVaultPluginSettings("{AADClientID}", "{KeyVaultEndpoint}", "{AADSecret}");
var keyVaultPlugin = new KeyVaultPlugin("{KeyVaultSecretName}", "{KeyVaultSecretVersion}", keyVaultSettings);

var queueClient = new QueueClient("{ServiceBusConnectionString}", "{ServiceBusEntityName}");
queueClient.RegisterPlugin(keyVaultPlugin);

var message = new Message(Encoding.UTF8.GetBytes("Super secret message"));

await queueClient.SendAsync(message).ConfigureAwait(false);
```