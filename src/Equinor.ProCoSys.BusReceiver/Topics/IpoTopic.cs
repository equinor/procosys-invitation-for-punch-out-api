namespace Equinor.ProCoSys.BusReceiver.Topics
{
    public class IpoTopic
    {
        public string ProjectSchema { get; set; }
        public string InvitationGuid { get; set; }
        public string Event { get; set; }
        public string TopicName { get { return "ipo"; } }
        public int Status { get; set; }

    }
}
