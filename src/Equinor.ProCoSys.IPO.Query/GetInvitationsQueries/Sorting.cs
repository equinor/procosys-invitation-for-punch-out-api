namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries
{
    public class Sorting
    {
        public Sorting(SortingDirection direction, SortingProperty property)
        {
            Direction = direction;
            Property = property;
        }

        public SortingDirection Direction { get; }
        public SortingProperty Property { get; }
    }
}
