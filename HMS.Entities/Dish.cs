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
        public virtual Category Category { get; set; }

        // Lista powiązanych zdjęć
        public virtual List<DishPicture> DishPictures { get; set; } = new();
    }
}
