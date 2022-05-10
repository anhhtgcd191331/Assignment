using Assignment1.Areas.Identity.Data;

namespace Assignment1.Models
{
    public class CartItem
    {
        public string UserID { get; set; }
        public Assignment1User? User { get; set; }

        public string BookIsbn { get; set; }
        public Book? Book { get; set; }

        public int Quantity { set; get; }

        public CartItem()
        {
        }
    }

}