﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //deklaracja listy PageVM
            List<PageVM> pagesList;

            //inicjalizacja listy
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            // zwracamy strony do widoku
            return View(pagesList);
        }
    }
}