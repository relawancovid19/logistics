using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Logistics.ViewModels
{
    public class EmailTemplate
    {
        public string IdEmailTemplate { get; set; }
        public string Subject { get; set; }
        [AllowHtml]
        public string Content { get; set; }
    }
}