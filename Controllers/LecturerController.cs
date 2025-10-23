using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CMCS.Controllers
{
    public class LecturerController : Controller
    {
        private List<Lecturer> _lecturers = new List<Lecturer>();//In-memory list to store lecturer data temporarily

        string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Lecturers.json");//File path to the JSON file where lecturer data is persisted (StackOverflow, no date) 

        //GET: Index
        public IActionResult Index()//Displays all lecturers in the system
        {
            if (System.IO.File.Exists(_filePath))//Checks if the file exists before reading
            {
                string json = System.IO.File.ReadAllText(_filePath);//Reads the JSON file content

                var options = new JsonSerializerOptions //Allows case-insensitive property matching when deserialising (OpenAI, 2025)
                {
                    PropertyNameCaseInsensitive = true
                };

                _lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();//Deserialises the JSON data into a list of Lecturer objects
            }

            return View(_lecturers);
        }

        //GET: Create
        public IActionResult Create()//Displays the form to add a new lecturer
        {
            return View();
        }

        //POST: Create
        [HttpPost]
        public IActionResult Create(Lecturer lecturer)//Handles submission of a new lecturer record
        {

            string directory = Path.GetDirectoryName(_filePath);//Ensures the directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!ModelState.IsValid)
            return View(lecturer);

            if (System.IO.File.Exists(_filePath))//Loads existing lecturers from file if it exists
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

            lecturer.LecturerID = _lecturers.Count > 0 ? _lecturers.Max(l => l.LecturerID) + 1 : 1;//Assigns a new unique LecturerID (auto-increment) (OpenAI, 2025)
            lecturer.claimIDs = new List<Claim>();//Initialises the lecturer’s claim list as empty

            _lecturers.Add(lecturer);//Adds the new lecturer to the lecturer list

            string updatedJson = JsonSerializer.Serialize(_lecturers, new JsonSerializerOptions { WriteIndented = true });//Serialises and saves the updated lecturer list to JSON
            System.IO.File.WriteAllText(_filePath, updatedJson);

            return RedirectToAction("Index");//Redirects back to Index page
        }

        //GET: Delete
        public IActionResult Delete(int lecturerId)//Displays the delete confirmation page for a specific lecturer
        {
            if (!System.IO.File.Exists(_filePath))//Returns not found, if file doesn't exist
            {
                return NotFound();
            }

            string json = System.IO.File.ReadAllText(_filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();

            var lecturer = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);//Finds the lecturer matching the LecturerID
            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        //POST: Delete
        [HttpPost, ActionName("Delete")]//(OpenAI, 2024)
        public IActionResult DeletePOST(int lecturerId)//Permanently deletes the lecturer record from the system
        {
            string directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!System.IO.File.Exists(_filePath))//If file does not exist, return to Index page
            {
                return RedirectToAction("Index");
            }

            string json = System.IO.File.ReadAllText(_filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json, options) ?? new List<Lecturer>();

            var lecturerToRemove = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);//Finds the lecturer to remove by LecturerID to delete
            if (lecturerToRemove != null)
            {
                lecturers.Remove(lecturerToRemove);//Removes the lecturer and saves the updated list (OpenAI, 2025)

                string updatedJson = JsonSerializer.Serialize(lecturers, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_filePath, updatedJson);
            }

            return RedirectToAction("Index");
        }
    }
}
