using DatabaseQueryApp.Models;
using DatabaseQueryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseQueryApp.Controllers
{
    public class DatabaseQueryController : Controller
    {
        private readonly DatabaseService _databaseService;
        private readonly ExcelExportService _excelExportService;

        public DatabaseQueryController(DatabaseService databaseService, ExcelExportService excelExportService)
        {
            _databaseService = databaseService;
            _excelExportService = excelExportService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new DatabaseQueryModel());
        }

        [HttpPost]
        public IActionResult Clear()
        {
            // Clear all form data by returning a new model
            return View("Index", new DatabaseQueryModel());
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(DatabaseQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError("ConnectionString", "Connection string is required");
                return View("Index", model);
            }

            var (isValid, message) = await _databaseService.ValidateConnectionAsync(model.ConnectionString);
            
            model.IsConnected = isValid;
            model.ConnectionMessage = message;
            
            if (!isValid)
            {
                ModelState.AddModelError("ConnectionString", message);
            }

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteQuery(DatabaseQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError("ConnectionString", "Connection string is required");
                return View("Index", model);
            }

            if (string.IsNullOrWhiteSpace(model.SqlQuery))
            {
                ModelState.AddModelError("SqlQuery", "SQL query is required");
                return View("Index", model);
            }

            // Validate connection first
            var (isValid, connectionMessage) = await _databaseService.ValidateConnectionAsync(model.ConnectionString);
            model.IsConnected = isValid;
            model.ConnectionMessage = connectionMessage;

            if (!isValid)
            {
                ModelState.AddModelError("ConnectionString", connectionMessage);
                return View("Index", model);
            }

            // Execute query
            var (results, columns, error, isSelect, affectedRows, executionMessage, executionTime) = 
                await _databaseService.ExecuteQueryAsync(model.ConnectionString, model.SqlQuery);
            
            model.QueryResults = results;
            model.ColumnNames = columns;
            model.ErrorMessage = error;
            model.IsSelectQuery = isSelect;
            model.AffectedRows = affectedRows;
            model.ExecutionMessage = executionMessage;
            model.ExecutionTime = executionTime;

            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("SqlQuery", error);
            }

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel(string connectionString, string sqlQuery)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(sqlQuery))
            {
                return BadRequest("Connection string and SQL query are required");
            }

            var (results, columns, error, isSelect, _, _, _) = await _databaseService.ExecuteQueryAsync(connectionString, sqlQuery);
            
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(error);
            }

            if (!isSelect || !results.Any())
            {
                return BadRequest("No data available to export. Only SELECT queries with results can be exported.");
            }

            var excelData = _excelExportService.ExportToExcel(results, columns);
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                       $"QueryResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}