using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodebaseAssistant.Infrastructure.Configuration
{
    public class UploadSettings
    {
        public string? RepositoryPath { get; set; }
        public int MaxFileSizeMb { get; set; }
    }
}
