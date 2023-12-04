using Inventory.Models;
using Inventory.ViewModels;
using Inventory.Services;
using Microsoft.AspNetCore.Mvc;
using Inventory.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;

namespace Inventory.Controllers
{
    // InventoryController: Controller class handling inventory-related operations.
    public class InventoryController : Controller
    {
        private InventoryDAO _inventoryDAO; // Data access object for inventory operations
        private readonly ILogger<InventoryController> _logger; // Logger for logging

        // Constructor initializes the DAO and Logger
        public InventoryController(ILogger<InventoryController> logger)
        {
            _inventoryDAO = new InventoryDAO();
            _logger = logger;
        }

        // Displays the main inventory list page
        [HttpGet]
        [CustomAuthorization] // Custom authorization to ensure only authorized users access this method
        public IActionResult Index()
        {
            // Initializes an empty ViewModel for the view
            var viewModel = new InventoryViewModel
            {
                Items = new List<ItemModel>(), // Initially empty list of items
                CurrentPage = 1, // Default to the first page
                TotalItems = 0, // No items initially
                ItemsPerPage = 50 // Default number of items per page
            };

            return View(viewModel); // Passes the ViewModel to the view
        }

        // Fetches a partial list of inventory items for display
        public ActionResult InventoryList(string searchTerm, int? itemId, int pageNumber = 1, int itemsPerPage = 50)
        {
            // Fetch total items and items for the current page based on search criteria
            var totalItems = _inventoryDAO.CountItems(searchTerm, itemId);
            var items = _inventoryDAO.SearchForItem(searchTerm, itemId, pageNumber, itemsPerPage);

            var viewModel = new InventoryViewModel
            {
                Items = items, // List of items for the current page
                CurrentPage = pageNumber, // Current page number
                TotalItems = totalItems, // Total number of items matching the search
                ItemsPerPage = itemsPerPage // Number of items per page
            };

            return PartialView("_InventoryListPartial", viewModel); // Returns a partial view for dynamic content update
        }

        // Searches the inventory based on given criteria
        [HttpGet]
        public IActionResult SearchInventory(string searchTerm, int? itemId, int pageNumber = 1, int itemsPerPage = 50)
        {
            try
            {
                // Fetches items and total count based on search criteria
                var items = _inventoryDAO.SearchForItem(searchTerm, itemId, pageNumber, itemsPerPage);
                var totalItems = _inventoryDAO.CountItems(searchTerm, itemId);

                var viewModel = new InventoryViewModel
                {
                    Items = items, // List of items for the current page
                    CurrentPage = pageNumber, // Current page number
                    TotalItems = totalItems, // Total number of items matching the search
                    ItemsPerPage = itemsPerPage // Number of items per page
                };

                return PartialView("_InventoryListPartial", viewModel); // Returns a partial view with the search results
            }
            catch (Exception ex)
            {
                _logger.LogError("SearchInventory error: {Message}", ex.Message); // Log error
                return StatusCode(500, "Internal Server Error: " + ex.Message); // Returns an error response
            }
        }

        // Displays the form to add a new inventory item
        [HttpGet]
        public IActionResult AddNewItem()
        {
            return View(); // Renders the AddNewItem view
        }

        // Processes the addition of a new inventory item
        [HttpPost]
        public IActionResult AddNewItem(ItemModel item)
        {
            // Validates the model state before proceeding
            if (ModelState.IsValid)
            {
                bool success = _inventoryDAO.AddNewItem(item); // Attempt to add the new item
                if (success)
                {
                    return RedirectToAction("Index"); // Redirects to the inventory list on success
                }
                else
                {
                    ModelState.AddModelError("", "Unable to add item."); // Adds an error message to the model state
                }
            }
            return View(item); // Returns to the same view with validation errors
        }

        // Displays the form for editing an existing inventory item
        [HttpGet]
        public IActionResult EditItem(int id)
        {
            var item = _inventoryDAO.GetItemById(id); // Retrieves the item by its ID
            if (item != null)
            {
                return View(item); // Renders the EditItem view with the item details
            }
            this.WithDanger("Index", "Item not found."); // Display an error message if item is not found
            return RedirectToAction("Index"); // Redirects to the inventory list
        }

