using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace MailQueue
{
    public static class PipeFactory
    {
        public static ISettingsContract NewSettingsChannel()
        {
            ChannelFactory<ISettingsContract> channelFactory = new ChannelFactory<ISettingsContract>(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), new EndpointAddress(Constants.SettingsPipeFullPath));
            return channelFactory.CreateChannel();
        }

        public static IMailContract NewMailChannel()
        {
            ChannelFactory<IMailContract> channelFactory = new ChannelFactory<IMailContract>(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), new EndpointAddress(Constants.MailPipeFullPath));
            return channelFactory.CreateChannel();
        }
    }
}
