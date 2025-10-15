namespace CMCS.Models
{
    public class Verification
    {
        //Model will be implemented later
        public int verificationID {  get; set; }//ID of verification
        public DateTime verificationDate { get; set; }//Date (and time) when claim was verified
        public string result {  get; set; }//Can be either Verified, Delinced, or Pending
    }
}
