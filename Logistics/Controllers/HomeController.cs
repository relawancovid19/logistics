using Logistics.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Logistics.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var provinces = await db.Provinces.Where(x => x.IsActive == true)
                .Select(i => new SelectListItem()
                {
                    Text = i.Name,
                    Value = i.IdProvince,
                    Selected = false
                }).ToArrayAsync();
            ViewBag.Provinces = provinces;

            var items = await db.Items.ToListAsync();
            ViewBag.Items = items;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateOrder(ViewModels.Order data)
        {
            if (ModelState.IsValid)
            {
                //Register user here 
                
                //
                var province = await db.Provinces.Where(x => x.IdProvince == data.Province).SingleOrDefaultAsync();
                var user = await db.Users.Where(x => x.UserName == User.Identity.Name).SingleOrDefaultAsync();
                var newOrder = new Models.Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = data.Title,
                    Descriptions = data.Descriptions,
                    Priority = data.Priority,
                    Province = province,
                    Created = DateTimeOffset.Now,
                    User = user,
                    DeliveryAddress = data.DeliveryAddress,
                    Status = Models.OrderStatus.Processing
                };
                db.Orders.Add(newOrder);
                var result = await db.SaveChangesAsync();
                if(result > 0)
                {
                    return RedirectToAction("PreviewOrder", new { id = newOrder.Id });
                }
            }
            return View();
        }

        public async Task<ActionResult> AddItem(ViewModels.AddItem data)
        {
            var order = await db.Orders.Include("Items").Include("Items.Item").Where(x => x.Id == data.IdOrder && x.User.UserName == User.Identity.Name).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            if(order != null)
            {
                var item = await db.Items.FindAsync(data.Id);
                var exist = order.Items.Where(x => x.Item.Id == data.Id).SingleOrDefault();
                if (exist != null)
                {
                    exist.Amount = data.Amount;
                    db.Entry(exist).State = EntityState.Modified;
                    var result = await db.SaveChangesAsync();
                    if (result > 0)
                    {
                        return RedirectToAction("PreviewOrder", new { id = data.IdOrder });
                    }
                }
                else
                {
                    var addItem = new Models.OrderItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Amount = data.Amount,
                        Item = item,
                        Unit = item.Unit
                    };
                    order.Items.Add(addItem);
                    var result = await db.SaveChangesAsync();
                    if (result > 0)
                    {
                        return RedirectToAction("PreviewOrder", new { id = data.IdOrder });
                    }
                }
            }
            return View("Error");
        }
        [AllowAnonymous]
        public async Task<ActionResult> TrackingOrder()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> TrackingOrder(ViewModels.TrackingOrder data)
        {
            var order = await db.Orders.Include("Items").Include("Items.Item").Where(x => x.Id == data.IdOrder && x.User.UserName == data.Email).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            if(order != null)
            {
                ViewBag.Order = order;
                ViewBag.Items = order.Items;
                ViewBag.Status = order.Status.ToString();
                return View();
            }
            return View();
        }
        public async Task<ActionResult> PreviewOrder(string id)
        {
            var items = await db.Items.ToListAsync();
            ViewBag.Items = items;
            ViewBag.Id = id;
            var cart = await db.Orders.Include("Items").Where(x => x.Id == id && x.User.UserName == User.Identity.Name).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            ViewBag.Cart = cart.Items;
            ViewBag.Status = cart.Status.ToString();
            return View();
        }
        public async Task<ActionResult> SubmitOrder(string id)
        {
            var order = await db.Orders.Include("Items").Include("Items.Item").Where(x => x.Id == id && x.User.UserName == User.Identity.Name).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            if(order != null)
            {
                foreach(var item in order.Items)
                {
                    var items = await db.Items.Where(x => x.Id == item.Item.Id).SingleOrDefaultAsync();
                    if(items != null)
                    {
                        items.Amount = items.Amount - item.Amount;
                        db.Entry(items).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                order.Status = Models.OrderStatus.Pending;
                db.Entry(order).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
            }
            return View("Error");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}