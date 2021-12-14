namespace MailQueueNet.Grpc
{
    public partial class ContentDisposition
    {
        public string DispositionType
        {
            get { return OptDispositionTypeCase == OptDispositionTypeOneofCase.None ? null : OptDispositionTypeValue; }
            set { if (value == null) ClearOptDispositionType(); else OptDispositionTypeValue = value; }
        }

        public string FileName
        {
            get { return OptFileNameCase == OptFileNameOneofCase.None ? null : OptFileNameValue; }
            set { if (value == null) ClearOptFileName(); else OptFileNameValue = value; }
        }

        public string CreationDate
        {
            get { return OptCreationDateCase == OptCreationDateOneofCase.None ? null : OptCreationDateValue; }
            set { if (value == null) ClearOptCreationDate(); else OptCreationDateValue = value; }
        }

        public string ModificationDate
        {
            get { return OptModificationDateCase == OptModificationDateOneofCase.None ? null : OptModificationDateValue; }
            set { if (value == null) ClearOptModificationDate(); else OptModificationDateValue = value; }
        }

        public string ReadDate
        {
            get { return OptReadDateCase == OptReadDateOneofCase.None ? null : OptReadDateValue; }
            set { if (value == null) ClearOptReadDate(); else OptReadDateValue = value; }
        }
    }
}
