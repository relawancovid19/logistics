using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Logistics.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class OrganizationsController : BaseController
    {
        // GET: Organizations
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public async Task<ActionResult> Items()
        {
            var items = await db.Items.ToListAsync();
            return View(items);
        }
        public async Task<ActionResult> EditItem(string id)
        {
            var item = await db.Items.Where(x => x.Id == id).SingleOrDefaultAsync();
            return View(item);
        }
        [HttpPost]
        public async Task<ActionResult> EditItem(ViewModels.AddItem data)
        {
            var item = await db.Items.Where(x => x.Id == data.Id).SingleOrDefaultAsync();
            if(item != null)
            {
                item.Name = data.Name ?? item.Name;
                item.Descriptions = data.Descriptions ?? item.Descriptions;
                item.Images = await Helpers.UploadFileHelper.UploadBannerImage(data.Images) ?? item.Images; 
                db.Entry(item).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("Items");
                }
            }
            return View(item);
        }
        public async Task<ActionResult> AddStock(string id)
        {
            var item = await db.Items.Where(x => x.Id == id).SingleOrDefaultAsync();
            return View(item);
        }
        [HttpPost]
        public async Task<ActionResult> AddStock(string id, int Amount)
        {
            var item = await db.Items.Where(x => x.Id == id).SingleOrDefaultAsync();
            if(item != null)
            {
                item.Amount = Amount;
                db.Entry(item).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if(result > 0)
                {
                    return RedirectToAction("Items");
                }
            }
            return View(item);
        }

        public async Task<ActionResult> AddItem()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddItem(ViewModels.AddItem addItem)
        {
            if (ModelState.IsValid)
            {
                var newItem = new Models.Item()
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = addItem.Amount,
                    Descriptions = addItem.Descriptions,
                    Name = addItem.Name,
                    Unit = addItem.Unit,
                    Images = await Helpers.UploadFileHelper.UploadBannerImage(addItem.Banner)
                };
                db.Items.Add(newItem);
                var result = await db.SaveChangesAsync();
                if(result > 0)
                {
                    return RedirectToAction("Items");
                }
            }
            return View();
        }
        public async Task<ActionResult> Orders()
        {
            var orders = await db.Orders.Include("User").Include("Province").Where(x => x.Status != Models.OrderStatus.Deleted).ToListAsync();
            return View(orders);
        }
        public async Task<ActionResult> DetailOrder(string id)
        {
            var order = await db.Orders.Include("Items").Include("Delivery").Include("User").Include("Province").Include("Items.Item").Where(x => x.Id == id).SingleOrDefaultAsync();
            ViewBag.Id = order.Id;
            return View(order);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDelivered(ViewModels.Delivery delivery, string IdOrder)
        {
            var order = await db.Orders.Where(x => x.Id == IdOrder).SingleOrDefaultAsync();
            if (order != null)
            {
                var newDelivery = new Models.Delivery()
                {
                    Id = Guid.NewGuid().ToString(),
                    Created = DateTimeOffset.Now,
                    ETA = delivery.ETA,
                    TrackingNumber = delivery.TrackingNumber,
                    Service = delivery.Service,
                    Status = Models.DeliveryStatus.Procesing
                };
                order.Delivery = newDelivery;
                order.Status = Models.OrderStatus.Delivered;
                db.Entry(order).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("DetailOrder", new { id = IdOrder });
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DetailOrder(ViewModels.UpdateTransaction data)
        {
            var order = await db.Orders.Include("User").Where(x => x.Id == data.Id).SingleOrDefaultAsync();
            if (order != null)
            {
                order.Status = data.Status;
                db.Entry(order).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    if (data.Status == Models.OrderStatus.Approved)
                    {
                        await SendOrderApproveEmail(order.User);
                    }
                    else if (data.Status == Models.OrderStatus.Rejected)
                    {
                        await SendOrderRejectEmail(order.User);
                    }
                    else if (data.Status == Models.OrderStatus.Delivered)
                    {
                        await SendOrderDeliveredEmail(order.User);
                    }
                    return RedirectToAction("DetailOrder", new { id = data.Id });

                }
            }
            return View("Error");
        }
        private async Task SendOrderApproveEmail(Models.ApplicationUser user)
        {
            var emailTemplate = await db.EmailTemplates.FindAsync("approve-order");
            var callbackUrl = Url.Action("TrackingOrder", "Home", null, protocol: Request.Url.Scheme);
            if (emailTemplate != null)
            {
                var emailBody = emailTemplate.Content
                    .Replace("[FullName]", user.FullName)
                    .Replace("[Logo]", "https://logistics.relawancovid19.id/assets/images/logo/logo-relawan-covid19.png")
                    .Replace("[Vector]", "~/assets/images/email-template/vector-approve.png")
                    .Replace("[Line]", "~/assets/images/email-template/lines.png")
                    .Replace("[Url]", callbackUrl);
                await Helpers.EmailHelper.Send(emailTemplate.Subject, user.Email, user.FullName, emailBody);
            }
        }
        private async Task SendOrderRejectEmail(Models.ApplicationUser user)
        {
            var emailTemplate = await db.EmailTemplates.FindAsync("reject-order");
            if (emailTemplate != null)
            {
                var emailBody = emailTemplate.Content
                    .Replace("[FullName]", user.FullName)
                    .Replace("[Logo]", "https://logistics.relawancovid19.id/assets/images/logo/logo-relawan-covid19.png")
                    .Replace("[Vector]", "~/assets/images/email-template/vector-cancel.png")
                    .Replace("[Line]", "~/assets/images/email-template/lines.png");
                await Helpers.EmailHelper.Send(emailTemplate.Subject, user.Email, user.FullName, emailBody);
            }
        }
        private async Task SendOrderDeliveredEmail(Models.ApplicationUser user)
        {
            var emailTemplate = await db.EmailTemplates.FindAsync("delivered-order");
            if (emailTemplate != null)
            {
                var emailBody = emailTemplate.Content
                    .Replace("[FullName]", user.FullName)
                    .Replace("[Logo]", "https://logistics.relawancovid19.id/assets/images/logo/logo-relawan-covid19.png")
                    .Replace("[Vector]", "~/assets/images/email-template/vector-approve.png")
                    .Replace("[Line]", "~/assets/images/email-template/lines.png");
                await Helpers.EmailHelper.Send(emailTemplate.Subject, user.Email, user.FullName, emailBody);
            }
        }
    }
}