using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodebaseAssistant.Infrastructure.Roslyn;

public class ParsedCodeItem
{
    public string FilePath { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string MethodName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}