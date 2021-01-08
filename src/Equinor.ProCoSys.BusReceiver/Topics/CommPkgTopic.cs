namespace Equinor.ProCoSys.BusReceiver.Topics
{
    public class CommPkgTopic
    {
        public string ProjectSchema { get; set; }
        public string ProjectName { get; set; }
        public string CommPkgNo { get; set; }
        public string Description { get; set; }
        public string TopicName { get { return "commpkg"; } }
    }
}
