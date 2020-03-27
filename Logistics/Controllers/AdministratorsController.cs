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
                            return Json("OK", JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> EmailTemplates()
        {

            var email = await db.EmailTemplates.ToListAsync();

            return View(email);
        }
        [HttpGet]
        public async Task<ActionResult> AddEmailTemplate()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddEmailTemplate(ViewModels.EmailTemplate newEmailTemplate)
        {

            if (ModelState.IsValid)
            {
                var currentUser = await db.Users.Where(x => x.UserName == User.Identity.Name)
                    .SingleOrDefaultAsync();
                var emailTemplate = new Models.EmailTemplate()
                {
                    IdEmailTemplate = newEmailTemplate.IdEmailTemplate,
                    Subject = newEmailTemplate.Subject,
                    Content = newEmailTemplate.Content,
                    CreatedBy = currentUser
                };

                try
                {
                    db.EmailTemplates.Add(emailTemplate);
                    var result = await db.SaveChangesAsync();
                    if (result > 0)
                    {
                        return RedirectToAction("EmailTemplates");
                    }
                }
                catch (Exception ex)
                {

                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.StackTrace);
                }
            }

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> EditEmail(ViewModels.EmailTemplate updateEmailTemplate)
        {
            if (ModelState.IsValid)
            {

                var currentUser = await db.Users.Where(x => x.UserName == User.Identity.Name)
                    .SingleOrDefaultAsync();
                var currentUTCTime = DateTimeOffset.UtcNow;
                var currentEmailTemplate = await db.EmailTemplates.FindAsync(updateEmailTemplate.IdEmailTemplate);
                if (currentEmailTemplate != null)
                {

                    currentEmailTemplate.Subject = updateEmailTemplate.Subject;
                    currentEmailTemplate.Content = updateEmailTemplate.Content;
                    try
                    {
                        db.Entry(currentEmailTemplate).State = EntityState.Modified;
                        var result = await db.SaveChangesAsync();
                        if (result > 0)
                        {
                            return RedirectToAction("EmailTemplates");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                        Trace.TraceError(ex.StackTrace);
                    }
                }
                else
                {

                    return View("NotFound");
                }
            }

            return View("ModelStateError");
        }


        [HttpGet]
        public async Task<ActionResult> EditEmail(string id)
        {

            if (id != null)
            {

                var emailTemplate = await db.EmailTemplates.FindAsync(id);

                if (emailTemplate != null)
                {

                    return View(emailTemplate);
                }
                else
                {

                    return View("NotFound");
                }
            }

            return View("Error");
        }
        public async Task<ActionResult> EmailDetails(String id)
        {
            if (id != null)
            {
                var emailDetail = await db.EmailTemplates.Where(x => x.IdEmailTemplate == id)
                    .SingleOrDefaultAsync();
                if (emailDetail != null)
                {
                    return View(emailDetail);
                }
                else
                {
                    return View("NotFound");
                }
            }
            return View("Error");
        }
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
            if (item != null)
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
            if (item != null)
            {
                item.Amount = Amount;
                db.Entry(item).State = EntityState.Modified;
                var result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("Items");
                }
            }
            return View(item);
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
                    //if (data.Status == Models.OrderStatus.Approved)
                    //{
                    //    await SendProgramRegistrationEmail(volunteer.Job, volunteer.Volunteer);

                    //}
                    return RedirectToAction("DetailOrder", new { id = data.Id });

                }
            }
            return View("Error");
        }
    }
}