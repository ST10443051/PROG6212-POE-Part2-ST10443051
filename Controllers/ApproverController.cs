using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class ApproverController : Controller
    {
        private List<Approver> _approvers = new List<Approver>();

        string _filePathApp = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Approvers.json");

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");

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

        public IActionResult Functions()
        {
            return View();
        }

        public IActionResult PendingClaims()
        {
            return View();
        }
    }
}
