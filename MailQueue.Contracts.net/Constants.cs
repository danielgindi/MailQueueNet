using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailQueue
{
    public static class Constants
    {
        public const string PipePath = "net.pipe://mailqueue";
        public const string MailPipePath = "mail";
        public const string MailPipeFullPath = PipePath + "/" + MailPipePath;
        public const string SettingsPipePath = "settings";
        public const string SettingsPipeFullPath = PipePath + "/" + SettingsPipePath;
    }
}
