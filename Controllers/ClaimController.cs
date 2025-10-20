using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class ClaimController : Controller
    {
        //private List<Lecturer> _lecturers = new List<Lecturer>();
        //private List<Claim> _claims = new List<Claim>();

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");

        private List<Lecturer> LoadLecturers()
        {
            if (!System.IO.File.Exists(_filePath))
            {
                return new List<Lecturer>();
            }

            string json = System.IO.File.ReadAllText(_filePath);
            var options = new JsonSerializerOptions
            { 
                PropertyNameCaseInsensitive = true 
            };

            return JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();
        }

        private void SaveLecturers(List<Lecturer> lecturers)
        {
            var json = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePath, json);
        }

        //GET: Index
        public IActionResult Index(int lecturerId)
        {
            var load = LoadLecturers();
            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);
            
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            ViewBag.LecturerName = lecturer.name;
            ViewBag.LecturerID = lecturer.LecturerID;

            return View(lecturer.claimIDs);
        }

        //GET: Create
        public IActionResult Create(int lecturerId)
        {
            ViewBag.LecturerID = lecturerId;

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(int lecturerId, Claim claim)
        {
            var load = LoadLecturers();
            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);

            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }


            claim.claimID = lecturer.claimIDs.Any() ? lecturer.claimIDs.Max(c => c.claimID) + 1 : 1;
            //Note
            //HoursWorked
            //HourlyRate
            //Status = "Pending" by default
            claim.date = DateTime.Now;
            claim.lecturerID = lecturerId;

            lecturer.claimIDs.Add(claim);

            SaveLecturers(load);

            return RedirectToAction("Index", new { lecturerId });
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId, int claimId)
        {   
            var load = LoadLecturers();

            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claim = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);
            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            ViewBag.LecturerID = lecturerId;
            ViewBag.LecturerName = lecturer.name;

            return View(claim);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST (int lecturerId, int claimId)
        {
            var load = LoadLecturers();

            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claimToRemove = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);
            if (claimToRemove == null)
            {
                return NotFound("Claim not found");
            }

            lecturer.claimIDs.Remove(claimToRemove);

            SaveLecturers(load);

            return RedirectToAction("Index", new { lecturerId });   
        }
    }
}
