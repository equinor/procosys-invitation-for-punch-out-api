namespace Equinor.ProCoSys.IPO.Domain
{
    public interface IPlantProvider
    {
        string Plant { get; }

        // HACK To be removed after bugfixed optimistic concurreny
        // This prop is misplaced here since this interface is alredt injected into IPOContext where needed.
        // This to not break 100+ constructions of IPOContext in tests
        bool IsOptimisticConcurrenyEnabled_HACK { get; }
    }
}
