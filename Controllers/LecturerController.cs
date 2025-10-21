using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class LecturerController : Controller
    {
        private List<Lecturer> _lecturers = new List<Lecturer>();

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");

        //GET: Index
        public IActionResult Index()
        {
            if (System.IO.File.Exists(_filePath))
            {
                string json = System.IO.File.ReadAllText(_filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();
            }

            return View(_lecturers);
        }

        //GET: Create
        public IActionResult Create()
        {
            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(Lecturer lecturer)
        {

            string directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!ModelState.IsValid) 
            return View(lecturer);

            if (System.IO.File.Exists(_filePath))
            {
                string json = System.IO.File.ReadAllText(_filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();
            }
            else
            {
                _lecturers = new List<Lecturer>();
            }

            lecturer.LecturerID = _lecturers.Count > 0 ? _lecturers.Max(l => l.LecturerID) + 1 : 1;
            lecturer.claimIDs = new List<Claim>();

            _lecturers.Add(lecturer);

            string updatedJson = JsonSerializer.Serialize(_lecturers, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePath, updatedJson);

            return RedirectToAction("Index");
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId)
        {
            if (!System.IO.File.Exists(_filePath))
            {
                return NotFound();
            }

            string json = System.IO.File.ReadAllText(_filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();

            var lecturer = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int lecturerId)
        {
            string directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!System.IO.File.Exists(_filePath))
            {
                return RedirectToAction("Index");
            }

            string json = System.IO.File.ReadAllText(_filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();

            var lecturerToRemove = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturerToRemove != null)
            {
                lecturers.Remove(lecturerToRemove);

                string updatedJson = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_filePath, updatedJson);
            }

            return RedirectToAction("Index");
        }
    }
}
