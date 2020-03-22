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
            var order = await db.Orders.Include("Items").Include("User").Include("Province").Where(x => x.Id == id).SingleOrDefaultAsync();
            ViewBag.Items = await db.Orders.Include("Items").Where(x => x.Id == id).ToListAsync();
            return View(order);
        }

        [HttpPost]
        public async Task<ActionResult> DetailOrder(ViewModels.UpdateTransaction data)
        {
            var volunteer = await db.Orders.Include("User").Where(x => x.Id == data.Id).SingleOrDefaultAsync();
            if (volunteer != null)
            {
                volunteer.Status = data.Status;
                db.Entry(volunteer).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    if (data.Status == Models.OrderStatus.Approved)
                    {
                        await SendOrderApproveEmail(volunteer.User);
                    }
                    return RedirectToAction("DetailOrder", new { id = data.Id });

                }
            }
            return View("Error");
        }
        private async Task SendOrderApproveEmail(Models.ApplicationUser user)
        {
            var emailTemplate = await db.EmailTemplates.FindAsync("approve-order");
            if (emailTemplate != null)
            {
                var emailBody = emailTemplate.Content.Replace("[FullName]", user.FullName);
                await Helpers.EmailHelper.Send(emailTemplate.Subject.Replace("[SessionName]", user.FullName), user.Email, user.FullName, emailBody);
            }
        }
    }
}