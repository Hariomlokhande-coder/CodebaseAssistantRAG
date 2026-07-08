using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodebaseAssistant.Domain.Enums
{
    public enum RepositoryStatus
    {
        Uploaded = 1,
        Indexing = 2,
        Indexed = 3,
        Failed = 4
    }
}
