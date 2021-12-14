namespace MailQueueNet.Grpc
{
    public partial class Settings
    {
        public string QueueFolder
        {
            get { return OptQueueFolderCase == OptQueueFolderOneofCase.None ? null : OptQueueFolderValue; }
            set { if (value == null) ClearOptQueueFolder(); else OptQueueFolderValue = value; }
        }

        public string FailedFolder
        {
            get { return OptFailedFolderCase == OptFailedFolderOneofCase.None ? null : OptFailedFolderValue; }
            set { if (value == null) ClearOptFailedFolder(); else OptFailedFolderValue = value; }
        }
    }
}
