# CodebaseAssistantRAG — Detailed Progress Report

This document records the current implementation state, the completed ingestion pipeline, and an expanded, actionable plan to implement the Embeddings layer. It includes concrete file names, interfaces, configuration examples, verification steps, and acceptance criteria for each subtask.

---

PROJECT SUMMARY
---------------

Name: CodebaseAssistantRAG
Target: .NET 8 (Clean Architecture)
Purpose: AI-powered RAG Codebase Assistant — user uploads a source repository ZIP, system indexes code, stores chunks and embeddings/vectors, and answers repository questions using retrieval-augmented generation (RAG).

SOLUTION LAYOUT
----------------
- CodebaseAssistant.Api (API endpoints and controllers)
- CodebaseAssistant.Application (use cases, interfaces, DTOs)
- CodebaseAssistant.Domain (entities, value objects)
- CodebaseAssistant.Infrastructure (implementations: Roslyn parser, repository, SQL)
- CodebaseAssistant.Shared (cross-cutting helpers, constants)

PHASE 1 — COMPLETED (Repository Ingestion Pipeline)
---------------------------------------------------
- ZIP upload, validation and extraction
- Repository metadata storage and status tracking
- SHA256 hashing of uploads
- Repository file discovery with ignored folders (bin, obj, .git, node_modules, .vs)
- Roslyn integration: CSharpCodeParser extracts namespace, class, method, method body
- Code chunk generation (Method and File chunks) and ContentHash
- CodeChunk SQL storage and repository indexing pipeline

DATA MODEL SUMMARY
------------------
- Repository: Id, Name, Path, Hash, CreatedAt, UploadedAt, Status, SizeInBytes, Description
- CodeChunk: Id, RepositoryId, FilePath, Namespace, ClassName, MethodName, StartLine, EndLine, Content, ContentHash, ChunkType (Method, File)

GAP ANALYSIS — NEXT PHASE
-------------------------
Missing modules to implement in order:
1. Embeddings generation service (OpenAI client)
2. Options/config binding for OpenAI settings
3. Embedding service implementation and DI registration
4. Integration tests / verification for embeddings
5. Vector store abstraction (IVectorStore) and later Qdrant implementation

IMPLEMENTATION RULES (ENFORCED)
--------------------------------
- Do NOT restart the project.
- Do NOT redesign architecture or recreate existing/complete files.
- Do NOT generate code for completed modules.
- Inspect existing code before making any edits.
- Continue exactly from the next unfinished step and work one step at a time.

DETAILED EMBEDDINGS LAYER PLAN
-------------------------------

STEP 1 — Define IEmbeddingService (Application layer)
- File: CodebaseAssistant.Application/Interfaces/IEmbeddingService.cs
- Purpose: Abstract embedding generation so the application layer is decoupled from OpenAI specifics.
- Suggested interface (contract):
  - Task<float[]> CreateEmbeddingAsync(string text, CancellationToken ct = default);
  - Task<IReadOnlyList<float[]>> CreateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken ct = default);
  - (Optional) Task<string> GetModelNameAsync();
- Acceptance criteria: interface file compiles, no implementation yet.
- Verification: Build solution in Visual Studio (or dotnet build). Build must succeed.

STEP 2 — Add OpenAiOptions and configuration binding
- File: CodebaseAssistant.Application/Options/OpenAiOptions.cs (or Shared)
- Fields:
  - string ApiKey
  - string BaseUrl (optional; default: https://api.openai.com)
  - string Model (e.g., text-embedding-3-small)
  - int TimeoutSeconds
  - bool UseAzure (optional flag) and Azure-specific fields if required later
- Appsettings example (appsettings.Development.json):

  "OpenAI": {
    "ApiKey": "__USE_USER_SECRETS_OR_ENV__",
    "BaseUrl": "https://api.openai.com/v1",
    "Model": "text-embedding-3-small",
    "TimeoutSeconds": 30
  }

- DI registration snippet (Program.cs / Startup.cs in Api project):
  services.Configure<OpenAiOptions>(Configuration.GetSection("OpenAI"));

- Acceptance criteria: OpenAiOptions class exists, DI binding compiles.
- Verification: Build solution.

STEP 3 — Implement OpenAiEmbeddingService
- File: CodebaseAssistant.Infrastructure/Services/OpenAiEmbeddingService.cs
- Responsibilities:
  - Implement IEmbeddingService
  - Use IHttpClientFactory (typed client) or HttpClient injected by DI
  - Build request per OpenAI embeddings API and parse float[] embeddings from response
  - Handle transient errors, timeouts, and invalid responses
  - Log requests at debug level (do not log API key or full text in production)

- Implementation details:
  - Register HttpClient: services.AddHttpClient<OpenAiEmbeddingService>(c => { c.BaseAddress = new Uri(options.BaseUrl); c.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds); });
  - Use IOptions<OpenAiOptions> to get ApiKey and Model
  - Add Authorization header: Bearer {ApiKey}
  - Request body example: { "input": "text here", "model": "text-embedding-3-small" }
  - Parse JSON response to extract embedding array (response.data[0].embedding)
  - Add retry/backoff with Polly (recommended) for transient 5xx responses

