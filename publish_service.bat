dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=Windows.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=WindowsSingle.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=Linux.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=LinuxSingle.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=macOS.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=macOSSingle.pubxml
dotnet publish MailQueueNet.Service -c Release /p:PublishProfile=Portable.pubxml
