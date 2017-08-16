using System;

namespace VisistsRecordsFilesProcessor
{
    internal class WebsiteVisitRecord
    {
        public string Website { get; set; }
        public DateTime Date { get; set; }
        public int VisitsCount { get; set; }

        public WebsiteVisitRecord(string website, DateTime date, int visits)
        {
            Website = website;
            Date = date;
            VisitsCount = visits;
        }

        public override string ToString()
        {
            return $"{Website}|{Date.ToShortDateString()}|{VisitsCount}";
        }
    }
}
