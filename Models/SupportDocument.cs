using System.Globalization;

namespace CMCS.Models
{
    public class SupportDocument
    {
        public int supportDocumentID {  get; set; }//ID of support document

        public string fileName { get; set; }//File name of support document

        public string fileType { get; set; }//File type of support document

        public DateTime uploadDate { get; set; }//Date (and time) when the support document was uploaded
    }
}
