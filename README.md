dg.MailQueue.net
================

This is a .NET service that queues .NET's `MailMessage` objects to go out in the background, 
so you can prevent blocking your app when sending those out.

## Installation:
* Copy the files from dg.MailQueue.net/Release folder, to where you want your service to be.
* Create a folder for queued and failed emails, with sufficient permissions. By default the service will look for "./mail/queue" and "./mail/failed" in the exe's folder.
* Now you can define basic configurations in dg.MailQueue.net.exe.config, or do them later from code.
* Use `installutil.exe dg.MailQueue.net.exe` to install the service. `installutil` is in .NET's folder (i.e. %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319
* If you get an *0x80131515* error, then go to the properties of the *dg.MailQueue.net.exe*, and click on "Unblock". (Windows Server may recognized that the file was downloaded, and block it automatically).
* Or if you want, you can run it from the command line (specifying any argument, will cause it to run as a console app)
* Use `net start dg.MailQueue.net` to start the service, or go to Services in Computer Management.

## Usage:
* Reference *dg.MailQueue.Contracts.net.dll* in your project.
* Use `ISettingsContract settingsChannel = PipeFactory.NewSettingsChannel()` or `IMailContract mailChannel = PipeFactory.NewMailChannel()` to create a channel the service.
* Then use `settingsChannel` to setup any of the settings of the service,
* And `mailChannel` to add mails to the queue.

## Me
* Hi! I am Daniel Cohen Gindi. Or in short- Daniel.
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
