using Logistics.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Logistics.ViewModels
{
    public class LogisticsViewModels
    {
    }
    public class AddOrganization
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string Institution { get; set; }
        public string Title { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string YoutubeUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
    }
    public class AddItem
    {
        public string IdOrder { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        [System.Web.Mvc.AllowHtml]
        public string Descriptions { get; set; }
        public HttpPostedFileBase Banner { get; set; }
        public HttpPostedFileBase Images { get; set; }
        public Unit Unit { get; set; }
        public int Amount { get; set; }
    }
    public class Order
    {
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public Priority Priority { get; set; }
        public string DeliveryAddress { get; set; }
        public string Province { get; set; }
    }
}