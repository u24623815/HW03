using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HW03.Models
{
    public class HomeViewModel
    {
        public List<StaffViewModel> Staffs { get; set; }
        public List<CustomerViewModel> Customers { get; set; }
        public List<ProductViewModel> Products { get; set; }
        public int? SelectedBrandId { get; set; }
        public int? SelectedCategoryId { get; set; }
    }

    public class StaffViewModel
    {
        public int staff_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public byte active { get; set; }
        public string StoreName { get; set; }
        public string ManagerName { get; set; }
        public List<SoldProductViewModel> SoldProducts { get; set; }
    }

    public class SoldProductViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
    }

    public class CustomerViewModel
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

    public class ProductViewModel
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