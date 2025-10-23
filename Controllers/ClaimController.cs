using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class ClaimController : Controller
    {

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
        public IActionResult Index(int lecturerId)//Displays all claims for specific lecturer in the system
        {
            var load = LoadLecturers();//Calls method to retrieve all lecturers from JSON file
            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);//Finds the lecturer matching the LecturerID

            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            ViewBag.LecturerName = lecturer.name;//Sets the found lecturer's name to be displayed in the index view
            ViewBag.LecturerID = lecturer.LecturerID;//Sets the found lecturer's ID to be displayed in the index view

            return View(lecturer.claimIDs);//Returns a view displaying all claims for this lecturer
        }

        //GET: Create
        public IActionResult Create(int lecturerId)//Displays the form to add claim for the specific lecturer
        {
            ViewBag.LecturerID = lecturerId;

            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(int lecturerId, Claim claim)//Handles the creation of a new claim for a specific lecturer
        {
            var load = LoadLecturers();
            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);

            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }


            claim.claimID = lecturer.claimIDs.Any() ? lecturer.claimIDs.Max(c => c.claimID) + 1 : 1;//Assigns a unique claim ID (incrementing based on existing claims) (OpenAI, 2025)
            //Note
            //HoursWorked
            //HourlyRate
            //Status = "Pending" by default
            claim.date = DateTime.Now;//Automatically set claim submission (current time)
            claim.lecturerID = lecturerId;

            lecturer.claimIDs.Add(claim);//Adds new claim to lecturer’s list

            SaveLecturers(load);//Saves updated lecturer list back to the JSON file

            return RedirectToAction("Index", new { lecturerId });//Redirects back to the lecturer's claim list
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId, int claimId)//Displays confirmation page before deleting a claim
        {   
            var load = LoadLecturers();

            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claim = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);//Finds the specific claim by its LecturerID
            if (claim == null)
            {
                return NotFound("Claim not found");
            }

            ViewBag.LecturerID = lecturerId;
            ViewBag.ClaimID = claimId;

            return View(claim);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST (int lecturerId, int claimId)//Permanently deletes a claim from the lecturer’s record.
        {
            var load = LoadLecturers();

            var lecturer = load.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found");
            }

            var claimToRemove = lecturer.claimIDs.FirstOrDefault(c => c.claimID == claimId);//Finds the specific claim by its ClaimID so it can be removed
            if (claimToRemove == null)
            {
                return NotFound("Claim not found");
            }

            lecturer.claimIDs.Remove(claimToRemove);//Removes the claim from lecturer’s list

            SaveLecturers(load);//Saves changes back to the JSON file

            return RedirectToAction("Index", new { lecturerId });   
        }
    }
}