using System.ComponentModel.DataAnnotations.Schema;

namespace SarEquipEnterprise.Models
{
    [Table("Items")]
    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
