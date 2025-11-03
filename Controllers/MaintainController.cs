using HW03.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HW03.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Maintain
        public async Task<ActionResult> Index(int? brandFilter, int? categoryFilter)
        {
           //get the staff details 
            var staffs = await db.staffs
                .Include(s => s.stores)
                .Include(s => s.staffs2)//join it with the stors details 
                .Select(s => new StaffMaintainViewModel
                {
                    staff_id = s.staff_id,
                    first_name = s.first_name,
                    last_name = s.last_name,
                    email = s.email,
                    phone = s.phone,
                    active = s.active,
                    StoreName = s.stores.store_name,
                    ManagerName = s.staffs2 != null ? s.staffs2.first_name + " " + s.staffs2.last_name : "No Manager"
                })
                .ToListAsync();

            //get the customer detilas 
            var customers = await db.customers
                .Select(c => new CustomerMaintainViewModel
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

            //get the product details 
            var productsQuery = db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .AsQueryable();
            //applyy the filters 
            if (brandFilter.HasValue)
                productsQuery = productsQuery.Where(p => p.brand_id == brandFilter.Value);
            if (categoryFilter.HasValue)
                productsQuery = productsQuery.Where(p => p.category_id == categoryFilter.Value);

            var products = await productsQuery
                .Select(p => new ProductMaintainViewModel
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    brand_name = p.brands.brand_name,
                    category_name = p.categories.category_name,
                    model_year = p.model_year,
                    list_price = p.list_price,
                    ImagePath = "~/Content/Images/" +
                               p.brands.brand_name.Replace(" ", "_") + "_" +
                               p.categories.category_name.Replace(" ", "_") + ".jpeg"//image section
                })
                .ToListAsync();

            //dropdown sections
            ViewBag.Brands = new SelectList(await db.brands.ToListAsync(), "brand_id", "brand_name", brandFilter);
            ViewBag.Categories = new SelectList(await db.categories.ToListAsync(), "category_id", "category_name", categoryFilter);
            ViewBag.Stores = new SelectList(await db.stores.ToListAsync(), "store_id", "store_name");

            var viewModel = new MaintainViewModel
            {
                Staffs = staffs,
                Customers = customers,
                Products = products,
                SelectedBrandId = brandFilter,
                SelectedCategoryId = categoryFilter
            };

            return View(viewModel);
        }
    }
}