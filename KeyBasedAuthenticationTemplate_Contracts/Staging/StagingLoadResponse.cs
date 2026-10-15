namespace KeyBasedAuthenticationTemplate_Contracts.Staging
{
    public class StagingLoadResponse
    {
        public string Table { get; set; } = string.Empty;
        public int RowsLoaded { get; set; }
        public DateTime SnapshotAtUtc { get; set; }
    }
}
