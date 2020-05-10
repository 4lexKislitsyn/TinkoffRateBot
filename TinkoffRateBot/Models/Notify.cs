using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.Models
{
    public class Notify
    {
        [Required]
        public string Message { get; set; }
        public long? ChatId { get; set; }
    }
}
