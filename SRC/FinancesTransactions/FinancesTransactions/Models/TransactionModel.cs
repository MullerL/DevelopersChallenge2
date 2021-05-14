using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinancesTransactions.Models
{
    public class TransactionModel
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "varchar(6)")]
        public string Type { get; set; }
        
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }
        
        [Column(TypeName = "float")]
        public double Amount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Memo { get; set; }

    }
}
