using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class ClaimsController : Controller
    {
        private List<Lecturer> _lecturers = new List<Lecturer>();
        private List<Claim> _claims = new List<Claim>();

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");

        public IActionResult Index(int Id)
        {
            if (System.IO.File.Exists(_filePath))
            {
                string json = System.IO.File.ReadAllText(_filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();

                var lecturer = _lecturers.FirstOrDefault(l => l.LecturerID == Id);
                if (lecturer == null)
                {
                    return NotFound($"Lecturer with ID {Id} not found.");
                }

                _claims = lecturer.claimIDs ?? new List<Claim>();
            }

            return View(_claims);
        }
        
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }
    }
}
