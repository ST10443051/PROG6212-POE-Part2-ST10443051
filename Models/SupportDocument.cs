using System.Globalization;

namespace CMCS.Models
{
    public class SupportDocument
    {
        public int supportDocumentID {  get; set; }//ID of support document
        public string fileName { get; set; }//File name of support document
        public string fileType { get; set; }//File type of support document
        public string filepath { get; set; }//File path of support document (Use for JSON file)
        public DateTime uploadDate { get; set; }//Date (and time) when the support document was uploaded
        public int claimID { get; set; }//ClaimID that is tied this support document

    }
}
