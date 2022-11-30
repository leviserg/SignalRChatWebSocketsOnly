using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ChatMessage
    {
        public string? Command { get; set; }
        public string? Sender { get; set; }
        public string? Body { get; set; }
        public string? CreatedAt { get; set; }
    }
}
