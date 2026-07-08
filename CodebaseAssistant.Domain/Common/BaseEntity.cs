using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodebaseAssistant.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id
        {
            get;set;
        }
        public DateTime CreatedAt { get; set; }= DateTime.Now;

        public DateTime UpdatedAt { get; set; }

        // public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow
    }
}
