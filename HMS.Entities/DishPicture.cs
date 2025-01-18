namespace HMS.Entities
{
    public class DishPicture
    {
        public int ID { get; set; }

        // klucz obcy do Dish
        public int DishID { get; set; }
        public virtual Dish Dish { get; set; }

        // klucz obcy do Picture
        public int PictureID { get; set; }
        public virtual Picture Picture { get; set; }
    }
}
