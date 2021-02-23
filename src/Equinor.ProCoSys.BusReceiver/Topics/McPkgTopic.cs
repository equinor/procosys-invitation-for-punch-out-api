namespace Equinor.ProCoSys.BusReceiver.Topics
{
    public class McPkgTopic
    {
        public string ProjectSchema { get; set; }
        public string ProjectName { get; set; }
        public string CommPkgNo { get; set; }
        public string CommPkgNoOld { get; set; }
        public string McPkgNo { get; set; }
        public string McPkgNoOld { get; set; }
        public string Description { get; set; }
        public string TopicName { get { return "mcpkg"; } }
    }
}
