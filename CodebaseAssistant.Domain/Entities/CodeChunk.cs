using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodebaseAssistant.Domain.Common;

namespace CodebaseAssistant.Domain.Entities;

public class CodeChunk : BaseEntity
{
    public Guid RepositoryId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string MethodName { get; set; } = string.Empty;

    public int StartLine { get; set; }

    public int EndLine { get; set; }

    public string Content { get; set; } = string.Empty;

    public string ContentHash { get; set; } = string.Empty;
    public string ChunkType { get; set; } = string.Empty;

    public Repository? Repository { get; set; }
}