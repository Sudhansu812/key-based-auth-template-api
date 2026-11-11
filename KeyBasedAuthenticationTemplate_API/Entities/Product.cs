namespace KeyBasedAuthenticationTemplate_API.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // The OwnerUsername of the API key that created the product. Plain text, not a
        // foreign key (API keys live in the separate SQL Server database).
        public string OwnerUsername { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
