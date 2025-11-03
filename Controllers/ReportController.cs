using HW03.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HW03.Controllers
{
    public class ReportController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        //GET: Report
        public async Task<ActionResult> Index(DateTime? startDate, DateTime? endDate)//checks whether the the date is missign if so, it sets a default date being the last 30 days 
        {
           
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var staffPerformance = await GetStaffPerformanceReportAsync(startDate.Value, endDate.Value);// fetched the details

            var viewModel = new ReportViewModel//works on the report view model which will hand it to the view 
            {
                StaffPerformance = staffPerformance,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                SavedReports = GetSavedReports()
            };

            return View(viewModel);
        }

        // POST: Generate Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                TempData["ErrorMessage"] = "Start date cannot be after end date.";
                return RedirectToAction("Index");
            }

            var staffPerformance = await GetStaffPerformanceReportAsync(startDate, endDate);

            var viewModel = new ReportViewModel
            {
                StaffPerformance = staffPerformance,
                StartDate = startDate,
                EndDate = endDate,
                SavedReports = GetSavedReports()
            };

            return View("Index", viewModel);
        }//fetches the correct date, also refreshes the screen with new data

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> SaveReport(string fileName, string fileType, string reportDescription, DateTime startDate, DateTime endDate)

        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["ErrorMessage"] = "Please enter a file name.";
                    return RedirectToAction("Index", new { startDate, endDate });
                }

                //create the reports directory if it doesn't exist
                var reportsPath = Server.MapPath("~/App_Data/Reports/");
                if (!Directory.Exists(reportsPath))
                    Directory.CreateDirectory(reportsPath);

                // it  creates a simple text file representing the PDF
                var filePath = Path.Combine(reportsPath, $"{fileName}.{fileType.ToLower()}");

                //The report information
                var reportContent = $"STAFF PERFORMANCE REPORT\n";
                reportContent += $"========================\n";
                reportContent += $"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}\n";
                reportContent += $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}\n";
                reportContent += $"Description: {reportDescription}\n\n";

                reportContent += "STAFF PERFORMANCE RANKING:\n";
                reportContent += "==========================\n";

                var staffData = await GetStaffPerformanceReportAsync(startDate, endDate);

                int rank = 1;
                foreach (var staff in staffData)
                {
                    reportContent += $"{rank}. {staff.StaffName}\n";
                    reportContent += $"   Total Orders: {staff.TotalOrders}\n";
                    reportContent += $"   Total Sales: ${staff.TotalSales:F2}\n";
                    reportContent += $"   Units Sold: {staff.UnitsSold}\n\n";
                    rank++;
                }

                System.IO.File.WriteAllText(filePath, reportContent);

                // separate description for the bonus marks section
                if (!string.IsNullOrEmpty(reportDescription))
                {
                    var descPath = Path.Combine(reportsPath, $"{fileName}_description.txt");
                    System.IO.File.WriteAllText(descPath, reportDescription);
                }

                TempData["SuccessMessage"] = $"Report '{fileName}' saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error saving report: {ex.Message}";
            }

            return RedirectToAction("Index", new { startDate, endDate });
        }

        //download report-archive section, when the save file is clcks , it fetches the file from the server and downloads it 
        public ActionResult DownloadReport(string fileName)
        {
            var filePath = Server.MapPath("~/App_Data/Reports/" + fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(Path.GetExtension(filePath));
                return File(fileBytes, contentType, fileName);
            }

            TempData["ErrorMessage"] = "File not found.";
            return RedirectToAction("Index");
        }

        // Delete Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReport(string fileName)
        {
            try
            {
                var filePath = Server.MapPath("~/App_Data/Reports/" + fileName);
                var descPath = Server.MapPath("~/App_Data/Reports/" + Path.GetFileNameWithoutExtension(fileName) + "_description.txt");

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                if (System.IO.File.Exists(descPath))
                    System.IO.File.Delete(descPath);

                TempData["SuccessMessage"] = $"Report '{fileName}' deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting report: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        //method for staff performance data
        private async Task<List<StaffPerformanceViewModel>> GetStaffPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            return await db.orders
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .GroupBy(o => new { o.staffs.staff_id, o.staffs.first_name, o.staffs.last_name })
                .Select(g => new StaffPerformanceViewModel
                {
                    StaffId = g.Key.staff_id,
                    StaffName = g.Key.first_name + " " + g.Key.last_name,
                    TotalOrders = g.Count(),
                    TotalSales = g.Sum(o => o.order_items.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount))),
                    UnitsSold = g.Sum(o => o.order_items.Sum(oi => oi.quantity))
                })
                .OrderByDescending(s => s.TotalSales)
                .ToListAsync();
        }//get the orders between the dates, group them by the staff member , count how many orders each person has and the items the sold, total amount they earned. Sort them from the highest to the lowest

                // Looks for the reports folder, finds all the saved files then displays them in the archive 
        private List<SavedReportViewModel> GetSavedReports()
        {
            var reportsPath = Server.MapPath("~/App_Data/Reports/");
            if (!Directory.Exists(reportsPath))
                return new List<SavedReportViewModel>();

            var files = Directory.GetFiles(reportsPath)
                .Where(f => !f.EndsWith("_description.txt"))
                .Select(f => new SavedReportViewModel
                {
                    FileName = Path.GetFileName(f),
                    FileType = Path.GetExtension(f).TrimStart('.'),
                    CreatedDate = System.IO.File.GetCreationTime(f),
                    Description = GetReportDescription(Path.GetFileNameWithoutExtension(f))
                })
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            return files;
        }

        private string GetReportDescription(string fileName)
        {
            var descPath = Server.MapPath($"~/App_Data/Reports/{fileName}_description.txt");
            return System.IO.File.Exists(descPath) ? System.IO.File.ReadAllText(descPath) : string.Empty;
        }

        private string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".txt":
                    return "text/plain";
                case ".csv":
                    return "text/csv";
                default:
                    return "application/octet-stream";
            }
        }

    }
}