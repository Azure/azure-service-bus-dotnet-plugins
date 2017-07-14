<p align="center">
  <img src="service-bus.png" alt="Microsoft Azure Service Bus" width="100"/>
</p>

# Plugins for the Microsoft Azure Service Bus .NET Standard client

|Build/Package|Status|
|------|-------------|
|Microsoft.Azure.ServiceBus.MessageIdPlugin|[![NuGet Version and Downloads count](https://buildstats.info/nuget/Microsoft.Azure.ServiceBus.MessageIdPlugin?includePreReleases=true)](https://www.nuget.org/packages/Microsoft.Azure.ServiceBus.MessageIdPlugin/)|
|Microsoft.Azure.ServiceBus.KeyVaultPlugin|[![NuGet Version and Downloads count](https://buildstats.info/nuget/Microsoft.Azure.ServiceBus.KeyVaultPlugin?includePreReleases=true)](https://www.nuget.org/packages/Microsoft.Azure.ServiceBus.KeyVaultPlugin/)|

This repository is for plugins for the [.NET Standard Azure Service Bus library](https://github.com/azure/azure-service-bus-dotnet) owned by the Azure-Service-Bus team.

Azure Service Bus is an asynchronous messaging cloud platform that enables you to send messages between decoupled systems. Microsoft offers this feature as a service, which means that you do not need to host any of your own hardware in order to use it.

Refer to the [online documentation](https://azure.microsoft.com/services/service-bus/) to learn more about Service Bus.

This library is built using .NET Standard 1.3. For more information on what platforms are supported see [.NET Platforms Support](https://docs.microsoft.com/en-us/dotnet/articles/standard/library#net-platforms-support).

## Plugins included with this repository

* [Key Vault](./src/Microsoft.Azure.ServiceBus.KeyVaultPlugin/readme.md)
* [Message ID](./src/Microsoft.Azure.ServiceBus.MessageIdPlugin/readme.md)

## Third party provided plugins

Run by community members and as such are not covered under the Microsoft License. The applicable license for each third party plugin is listed by each third party plugin provider. 

* [Claim Check pattern](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/)

To link your plugin to this readme, contact Azure-Service-Bus team by opening an issue in this repository. The requests will be looked on a case-by-case basis.

## How to provide feedback

See our [Contribution Guidelines](./.github/CONTRIBUTING.md).

