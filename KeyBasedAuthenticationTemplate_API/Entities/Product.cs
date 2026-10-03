namespace KeyBasedAuthenticationTemplate_API.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // References User.Id in the (separate) auth database. No FK is possible
        // across SQL Server/PostgreSQL, so this is a plain, unenforced Guid column.
        public Guid OwnerUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
