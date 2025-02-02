﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;   

        public ProductController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;   
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _db.Product;

            foreach (var product in products)
            {
                  product.Category = _db.Category.FirstOrDefault(u => u.Id == product.CategoryId);
            };
            
            return View(products);
        }


        //Get - Upsert
        public IActionResult Create()
        {
            return View();
        }

        //Get - Upsert
        public IActionResult Upsert(int? id) //если редактируем то передаем id
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});

            //ViewBag.CategoryDropDown = CategoryDropDown;    

            //Product product = new Product();


            ProductVM productVM = new ProductVM() 
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM.Product);
            }
        }

        //Post - Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid) // валидация на стороне сервера
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    //Creating
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;
                    _db.Product.Add(productVM.Product);


                }
                else
                {
                    //updating
                }
                _db.SaveChanges();  
                return RedirectToAction("Index");
            }
            return View();
        }

        //delete

        //Get - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _db.Category.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        //Post - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var category = _db.Category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
                _db.Category.Remove(category);
                _db.SaveChanges();
                return RedirectToAction("Index");    
        }


    }
}
