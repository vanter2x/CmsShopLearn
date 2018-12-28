﻿using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x)).ToList();
            }

            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //deklaracja id
            string id;
            using (Db db = new Db())
            {
                //sprawdzenie czy nazwa kategorii jest unikatowa
                if (db.Categories.Any(x => x.Name == catName))
                    return "tytulzajety";

                CategoryDto dto = new CategoryDto();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();
            }
            return id;
        }
    }
}