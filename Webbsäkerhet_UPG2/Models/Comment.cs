using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Webbsäkerhet_UPG2.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        [Required (ErrorMessage = "Please enter a message")]
        public string Message { get; set; }
    }
}
