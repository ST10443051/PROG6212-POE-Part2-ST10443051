using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class SupportDocumentController : Controller
    {
        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");//File path to the JSON file where lecturer data is persisted (StackOverflow, no date) 

        string supportPath = Path.Combine(Directory.GetCurrentDirectory(), "SupportDocuments");//File path to the flder where support documents are stored (StackOverflow, no date) 

        private List<Lecturer> LoadLecturers()//Returns list of Lecturer objects loaded from the JSON file.
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

        private void SaveLecturers(List<Lecturer> lecturers)//Updates and saves the list of lecturers to be serialised
        {
            var json = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });//Serialises and saves the updated lecturer list to JSON
            System.IO.File.WriteAllText(_filePath, json);
        }


        public IActionResult Index(int lecturerId, int claimId)//Displays all support documents for specific claim from the specific lecturer in the system
        {
            var load = LoadLecturers();//Calls method to retrieve all lecturers from JSON file
            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);//Finds the lecturer matching the LecturerID
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claim = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);
            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            ViewBag.LecturerID = lecturer.LecturerID;//Sets the found lecturer's ID to be displayed in the index view
            ViewBag.ClaimID = claim.claimID;//Sets the found claim's ID to be displayed in the index view

            ViewBag.LecturerName = lecturer.name;

            return View(claim.SupportDocumentIDs);//Returns a view displaying all support documents for this lecturer
        }

        //GET: Create
        public IActionResult Create(int lecturerId, int claimId)//Displays the form to add support document for the specific lecturer
        {
            ViewBag.LecturerID = lecturerId;
            ViewBag.ClaimID = claimId;

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(int lecturerId, int claimId, IFormFile uploadedFile)//Handles the creation of a new support document for the specific claim form a specific lecturer
                                                                                        //and handles the uploading of the new support docuemnt to the specified folder
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

            if (uploadedFile == null || uploadedFile.Length == 0)//If uploaded file is null or the name length is 0, return a no file uploaded error
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))//Ensures that only allowed files that can be uploaded are PDF, DOCX, and XLSXs files.
            {
                return BadRequest("Only PDF, DOCX, and XLSX files are allowed.");
            }

            if (!Directory.Exists(supportPath))//Ensures the directory for uploaded files exists
                Directory.CreateDirectory(supportPath);

            //Gets file name and filepath for JSON file purposes
            string fileName = Path.GetFileName(uploadedFile.FileName);
            string filePath = Path.Combine(supportPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))//The actaul uploading portion to the directory (StackOverflow, no date)
            {
                uploadedFile.CopyTo(stream);
            }

            var doc = new SupportDocument//Creates new SupportDocuemnt to upload to JSON file
            {
                supportDocumentID = claim.SupportDocumentIDs.Any() ? claim.SupportDocumentIDs.Max(d => d.supportDocumentID) + 1 : 1,
                fileName = Path.GetFileNameWithoutExtension(fileName),
                fileType = fileExtension.TrimStart('.'),
                filepath = filePath,
                uploadDate = DateTime.Now,
                claimID = claimId
            };

            claim.SupportDocumentIDs.Add(doc);//Adds new support document to specific claim to lecturer’s list

            SaveLecturers(load);//Saves updated lecturer list back to the JSON file

            return RedirectToAction("Index", new { lecturerId, claimId });//Redirects back to the claims's support document list
        }

        public IActionResult ViewDocument(int lecturerId, int claimId, int supportDocumentId)//Allows to load and display selected support document by downloading the document to
                                                                                             //the user's local computer
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

            string contentType = doc.fileType.ToLower() switch //Determines the correct MIME (content) type based on the file extension (StackOverflow, no date) (OpenAI, 2025)
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return PhysicalFile(doc.filepath, contentType, doc.fileName + "." + doc.fileType);//Returns the physical file for download or display in browser (StackOverflow, no date)
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId, int claimId, int supportDocumentId)//Displays confirmation page before deleting a support document
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

            var doc = claim.SupportDocumentIDs.FirstOrDefault(d => d.supportDocumentID == supportDocumentId);//Finds the specific support document by its ClaimID form the specific LecturerID
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
        public IActionResult DeletePOST(int lecturerId, int claimId, int supportDocumentId)//Permanently deletes a support claim from the lecturer’s record.
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

            var docToRemove = claim.SupportDocumentIDs.FirstOrDefault(d => d.supportDocumentID == supportDocumentId);//Finds the specific support claim by its SupportDocumentID so it can be removed
            if (docToRemove == null)
            {
                return NotFound("Support Document not found");
            }

            if (System.IO.File.Exists(docToRemove.filepath))//Checks if the physical file exists before attempting to delete it(OpenAI, 2025)
            {
                System.IO.File.Delete(docToRemove.filepath);//Deletes the file from the server storage
            };

            claim.SupportDocumentIDs.Remove(docToRemove);//Removes the document reference from the lecturer's claim record

            SaveLecturers(load);

            return RedirectToAction("Index", new { lecturerId, claimId });//Redirects back to the Support Document Index view for the same lecturer and claim
        }

        public IActionResult IndexApprover(int lecturerId, int claimId)//Pretty much identical to the normal Index view, however this version is used
                                                                       //when an Approver views it and cannot delete the support document
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
    }
}
