namespace CMCS.Models
{
    public class Approver
    {
        public int approverID { get; set; }//ID of approver profile
        public string name { get; set; }//Name of approver profile
        public string role { get; set; }//Role (Project Coordinator or Academic Manager) of approver profile
    }
}
