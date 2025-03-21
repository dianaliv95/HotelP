﻿namespace HMS.Entities
{
    public class Dish
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        
        public int CategoryID { get; set; }
        public virtual Category Category { get; set; }

       
        public virtual List<DishPicture> DishPictures { get; set; } = new();
    }
}