- Acceptance criteria: Implementation compiles, DI registration added once.
- Verification: Build solution.

STEP 4 — Test embedding generation (manual/integration)
- Purpose: Verify the service returns a non-empty vector and vector length matches expected model dims.
- How to test:
  1. Set OPENAI__APIKEY (or use dotnet user-secrets) and OpenAI settings in appsettings.Development.json.
  2. Create a temporary Console runner or a unit/integration test that resolves IEmbeddingService from DI and calls CreateEmbeddingAsync("hello world").
  3. Assert: returned vector is not null, Length == expected (for text-embedding-3-small => 1536 dims; text-embedding-3-large => 3072 dims).

- Verification commands:
  - Build: dotnet build
  - Run test/runner: dotnet test OR run Api locally and hit test endpoint

- Acceptance criteria: embedding vector returned, length equals expected model dim, no unhandled exceptions.

STEP 5 — Create IVectorStore (Application layer)
- File: CodebaseAssistant.Application/Interfaces/IVectorStore.cs
- Minimal suggested methods:
  - Task UpsertAsync(string id, float[] vector, IDictionary<string, object> metadata, CancellationToken ct = default);
  - Task<SearchResult[]> SearchAsync(float[] vector, int topK, CancellationToken ct = default);
  - Task DeleteAsync(string id, CancellationToken ct = default);
  - Task<bool> ExistsAsync(string id, CancellationToken ct = default);

- Keep concrete Qdrant implementation for a later step. For now only add the interface so application code can call it once the embedding pipeline is ready.

SECURITY & OPERATIONS NOTES
----------------------------
- Never commit API keys to source. Use dotnet user-secrets, environment variables, or a secrets store.
- Use IHttpClientFactory and configure sensible timeouts and retry/backoff policies (Polly) to avoid cascading failures.
- Log minimal information for debugging: correlation ids, request/response status. Do NOT log API keys or full code contents in production logs.
- Add circuit-breaker protection when calling external APIs repeatedly from bulk indexing jobs.

EXPECTED EMBEDDING MODELS & VECTOR SIZES (reference)
--------------------------------------------------
- text-embedding-3-small: 1536 dimensions
- text-embedding-3-large: 3072 dimensions

NOTE: Confirm model vector size after selecting the exact model; sizes can change between providers and model versions.

DETAILED VERIFICATION CHECKLIST (per step)
------------------------------------------
1. After creating IEmbeddingService: dotnet build  success
2. After adding OpenAiOptions and DI: dotnet build  success; verify options bound in runtime by logging options on startup (without ApiKey)
3. After implementing OpenAiEmbeddingService: dotnet build  success; verify service resolves from DI
4. Run the small integration test/runner, call CreateEmbeddingAsync("test"), assert returned vector length equals expected
5. Create IVectorStore interface: dotnet build  success

WHAT I WILL DO NEXT (if you confirm)
----------------------------------
I will perform STEP 1: add Application/Interfaces/IEmbeddingService.cs to the workspace and build the solution. After that I will report the build output and next actions.

Before I start STEP 1, confirm "Start STEP 1". After each step completes I will tell you exactly what to build, how to verify it, and what should happen before moving to the next step.

---

Acceptance: This enhanced report provides the detailed plan, precise file paths, method contracts, config snippets, verification steps and security notes required to implement the Embeddings layer in small, verifiable increments.
