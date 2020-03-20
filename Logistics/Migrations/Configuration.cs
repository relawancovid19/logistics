namespace Logistics.Migrations
{
    using Logistics.Infrastructures;
    using Logistics.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Logistics.Infrastructures.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Logistics.Infrastructures.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            if (roleManager.Roles.Count() == 0)
            {
                roleManager.Create(new IdentityRole { Name = "SA" });
                roleManager.Create(new IdentityRole { Name = "Volunteer" });
                roleManager.Create(new IdentityRole { Name = "Administrator" });
            };
            var admin = new ApplicationUser
            {
                PhoneNumber = "+62818271214",
                PhoneNumberConfirmed = true,
                UserName = "alex@cloudcomputing.id",
                Email = "alex@cloudcomputing.id",
                FullName = "Alex Budiyanto",
                Institution = "ACCI",
                Title = "CEO"
            };
            if (manager.FindByName("alex@cloudcomputing.id") == null)
            {
                manager.Create(admin, "Volunteer@2020");
                manager.AddToRoles(admin.Id, new string[] { "Administrator", "SA" });
            }
            var user = new ApplicationUser
            {
                PhoneNumber = "+62818271214",
                PhoneNumberConfirmed = true,
                UserName = "user@user.com",
                Email = "user@user.com",
                FullName = "Alex Budiyanto",
                Institution = "ACCI",
                Title = "CEO"
            };
            if (manager.FindByName("user@user.com") == null)
            {
                manager.Create(user, "User@2020");
                manager.AddToRoles(user.Id, new string[] { "Volunteer" });
            }
        }
    }
}
