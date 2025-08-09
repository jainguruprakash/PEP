using System;

namespace PEPScanner.Domain.Entities
{
    public class CustomerRelationship
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RelatedCustomerId { get; set; }
        public string RelationshipType { get; set; } = string.Empty; // e.g., Spouse, BusinessAssociate
        public string? RelationshipDetails { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public Customer? Customer { get; set; }
        public Customer? RelatedCustomer { get; set; }
    }
}
