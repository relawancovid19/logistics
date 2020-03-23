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
    //[Authorize]
    public class HomeController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            //var provinces = await db.Provinces.Where(x => x.IsActive == true)
            //    .Select(i => new SelectListItem()
            //    {
            //        Text = i.Name,
            //        Value = i.IdProvince,
            //        Selected = false
            //    }).ToArrayAsync();
            //ViewBag.Provinces = provinces;

            var items = await db.Items.ToListAsync();
            ViewBag.Items = items;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateOrder(ViewModels.Register register)
        {
            var checkUser = await CheckUser(register);
            if (checkUser != null)
            {
                var user = await db.Users.Where(x => x.Id == checkUser.Id).SingleOrDefaultAsync();
                var province = await db.Provinces.Where(x => x.IdProvince == "ID-JB").SingleOrDefaultAsync();
                var newOrder = new Models.Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    Province = province,
                    Created = DateTimeOffset.Now,
                    User = user,
                    Status = Models.OrderStatus.Processing
                };
                db.Orders.Add(newOrder);
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("PreviewOrder", new { id = newOrder.Id });
                }
            }
            return View();
        }
        public async Task<Models.ApplicationUser> CheckUser(ViewModels.Register register)
        {
            var currentUTCTime = DateTimeOffset.UtcNow;
            var province = await db.Provinces.Where(x => x.IdProvince == "ID-JB").SingleOrDefaultAsync();
            var user = new Models.ApplicationUser()
            {
                Id = Guid.NewGuid().ToString(),
                FullName = register.FullName,
                UserName = register.Email,
                PhoneNumber = register.PhoneNumber,
                Registered = DateTimeOffset.UtcNow,
                Email = register.Email,
                Institution = register.Institution,
                Title = register.Title,
                EmailConfirmed = true

            };
            var searchUser = await db.Users.Where(x => x.Email == register.Email).SingleOrDefaultAsync();
            if (searchUser == null)
            {
                var addVolunteer = await UserManager.CreateAsync(user, "2020@Logistik!");
                var currentUser = await UserManager.FindByEmailAsync(register.Email);
                var addToRoleResult = await UserManager.AddToRoleAsync(currentUser.Id, "Requestor");
                if (addVolunteer.Succeeded && addToRoleResult.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    var addUserProvince = await db.Users.Include("Province").
                                                    Where(x => x.Id == currentUser.Id).SingleOrDefaultAsync();
                    addUserProvince.Province = province;
                    var result = await db.SaveChangesAsync();
                    if (result > 0)
                    {
                        return currentUser;
                    }
                }
            }
            else
            {
                return searchUser;
            }
            return null;
        }
        public async Task<ActionResult> AddItem(ViewModels.AddItem data)
        {
            var order = await db.Orders.Include("Items").Include("Items.Item").Where(x => x.Id == data.IdOrder).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            if(order != null)
            {
                var item = await db.Items.FindAsync(data.Id);
                var exist = order.Items.Where(x => x.Item.Id == data.Id).FirstOrDefault();
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
            var cart = await db.Orders.Include("Items").Where(x => x.Id == id).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            ViewBag.Cart = cart.Items;
            ViewBag.Status = cart.Status.ToString();
            return View();
        }
        public async Task<ActionResult> InfoData(string id)
        {
            var order = await db.Orders.Include("Items").Include("Items.Item").Where(x => x.Id == id).OrderByDescending(x => x.Created).FirstOrDefaultAsync();
            if(order != null)
            {
                ViewBag.Id = order.Id;
                return View();
            }
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Finish()
        {
            return View();
        }
        public async Task<ActionResult> Success()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SubmitData(ViewModels.Order data)
        {
            var order = await db.Orders.Where(x => x.Id == data.Id).SingleOrDefaultAsync();
            if(order != null)
            {
                order.Title = data.Title;
                order.Priority = data.Priority;
                order.Descriptions = data.Descriptions;
                order.DeliveryAddress = data.DeliveryAddress;
                order.Status = OrderStatus.Pending;
                db.Entry(order).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("Finish");
                }
            }
            
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
        public async Task<ActionResult> DetailsItem(string id)
        {
            var detail = await db.Items.FindAsync(id);
            return View(detail);
        }
    }
}