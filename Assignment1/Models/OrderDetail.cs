namespace Assignment1.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string BookIsbn { get; set; }
        public Book Book { get; set; }

        public int Quantity { get; set; }
    }
}