        // Processes the update of an existing inventory item
        [HttpPost]
        public IActionResult Edit(ItemModel item)
        {
            // Validates the model state before proceeding
            if (ModelState.IsValid)
            {
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        _logger.LogError(error.ErrorMessage); // Log each error message
                    }
                }
                bool success = _inventoryDAO.UpdateItem(item); // Attempt to update the item
                if (success)
                {
                    this.WithSuccess("Index", "Item Updated."); // Display a success message
                    return RedirectToAction("Index"); // Redirects to the inventory list
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update item."); // Adds an error message to the model state
                }
            }
            return View(item); // Returns to the same view with validation errors
        }

        // Deletes an inventory item
        [HttpPost]
        public IActionResult Delete(int itemId)
        {
            bool success = _inventoryDAO.DeleteItem(itemId); // Attempt to delete the item
            if (success)
            {
                TempData["SuccessMessage"] = "Item successfully deleted."; // Display a success message
            }
            else
            {
                TempData["ErrorMessage"] = "There was a problem deleting the item."; // Display an error message
            }

            return RedirectToAction("Index"); // Redirects to the inventory list
        }

        // Generates a PDF report of the inventory
        public IActionResult GeneratePdfReport()
        {
            // Using MemoryStream for on-the-fly generation
            using (MemoryStream stream = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(stream);
                // Initialize PDF document
                PdfDocument pdf = new PdfDocument(writer);
                // Initialize document
                Document document = new Document(pdf);

                // Fetch data
                var inventoryData = _inventoryDAO.GetItemsForReport();

                // Adding content to the document
                document.Add(new Paragraph("Inventory Report"));
                document.Add(new Paragraph("Generated on " + DateTime.Now.ToString()));

                // Create a table with the appropriate column count
                Table table = new Table(9);

                // Add headers
                table.AddHeaderCell("Item ID");
                table.AddHeaderCell("Name");
                table.AddHeaderCell("Description");
                table.AddHeaderCell("Price");
                table.AddHeaderCell("Quantity");
                table.AddHeaderCell("Vendor Id");
                table.AddHeaderCell("Vendor");
                table.AddHeaderCell("Vendor Contact");
                table.AddHeaderCell("Associated Products");

                // Add data
                foreach (var item in inventoryData)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemId.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemName)));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemDescription)));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemPrice.ToString("C"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemQuantity.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemVendorId.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemVendorName.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemVendorContactDetails?.ToString() ?? "N/A")));
                    table.AddCell(new Cell().Add(new Paragraph(item.ItemVendorAssociatedProducts?.ToString() ?? "N/A")));
                }

                document.Add(table);

                // Close the document
                document.Close();

                // Convert the written MemoryStream to an array and send it as a File
                return File(stream.ToArray(), "application/pdf", "InventoryReport" + DateTime.Now.ToString() + ".pdf");
            }
        }

        // Generates an Excel report of the inventory
        public IActionResult GenerateXlsxReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory Report");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Item ID";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "Description";
                worksheet.Cell(currentRow, 4).Value = "Price";
                worksheet.Cell(currentRow, 5).Value = "Quantity";
                worksheet.Cell(currentRow, 6).Value = "Vendor Id";
                worksheet.Cell(currentRow, 7).Value = "Vendor";
                worksheet.Cell(currentRow, 8).Value = "Vendor Contact";
                worksheet.Cell(currentRow, 9).Value = "Associated Products";

                var inventoryData = _inventoryDAO.GetItemsForReport();
                foreach (var item in inventoryData)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.ItemId;
                    worksheet.Cell(currentRow, 2).Value = item.ItemName;
                    worksheet.Cell(currentRow, 3).Value = item.ItemDescription;
                    worksheet.Cell(currentRow, 4).Value = item.ItemPrice;
                    worksheet.Cell(currentRow, 5).Value = item.ItemQuantity;
                    worksheet.Cell(currentRow, 6).Value = item.ItemVendorId;
                    worksheet.Cell(currentRow, 7).Value = item.ItemVendorName;
                    worksheet.Cell(currentRow, 8).Value = item.ItemVendorContactDetails;
                    worksheet.Cell(currentRow, 9).Value = item.ItemVendorAssociatedProducts;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryReport" + DateTime.Now.ToString() + ".xlsx");
                }
            }
        }

        // Generates a CSV report of the inventory
        public IActionResult GenerateCsvReport()
        {
            var inventoryData = _inventoryDAO.GetItemsForReport();
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(inventoryData);
                streamWriter.Flush();
                return File(memoryStream.ToArray(), "text/csv", "InventoryReport" + DateTime.Now.ToString() + ".csv");
            }
        }
    }
}