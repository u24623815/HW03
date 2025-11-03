using HW03.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HW03.Controllers
{
    public class HomeController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Home
        public async Task<ActionResult> Index(int? brandFilter, int? categoryFilter)
        {
            // Get staffs with ALL details including sales - FIXED MANAGER QUERY
            var staffs = await db.staffs
                .Include(s => s.stores)
                .Include(s => s.staffs1) // This is the collection of staff they manage
                .Include(s => s.staffs2) // This should be their manager (reverse relationship)
                .Include(s => s.orders)   // Orders they created
                .Select(s => new StaffViewModel
                {
                    staff_id = s.staff_id,
                    first_name = s.first_name,
                    last_name = s.last_name,
                    email = s.email,
                    phone = s.phone,
                    active = s.active,
                    StoreName = s.stores.store_name,
                    // FIX: Get manager from staffs2 (reverse relationship)
                    ManagerName = s.staffs2 != null ? s.staffs2.first_name + " " + s.staffs2.last_name : "No Manager",
                    // Get products they sold
                    SoldProducts = s.orders.SelectMany(o => o.order_items
                        .Select(oi => new SoldProductViewModel
                        {
                            ProductName = oi.products.product_name,
                            Quantity = oi.quantity,
                            OrderDate = o.order_date,
                            CustomerName = o.customers.first_name + " " + o.customers.last_name
                        })).ToList()
                })
                .ToListAsync();

            // Rest of your code remains the same...
            var customers = await db.customers
                .Select(c => new CustomerViewModel
                {
                    customer_id = c.customer_id,
                    first_name = c.first_name,
                    last_name = c.last_name,
                    phone = c.phone,
                    email = c.email,
                    city = c.city,
                    state = c.state
                })
                .ToListAsync();

            // Products query
            var productsQuery = db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .AsQueryable();

            if (brandFilter.HasValue)
                productsQuery = productsQuery.Where(p => p.brand_id == brandFilter.Value);
            if (categoryFilter.HasValue)
                productsQuery = productsQuery.Where(p => p.category_id == categoryFilter.Value);

            var products = await productsQuery
                .Select(p => new ProductViewModel
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    brand_name = p.brands.brand_name,
                    category_name = p.categories.category_name,
                    model_year = p.model_year,
                    list_price = p.list_price,
                    ImagePath = "~/Content/Images/" +
                   p.brands.brand_name.Replace(" ", "_") + "_" +
                   p.categories.category_name.Replace(" ", "_") + ".jpeg"

                })
                .ToListAsync();

            // ViewBag for dropdowns
            ViewBag.Brands = new SelectList(await db.brands.ToListAsync(), "brand_id", "brand_name", brandFilter);
            ViewBag.Categories = new SelectList(await db.categories.ToListAsync(), "category_id", "category_name", categoryFilter);
            ViewBag.Stores = new SelectList(await db.stores.ToListAsync(), "store_id", "store_name");

            var viewModel = new HomeViewModel
            {
                Staffs = staffs,
                Customers = customers,
                Products = products,
                SelectedBrandId = brandFilter,
                SelectedCategoryId = categoryFilter
            };

            return View(viewModel);
        }

        // Your CreateStaff and CreateCustomer methods remain the same...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff(string first_name, string last_name, string email, string phone, int store_id)
        {
            if (ModelState.IsValid)
            {
                var newStaff = new staffs
                {
                    first_name = first_name,
                    last_name = last_name,
                    email = email,
                    phone = phone,
                    store_id = store_id,
                    active = 1
                };

                db.staffs.Add(newStaff);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Staff member created successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please check the form for errors.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(string first_name, string last_name, string email, string phone, string city, string state)
        {
            if (ModelState.IsValid)
            {
                var newCustomer = new customers
                {
                    first_name = first_name,
                    last_name = last_name,
                    email = email,
                    phone = phone,
                    city = city,
                    state = state
                };

                db.customers.Add(newCustomer);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please check the form for errors.";
            return RedirectToAction("Index");
        }

       
    }
}