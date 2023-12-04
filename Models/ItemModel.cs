namespace Inventory.Models
{
    public class ItemModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public decimal ItemPrice { get; set; }
        public int ItemQuantity { get; set; }
        public int ItemVendorId { get; set; }
        public string ItemVendorName { get; set; }
        public string ItemVendorContactDetails { get; set; }
        public string ItemVendorAssociatedProducts { get; set; }
    }
}
