using System.ComponentModel.DataAnnotations;

namespace CP_FleetDataJob
{
    public class FMSRecords
    {

        [Key]
        public int Id { get; set; }
        public string? DOC_HEADER { get; set; }
        public string? DOC_NUMBER { get; set; }
        public string? POSTING_DATE { get; set; }
        public string? REFERENCE_NO { get; set; }
        public string? DOCUMENT_DATE { get; set; }
        public string? INVOICE_AMOUNT { get; set; }
        public int? LoadedBy { get; set; }
        public DateTime? LoadedOn { get; set; } = DateTime.Now;
    }
}
