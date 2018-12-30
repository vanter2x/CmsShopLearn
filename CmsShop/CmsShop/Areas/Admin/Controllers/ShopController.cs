using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Web;
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

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                CategoryDto dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
            return View();
        }

        // GET: Admin/Shop/DeleteCategory
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                CategoryDto dto = db.Categories.Find(id);

                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //sprawdzenie czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "tytulzajety";

                // Edycja kategorii
                CategoryDto dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();

            }
            return "Ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //inicjalizacja modelu
            ProductVM model = new ProductVM();

            // pobieranie listy kategorii
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                }
                return View(model);
            }
            //sprawdzenie czy nazwa produktu jest unikalna
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("","Ta nazwa produktu jest zajęta");
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //deklaracja ProductId
            int id;
            using (Db db = new Db())
            {
                ProductDto product = new ProductDto();
                product.Name = model.Name;
                product.Slug = model.Slug.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDto catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            TempData["SM"] = "Dodałeś produkt do bazy.";

            #region Upload Image

            

            #endregion

            return View();
        }
    }
}