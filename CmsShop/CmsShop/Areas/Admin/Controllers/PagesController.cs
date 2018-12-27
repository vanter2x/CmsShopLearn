using System.Collections.Generic;
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

        //GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        //POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //sprawdzanie formularza (ModelState)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;
                
                //inicjalizacja PageDto
                PageDTO dto = new PageDTO();
               
                //gdy nie mamy adresu to przypisujemy tytuł
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //zapobiegamy dodaniu takiej samej nazwy strony
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("","Ten tytuł lub adres strony już istnieje.");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                //zapis dto do bazy
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "Dodałeś nową stronę";
            return RedirectToAction("AddPage");
        }

        //GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                // pobieramy z bazy stronę o podanym id
                PageDTO dto = db.Pages.Find(id);

                // sprawdzamy czy ta strona istnieje
                if (dto == null)
                {
                    return Content("Strona nie istnieje.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }
    }
}