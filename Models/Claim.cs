namespace CMCS.Models
{
    public class Claim
    {
        public string claimID {  get; set; }//ID of claims for lecturer profile
        public DateTime date { get; set; }//Date (and time) of claims for lecturer profile
        public double amount { get; set; }//Amount (money) of claims for lecturer profile
        public string status { get; set; }//Status (Pending, Verified, or Declined) of claims for lecturer profile
        public string lecturerID { get; set; }
        public List<SupportDocument> SupportDocumentIDs { get; set; }//List for the ability to have multiples support documents tied to a one specific Claim
    }
}
