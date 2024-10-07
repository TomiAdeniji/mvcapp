using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_itemimport")]
    public class TraderItemImport : DataModelBase
    {
        [Required]
        public virtual QbicleDomain Domain { get; set; }
        [Required]
        public virtual TraderLocation Location { get; set; }
        [Required]
        public string Spreadsheet { get; set; }
        /// <summary>
        /// Key of origin SpreadSheet uploaded to S3
        /// </summary>
        [Required]
        public string SpreadsheetKey { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }

        public ImportStatus Status { get; set; } = ImportStatus.Verifying;
        public int ItemsImported { get; set; } = 0;
        public int ItemsUpdated { get; set; } = 0;
        public int ItemsError { get; set; } = 0;
        /// <summary>
        /// While create the TraderItem, if catch error, write current item to a SpreadSheet file and upload to S3
        /// </summary>
        public string SpreadsheetErrorsKey { get; set; }
    }

    public enum ImportStatus
    {
        [Description("Verifying file format")]
        Verifying = 1,
        [Description("Uploading & updating items")]
        Uploading = 2,
        [Description("Uploaded")]
        Uploaded = 3,
        [Description("Uploaded with errors")]
        UploadedWithErrors = 4,
        [Description("File error")]
        FileError = 5
    }
}
