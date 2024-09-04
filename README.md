# SICK.RFU610.SDK

This is an SDK to communicate with the SICK RFU610 RFID reader over ethernet.
It simplifies communication with the reader by providing an interface to read tags.
Developers can use this SDK to integrate the reader into their applications.
Features include:
- Connect to the reader over ethernet
- Read tags
- Disconnect from the reader

Eventually, the we would like to support more features such as writing tags, configuring the reader, and more.

## Installation
Installation via Package Manager Console in Visual Studio:

```powershell
PM> Install-Package SickRfid
```

Installation via .NET CLI:

```console
> dotnet add <TARGET PROJECT> package SickRfid
```

## Usage
The SDK is very easy to use. Here is an example of how to read tags:

```csharp
using SickRfid;

public class Program
{
    public async static void Main()
    {
        // Replace the IP address with the IP address of your reader
        var disconnectedReader = new SickRfidControllerBuilder("192.168.1.45").Build();
        
        // Connect to the reader
        var connectedReader = await disconnectedReader.ConnectAsync();
        
        // Read a tag. This starts the scanner, reads a tag within the timeout, and stops the scanner.
        var tagId = await connectedReader.ScanRfidAsync();
        
        // Print the tag ID
        Console.WriteLine($"Tag ID: {tagId}");
    }
}
```

Under the hood, `connectedReader.ScanRfidAsync()` calls three methods itself.
It is possible to call these methods separately if you need more control over the process:

```csharp
using SickRfid;

public class Program
{
    public async static void Main()
    {
        var disconnectedReader = new SickRfidControllerBuilder("192.168.1.45").Build();
        var connectedReader = await disconnectedReader.ConnectAsync();
        
        // Start the scanner
        await connectedReader.StartAsync();
        
        // Read a tag
        await connectedReader.ReadAsync();
        
        // Stop the scanner
        await connectedReader.StopAsync();     
            
        Console.WriteLine($"Tag ID: {tagId}");
    }
}
```
