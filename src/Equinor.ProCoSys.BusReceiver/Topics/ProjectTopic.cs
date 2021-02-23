namespace Equinor.ProCoSys.BusReceiver.Topics
{
    public class ProjectTopic
    {
        public string ProjectSchema { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string IsClosed { get; set; }
        public string TopicName { get { return "project"; } }
    }
}
