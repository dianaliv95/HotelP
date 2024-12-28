namespace HMS.Entities
{
    public class Dish
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        // Relacja do kategorii
        public int CategoryID { get; set; }
        public Category Category { get; set; }
    }
}
