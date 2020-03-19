using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Logistics.Controllers
{
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
        public async Task<ActionResult> PreviewOrder(string id)
        {
            var items = await db.Items.ToListAsync();
            ViewBag.Items = items;
            return View();
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