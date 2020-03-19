using Logistics.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Logistics.Controllers
{
    [Authorize(Roles = "SA")]
    public class AdministratorsController : BaseController
    {
        //// GET: Administrators
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public async Task<ActionResult> Organizations()
        {
            var organizations = await db.Organizations.Include("Province").ToListAsync();
            return View(organizations);
        }
        public async Task<ActionResult> AddOrganization()
        {
            var provinces = await db.Provinces.Where(x => x.IsActive == true)
                .Select(i => new SelectListItem()
                {
                    Text = i.Name,
                    Value = i.IdProvince,
                    Selected = false
                }).ToArrayAsync();

            ViewBag.Provinces = provinces;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddOrganization(ViewModels.AddOrganization data)
        {
            if (ModelState.IsValid)
            {
                var currentUTCTime = DateTimeOffset.UtcNow;
                var province = await db.Provinces.Where(x => x.IdProvince == data.Province).SingleOrDefaultAsync();
                var addOrganization = new Models.Organization()
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = data.FullName,
                    Title = data.Title,
                    Registered = currentUTCTime,
                    Updated = currentUTCTime,
                    Institution = data.Institution,
                    Email = data.Email,
                    EmailConfirmed = true,
                    PhoneNumber = data.PhoneNumber,
                    UserName = data.Email,
                    RegistrationStatus = RegistrationStatus.Approved,
                    Address = data.Address,
                    FacebookUrl = data.FacebookUrl,
                    InstagramUrl = data.InstagramUrl,
                    TwitterUrl = data.TwitterUrl,
                    LinkedInUrl = data.LinkedInUrl,
                    YoutubeUrl = data.YoutubeUrl
                };
                try
                {
                    var addOrganizerResult = await UserManager.CreateAsync(addOrganization, data.Password);
                    var currentOrganization = await UserManager.FindByEmailAsync(data.Email);
                    var addToRoleResult = await UserManager.AddToRoleAsync(currentOrganization.Id, "Administrator");
                    if (addOrganizerResult.Succeeded && addToRoleResult.Succeeded)
                    {
                        var addOrganizationProvince = await db.Organizations.Include("Province").
                            Where(x => x.Id == currentOrganization.Id).SingleOrDefaultAsync();
                        addOrganizationProvince.Province = province;
                        var result = await db.SaveChangesAsync();
                        if (result > 0)
                        {
                            return RedirectToAction("Organizations");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.StackTrace);
                }
            }
            var provinces = await db.Provinces.Where(x => x.IsActive == true)
                .Select(i => new SelectListItem()
                {
                    Text = i.Name,
                    Value = i.IdProvince,
                    Selected = false
                }).ToArrayAsync();

            ViewBag.Provinces = provinces;
            return View();
        }
    }
}