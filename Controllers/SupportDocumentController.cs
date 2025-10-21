using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class SupportDocumentController : Controller
    {
        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");

        string supportPath = Path.Combine(Directory.GetCurrentDirectory(), "SupportDocuments");

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


        public IActionResult Index(int lecturerId, int claimId)
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

            ViewBag.LecturerID = lecturer.LecturerID;
            ViewBag.ClaimID = claim.claimID;

            ViewBag.LecturerName = lecturer.name;

            return View(claim.SupportDocumentIDs);
        }

        //GET: Create
        public IActionResult Create(int lecturerId, int claimId)
        {
            ViewBag.LecturerID = lecturerId;
            ViewBag.ClaimID = claimId;

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(int lecturerId, int claimId, IFormFile uploadedFile)
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

            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only PDF, DOCX, and XLSX files are allowed.");
            }

            if (!Directory.Exists(supportPath))
                Directory.CreateDirectory(supportPath);

            string fileName = Path.GetFileName(uploadedFile.FileName);
            string filePath = Path.Combine(supportPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadedFile.CopyTo(stream);
            }

            var doc = new SupportDocument
            {
                supportDocumentID = claim.SupportDocumentIDs.Any() ? claim.SupportDocumentIDs.Max(d => d.supportDocumentID) + 1 : 1,
                fileName = Path.GetFileNameWithoutExtension(fileName),
                fileType = fileExtension.TrimStart('.'),
                filepath = filePath,
                uploadDate = DateTime.Now,
                claimID = claimId
            };

            claim.SupportDocumentIDs.Add(doc);

            SaveLecturers(load);

            return RedirectToAction("Index", new { lecturerId, claimId });
        }

        public IActionResult ViewDocument(int lecturerId, int claimId, int supportDocumentId)
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


            var doc = claim.SupportDocumentIDs.FirstOrDefault(d => d.supportDocumentID == supportDocumentId);
            if (doc == null)
            {
                return NotFound("Document not found");
            }

            if (!Directory.Exists(supportPath))
                Directory.CreateDirectory(supportPath);

            string contentType = doc.fileType.ToLower() switch //(OpenAI, 2025)
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return PhysicalFile(doc.filepath, contentType, doc.fileName + "." + doc.fileType);
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId, int claimId, int supportDocumentId)
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

            var doc = claim.SupportDocumentIDs.FirstOrDefault(d => d.supportDocumentID == supportDocumentId);
            if (doc == null)
            {
                return NotFound("Support Document not found");
            }

            ViewBag.LecturerID = lecturerId;
            ViewBag.ClaimID = claimId;
            ViewBag.SupportDocumentID = supportDocumentId;

            return View(doc);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int lecturerId, int claimId, int supportDocumentId)
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

            var docToRemove = claim.SupportDocumentIDs.FirstOrDefault(d => d.supportDocumentID == supportDocumentId);
            if (docToRemove == null)
            {
                return NotFound("Support Document not found");
            }

            if (System.IO.File.Exists(docToRemove.filepath))//(OpenAI, 2025)
            {
                System.IO.File.Delete(docToRemove.filepath);
            };

            claim.SupportDocumentIDs.Remove(docToRemove);

            SaveLecturers(load);

            return RedirectToAction("Index", new { lecturerId, claimId });
        }
    }
}
