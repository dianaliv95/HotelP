using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Entities
{
    public class AccommodationType
    {
        public int ID { get; set; }

        
        public string Name { get; set; }


         // Dodaj, jeśli "Description" musi mieć wartość
        public string Description { get; set; }

    }

}
