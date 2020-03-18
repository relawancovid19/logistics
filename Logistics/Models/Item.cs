using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Logistics.Models
{
    public class Item
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }
        public ICollection<string> Images { get; set; }
        public Unit Unit { get; set; }
        public ICollection<string> References { get; set; }
        public int Amount { get; set; }
    }
    public enum Unit
    {
        Box,
        Buah,
        Pasang,
        Rol,
        Kg,
        Meter,
        Vial
    }
}