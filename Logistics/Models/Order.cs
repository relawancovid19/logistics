using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Logistics.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public ICollection<OrderItem> Items { get; set; }
        public ApplicationUser User { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public string Notes { get; set; }
        public Priority Priority { get; set; }
        public OrderStatus Status { get; set; }
        public string DeliveryAddress { get; set; }
        public Province Province { get; set; }
        public Delivery Delivery { get; set; }
    }
    public enum OrderStatus
    {
        Processing,
        Pending,
        Rejected,
        Approved,
        Deleted,
        Delivered
    }
    public enum Priority
    {
        Mendesak,
        Tinggi,
        Rendah
    }
    public class OrderItem
    {
        public string Id { get; set; }
        public Item Item { get; set; }
        public int Amount { get; set; }
        public Unit Unit { get; set; }
    }
    public class Delivery
    {
        public string Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset ETA { get; set; }
        public string TrackingNumber { get; set; }
        public DeliveryService Service { get; set; }
        public DeliveryStatus Status { get; set; }
    }
    public enum DeliveryService
    {
        Gojek,
        Grab,
        TIKI,
        JNE,
        Relawan
    }
    public enum DeliveryStatus
    {
        Pending,
        Procesing,
        Arrived,
        Cancelled
    }
}