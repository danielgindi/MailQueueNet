MailQueueNet
================

This is a .NET Core service that queues .NET's `MailMessage` objects to go out in the background, 
so you can prevent blocking your app when sending those out.  
The messages are queued and serialized on disk for resiliency.

The project has been migrated to `.NET Standard` and a `.NET Core 5` service. Which also makes it multiplatform.  
The name has changed to `MailQueueNet`.

## Service Installation:
* Publish `MailQueueNet.Service` from code for your target machine, or take binaries from the `Releases` section.
* Install the service (preferrably) on the same machine. Note that file attachments are only supported on the same machine, as only file paths are serialized and not the data. This may change or be configurable in the future.
* For installing a Windows service, use `sc create MailQueueNet BinPath=C:\full\path\service\MailQueueNet.Service.exe`. (You can use `sc delete MailQueueNet`) to uninstall.
* For installing a Linux service, you can follow the instructions in `MailQueueNet.systemd.service` file that comes with the package.
* Create a folder for queued and failed emails, with sufficient permissions. By default the service will look for "./mail/queue" and "./mail/failed" in the exe's folder.
* Now you can define basic configurations in `appsettings.json`, or do them later from code.
* On Windows, if you get an *0x80131515* error, then go to the properties of the *MailQueueNet.Service.exe*, and click on "Unblock". (Windows Server may recognized that the file was downloaded, and block it automatically).
* On Windows, use `net start MailQueueNet` to start the service, or go to Services in Computer Management.
* On Linux, use `sudo service start MailQueueNet` to start the service.
* You can also run the service as a cli program, without installing as a service.

## Usage as a queuing service:
* Install `MailQueueNet.Common` package from Nuget in your project
* Do this:
```c# 
var mailChannel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions {
    // .NET Core: HttpHandler = new HttpClientHandler { /* allow self-signed certs ServerCertificateCustomValidationCallback = (e, c, ch, errs) => true */ },
    // Windows/.NET 4.7.2: HttpHandler = new WinHttpHandler { /* allow self-signed certs ServerCertificateValidationCallback = (e, c, ch, errs) => true */ },
});
var mailClient = new MailQueueNet.Grpc.MailGrpcService.MailGrpcServiceClient(mailChannel);
```
* Then use `mailClient` to add mails to the queue, or update the settings of the service on the fly.

## Usage as a library:
* It's possible to directly reference `MailQueueNet.Core` and use `SenderFactory` to send out mails without the queuing service, to both SMTP and Mailgun API (or any other service that may be supported in the future).

## Me
* Hi! I am Daniel.
* danielgindi@gmail.com is my email address.
* That's all you need to know.

## License

All the code here is under MIT license. Which means you could do virtually anything with the code.
I will appreciate it very much if you keep an attribution where appropriate.

    The MIT License (MIT)
    
    Copyright (c) 2013 Daniel Cohen Gindi (danielgindi@gmail.com)
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
