namespace CMCS.Models
{
    public class Claim
    {
        public int claimID {  get; set; }//ID of claims for lecturer profile
        public string notes {  get; set; }
        public int hoursWorked { get; set; }
        public int hourlyRate { get; set; }
        public string status { get; set; } = "Pending";//Status (Pending, Verified, or Declined) of claims for lecturer profile
        public DateTime date { get; set; }//Date (and time) of claims for lecturer profile
        public int lecturerID { get; set; }
        public List<SupportDocument> SupportDocumentIDs { get; set; } = new List<SupportDocument>();//List for the ability to have multiples support documents tied to a one specific Claim
    }
}
