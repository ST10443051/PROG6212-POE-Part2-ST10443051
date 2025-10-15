using System.Security.Claims;

namespace CMCS.Models
{
    public class Lecturer
    {
        public int LecturerID {get; set; }//ID of lecturer profile
        public string name { get; set; }//Full name of lecturer profile
        public string email { get; set; }//Email of lecturer profile
        public List<Claim> claimIDs { get; set; } = new List<Claim>();//List for the ability to have multiples Claims tied to a one specific lecturer profile
    }
}
