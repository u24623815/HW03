using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace HW03.Models
{
    public class ReportViewModel
    {
        public List<StaffPerformanceViewModel> StaffPerformance { get; set; }
        public List<SavedReportViewModel> SavedReports { get; set; }
        
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }

    public class StaffPerformanceViewModel
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int UnitsSold { get; set; }
        public int Rank { get; set; }
    }

    public class SavedReportViewModel
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
    }
}