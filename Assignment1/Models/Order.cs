using Assignment1.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment1.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public Assignment1User User { get; set; }

        public DateTime OrderTime { get; set; }

        public double Total { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }

        public Order()
        {
        }
    }
}