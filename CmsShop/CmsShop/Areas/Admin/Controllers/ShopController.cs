using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;

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
                product.Slug = model.Name.Replace(" ", "-").ToLower();
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

            //Utworzenie struktury katalogów
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string[] pathStrings = new string[5]; 

            pathStrings[0] = Path.Combine(originalDirectory.ToString(), "Products");
            pathStrings[1] = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            pathStrings[2] = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            pathStrings[3] = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            pathStrings[4] = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            foreach (var pathString in pathStrings)
            {
                if (!Directory.Exists(pathString))
                    Directory.CreateDirectory(pathString);
            }

            if (file != null && file.ContentLength > 0)
            {
                //sprawdzenie rozszerzenia pliku
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/gif"
                    && ext != "image/png" && ext != "image/x-png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany.");
                        return View(model);
                    }
                }
                //inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDto dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathStrings[1], imageName);
                var path2 = string.Format("{0}\\{1}", pathStrings[2], imageName);

                //zapis obrazka
                file.SaveAs(path);

                //zapis miniaturki
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //deklaracja listy produktów
            List<ProductVM> listOfProductVM;
            
            //ustawiamy numer strony
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                //inicjalizacja listy produktów
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x)).ToList();

                //lista kategorii do dropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(),"Id","Name");

                //ustawiamy wybraną kategorię
                ViewBag.SelectedCat = catId.ToString();
            }

            //ustawienie stronnicowania
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }
    }
}