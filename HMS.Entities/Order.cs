using System;
using System.Collections.Generic;

namespace HMS.Entities
{
    public class Order
    {
        public int ID { get; set; }
        public string GuestName { get; set; }
        public bool IsHotelGuest { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public int ID { get; set; }
        public int DishID { get; set; }
        public Dish Dish { get; set; }
        public int Quantity { get; set; }
    }
}
