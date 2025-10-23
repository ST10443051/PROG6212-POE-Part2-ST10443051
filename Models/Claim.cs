namespace CMCS.Models
{
    public class Claim
    {
        public int claimID {  get; set; }//ID of claims for lecturer profile
        public string notes {  get; set; }//Descriptive note for claim
        public int hoursWorked { get; set; }//How many hours was done for claim
        public int hourlyRate { get; set; }//The hourly rate for the claim (in rands)
        public string status { get; set; } = "Pending";//Status (Pending, Verified, or Declined) of claims for lecturer profile
        public DateTime date { get; set; }//Date (and time) of claims for lecturer profile
        public int lecturerID { get; set; }//LecturerID that is tied to this claim (tells you who created the claim)
        public List<SupportDocument> SupportDocumentIDs { get; set; } = new List<SupportDocument>();//List for the ability to have multiples support documents tied to a one specific Claim
    }
}
