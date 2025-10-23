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

        string _filePathApp = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Approvers.json");//File path to the JSON file where approver data is persisted (StackOverflow, no date) 

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");//File path to the JSON file where lecturer data is persisted (StackOverflow, no date) 


        public List<Lecturer> LoadLecturers()//Returns list of Lecturer objects loaded from the JSON file.
        { 
            if (!System.IO.File.Exists(_filePath))//Checks if the file exists before reading
            {
                return new List<Lecturer>();
            }

            string json = System.IO.File.ReadAllText(_filePath);//Reads the JSON file content
            var options = new JsonSerializerOptions //Allows case-insensitive property matching when deserialising (OpenAI, 2025)
            { 
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();//Deserialises the JSON data into a list of Lecturer objects
        }

        public void SaveLecturers(List<Lecturer> lecturers)//Updates and saves the list of lecturers to be serialised
        {
            var json = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });//Serialises and saves the updated lecturer list to JSON
            System.IO.File.WriteAllText(_filePath, json);
        }

        //GET: Index
        public IActionResult Index()//Displays all claims for a specific lecturer
        {
            if (System.IO.File.Exists(_filePathApp))//Checks if the file exists before reading
            {
                string json = System.IO.File.ReadAllText(_filePathApp);//Reads the JSON file content

                var options = new JsonSerializerOptions //Allows case-insensitive property matching when deserialising (OpenAI, 2025)
                {
                    PropertyNameCaseInsensitive = true
                };

                _approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();//Deserialises the JSON data into a list of Lecturer objects
            }

            return View(_approvers);
        }

        //GET: Create
        public IActionResult Create()//Displays the form to add a new approver
        {
            ViewBag.Roles = new List<string> //Creates a preloaded set of roles (Programme Coordinator and Academic Manager)
            {
                "Programme Coordinator",
                "Academic Manager"
            };

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(Approver approver)//Handles submission of a new approver record
        {

            string directory = Path.GetDirectoryName(_filePathApp);//Ensures the directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!ModelState.IsValid)
                return View(approver);

            if (System.IO.File.Exists(_filePathApp))//Loads existing lecturers from file if it exists
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

            approver.ApproverID = _approvers.Count > 0 ? _approvers.Max(a => a.ApproverID) + 1 : 1;//Assigns a new unique ApproverID (auto-increment) (OpenAI, 2025)

            _approvers.Add(approver);//Adds the new lecturer to the approver list


            string updatedJson = JsonSerializer.Serialize(_approvers, new JsonSerializerOptions { WriteIndented = true });//Serialises and saves the updated approver list to JSON
            System.IO.File.WriteAllText(_filePathApp, updatedJson);

            return RedirectToAction("Index");//Redirects back to Index page
        }

        //GET: Delete
        public IActionResult Delete(int approverId)//Displays the delete confirmation page for a specific approver
        {
            if (!System.IO.File.Exists(_filePathApp))//Returns not found, if file doesn't exist
            {
                return NotFound();
            }

            string json = System.IO.File.ReadAllText(_filePathApp);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();

            var approver = approvers.FirstOrDefault(a => a.ApproverID == approverId);//Finds the approver matching the ApproverID
            if (approver == null)
            {
                return NotFound();
            }

            return View(approver);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]//(OpenAI, 2024)
        public IActionResult DeletePOST(int approverId)//Permanently deletes the approver record from the system
        {
            if (!System.IO.File.Exists(_filePath))//If file does not exist, return to Index page
            {
                return RedirectToAction("Index");
            }

            string json = System.IO.File.ReadAllText(_filePathApp);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var approvers = JsonSerializer.Deserialize<List<Approver>>(json, options) ?? new List<Approver>();

            var approverToRemove = approvers.FirstOrDefault(a => a.ApproverID == approverId);//Finds the approver to remove by ApproverID  to delete
            if (approverToRemove != null)
            {
                approvers.Remove(approverToRemove);//Removes the approver and saves the updated list (OpenAI, 2025)

                string updatedJson = JsonSerializer.Serialize(approvers, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_filePathApp, updatedJson);
            }

            return RedirectToAction("Index");
        }


        //GET: 
        public IActionResult PendingClaims()//retrieves all claims (from all lecturers) that have the "Pending" status
        {
            var lecturers = LoadLecturers();

            var pendingClaims = lecturers.SelectMany(l => l.claimIDs.Select(c => new // // Flatten lecturer + claim data for display (OpenAI, 2025)
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
            .ToList();//Filters to only with status "pending"

            return View(pendingClaims);
        }

        //POST: 
        [HttpPost]
        public IActionResult ApproveClaim(int lecturerId, int claimId)//updates specific claim status from specific lecturer to "Approved"
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

            claim.status = "Approved";//Changes status from "Pending" to "Approved"
            SaveLecturers(lecturers);

            TempData["Message"] = $"Latest action: Claim #{claimId} approved!";//Temp message notifying approver that the status as succecfully changed to "Approved"

            return RedirectToAction("PendingClaims");
        }

        //POST: 
        [HttpPost]
        public IActionResult DeclineClaim(int lecturerId, int claimId)//updates specific claim status from specific lecturer to "Declined"
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

            claim.status = "Declined";//Changes status from "Pending" to "Declined"
            SaveLecturers(lecturers);

            TempData["Message"] = $"Latest action: Claim #{claimId} declined!";//Temp message notifying approver that the status as succecfully changed to "Declined"


            return RedirectToAction("PendingClaims");
        }
    }
}
