﻿namespace HMS.Entities
{
    public class Category
    {
        public int ID { get; set; } // Klucz główny
        public string Name { get; set; } // Nazwa kategorii
        public string Description { get; set; } // Opis kategorii

       
        public ICollection<Dish> Dishes { get; set; }
    }
}
