using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BooksApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BooksApp.Controllers;

public class HomeController : Controller
{

    public HomeController()
    {

    }

    public IActionResult Index(string searchString, string category)
    {
        var products = Repository.Products;

        if (!string.IsNullOrEmpty(searchString))
        {
            ViewBag.SearchString = searchString;
            products = products.Where(p => p.Name!.ToLower().Contains(searchString)).ToList();
        }

        if (!string.IsNullOrEmpty(category) && category != "0")
        {
            products = products.Where(p => p.CategoryId == int.Parse(category)).ToList();
        }

        // ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId","Name",category);

        var model = new ProductViewModel
        {
            Products = products,
            Categories = Repository.Categories,
            SelectedCategory = category
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model, IFormFile imageFile)
    {
        if (imageFile != null)
        {
            var imageName = await ProcessImageFile(imageFile);
            if (imageName != null)
            {
                model.Image = imageName;
            }
        }
        else
        {
            ModelState.AddModelError("", "Bir resim seçiniz!");
        }

        if (ModelState.IsValid)
        {
            model.ProductId = Repository.Products.Count + 1;
            Repository.CreateProduct(model);
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(model);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
        if (entity == null)
        {
            return NotFound();
        }
        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
    {

        if (id != model.ProductId)
        {
            return NotFound();
        }

        if (imageFile != null)
        {
            var imageName = await ProcessImageFile(imageFile);
            if (imageName != null)
            {
                model.Image = imageName;
            }
        }

        if (ModelState.IsValid)
        {
            Repository.EditProduct(model);
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(model);

    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
        if (entity == null)
        {
            return NotFound();
        }
        Repository.DeleteProduct(entity);
        return RedirectToAction("index");
    }


    // repositoryde oluşturudğum ActiveProducts listesini bu fonksiyon içerinde kullandım
    // kalan kısımlar ise Index ile aynıdır
    // cshtml sayfasında ise search ve filtreleme sonrasında Eticaret action'a yönlendirecek şekilde değiştirildi
    public IActionResult ETicaret(string searchString, string category)
    {
        var products = Repository.ActiveProducts;

        if (!string.IsNullOrEmpty(searchString))
        {
            ViewBag.SearchString = searchString;
            products = products.Where(p => p.Name!.ToLower().Contains(searchString)).ToList();
        }

        if (!string.IsNullOrEmpty(category) && category != "0")
        {
            products = products.Where(p => p.CategoryId == int.Parse(category)).ToList();
        }

        // ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId","Name",category);

        var model = new ProductViewModel
        {
            Products = products,
            Categories = Repository.Categories,
            SelectedCategory = category
        };
        return View(model);
    }


    // resim yükleme işlemi için ortak olan kodu bir fonksiyona dönüştürerek daha okunabilir ve modüler bi hale getirdim.
    // bu fonksiyonun içerinde dosya boyutunu belirttim ve gerekli validate işlemini yaptım. 
    private async Task<string?> ProcessImageFile(IFormFile imageFile)
    {
        var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
        const long maxFileSize = 2 * 1024 * 1024; // 2 MB

        // Dosya boyutunu kontrol et
        if (imageFile.Length > maxFileSize)
        {
            ModelState.AddModelError("", "Resim boyutu 2 MB'dan büyük olamaz.");
            return null;
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        // Dosya uzantısını kontrol et
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("", "Geçerli bir resim türü seçiniz.");
            return null;
        }

        var randomFileName = $"{Guid.NewGuid()}{extension}";
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

        // Dosyayı kaydet
        try
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
        }
        catch
        {
            ModelState.AddModelError("", "Dosya yüklenirken bir hata oluştu!");
            return null;
        }

        return randomFileName;
    }

}
