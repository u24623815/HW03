using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HW03.Models
{
    public class MaintainViewModel
    {
        public List<StaffMaintainViewModel> Staffs { get; set; }
        public List<CustomerMaintainViewModel> Customers { get; set; }
        public List<ProductMaintainViewModel> Products { get; set; }

      
        public int? SelectedBrandId { get; set; }
        public int? SelectedCategoryId { get; set; }
    }

    public class StaffMaintainViewModel
    {
        public int staff_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public byte active { get; set; }
        public string StoreName { get; set; }
        public string ManagerName { get; set; }
    }

    public class CustomerMaintainViewModel
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

    public class ProductMaintainViewModel
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string brand_name { get; set; }
        public string category_name { get; set; }
        public int model_year { get; set; }
        public decimal list_price { get; set; }

        public string ImagePath { get; set; }
    }

}