using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class ApproverController : Controller
    {
        private List<Approver> _approvers = new List<Approver>();

        private List<Lecturer> _lecturers = new List<Lecturer>();

        string _filePathApp = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Approvers.json");

        string _filePath;

        public ApproverController()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");
        }

        public ApproverController(string filePath)//FOR TESTING
        {
            _filePath = filePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");
        }

        public List<Lecturer> LoadLecturers()
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

        public void SaveLecturers(List<Lecturer> lecturers)
        {
            var json = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePath, json);
        }

        //GET: Index
        public IActionResult Index()
        {
            if (System.IO.File.Exists(_filePathApp))
            {
                string json = System.IO.File.ReadAllText(_filePathApp);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();
            }

            return View(_approvers);
        }

        //GET: Create
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string>
            {
                "Programme Coordinator",
                "Academic Manager"
            };

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(Approver approver)
        {

            string directory = Path.GetDirectoryName(_filePathApp);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!ModelState.IsValid)
                return View(approver);

            if (System.IO.File.Exists(_filePathApp))
            {
                string json = System.IO.File.ReadAllText(_filePathApp);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();
            }
            else
            {
                _approvers = new List<Approver>();
            }

            approver.ApproverID = _approvers.Count > 0 ? _approvers.Max(a => a.ApproverID) + 1 : 1;

            _approvers.Add(approver);

            string updatedJson = JsonSerializer.Serialize(_approvers, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePathApp, updatedJson);

            return RedirectToAction("Index");
        }

        //GET: Delete
        public IActionResult Delete(int approverId)
        {
            if (!System.IO.File.Exists(_filePathApp))
            {
                return NotFound();
            }

            string json = System.IO.File.ReadAllText(_filePathApp);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();

            var approver = approvers.FirstOrDefault(a => a.ApproverID == approverId);
            if (approver == null)
            {
                return NotFound();
            }

            return View(approver);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int approverId)
        {
            if (!System.IO.File.Exists(_filePath))
            {
                return RedirectToAction("Index");
            }

            string json = System.IO.File.ReadAllText(_filePathApp);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();

            var approverToRemove = approvers.FirstOrDefault(a => a.ApproverID == approverId);
            if (approverToRemove != null)
            {
                approvers.Remove(approverToRemove);

                string updatedJson = JsonSerializer.Serialize(approvers, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_filePathApp, updatedJson);
            }

            return RedirectToAction("Index");
        }


        //GET: retrieves all claims (from all lecturers) that have the "Pending" status
        public IActionResult PendingClaims()
        {
            var lecturers = LoadLecturers();

            var pendingClaims = lecturers.SelectMany(l => l.claimIDs.Select(c => new //(OpenAI, 2025)
            {
                LecturerID = l.LecturerID,
                LecturerName = l.name,
                ClaimID = c.claimID,
                Notes = c.notes,
                HoursWorked = c.hoursWorked,
                HourlyRate = c.hourlyRate,
                Amount = c.hoursWorked * c.hourlyRate,
                Status = c.status
            }))
            .Where(c => c.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            .ToList();

            return View(pendingClaims);
        }

        //POST: updates specific claim status from specific lecturer to "Approved"
        [HttpPost]
        public IActionResult ApproveClaim(int lecturerId, int claimId)
        {
            var lecturers = LoadLecturers();

            var lecturer = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claim = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);
            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            claim.status = "Approved";
            SaveLecturers(lecturers);

            TempData["Message"] = $"Latest action: Claim #{claimId} approved!";

            return RedirectToAction("PendingClaims");
        }

        //POST: updates specific claim status from specific lecturer to "Declined"
        [HttpPost]
        public IActionResult DeclineClaim(int lecturerId, int claimId)
        {
            var lecturers = LoadLecturers();

            var lecturer = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claim = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);
            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            claim.status = "Declined";
            SaveLecturers(lecturers);

            TempData["Message"] = $"Latest action: Claim #{claimId} declined!";

            return RedirectToAction("PendingClaims");
        }
    }
}
