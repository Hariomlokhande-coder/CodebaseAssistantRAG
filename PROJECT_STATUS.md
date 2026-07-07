Project: Codebase Assistant using RAG (Retrieval-Augmented Generation)

Summary
-------
This document summarizes the current state of the repository, what has already been implemented, and the remaining work required to complete the RAG-based Codebase Assistant MVP.

Implemented (current codebase)
--------------------------------
- Repository upload endpoint and service (accepts .zip, extracts to Uploads/<guid>/Source):
  - Controllers/RepositoryController.cs
  - CodebaseAssistant.Infrastructure/Services/RepositoryService.cs
  - Models/Requests/UploadRepositoryRequest.cs
- Domain entity and EF DbContext to track repositories:
  - CodebaseAssistant.Domain/Entities/Repository.cs
  - CodebaseAssistant.Infrastructure/Persistence/CodebaseAssistantDbContext.cs
- Application interface for repository service:
  - CodebaseAssistant.Application/Interfaces/IRepositoryService.cs
- Program startup and DI registration (DbContext wired):
  - Program.cs

Notes on implemented behavior
- Upload accepts ZIP (max 100 MB), stores repository.zip and extracts into Uploads/{repositoryId}/Source.
- Database model exists for Repository but no indexing records (chunks/embeddings) schema implemented yet.

Remaining work (high level)
---------------------------
The following high-level features required by the project are not yet implemented in the repository:

1) Code Parsing and Chunking (FR-2, FR-3)
   - Implement a parser using Roslyn to extract metadata (file, project, namespace, class, method, line ranges).
   - Implement chunking strategies (file-level, class-level, method-level).
   - Suggested files:
	 - CodebaseAssistant.Infrastructure/Services/IndexingService.cs (new)
	 - CodebaseAssistant.Domain/Entities/CodeChunk.cs (new) and EF configuration

2) Embedding Generation (FR-4)
   - Add an embedding service that calls OpenAI/Azure embeddings (text-embedding-3-small).
   - Suggested files:
	 - CodebaseAssistant.Infrastructure/Services/EmbeddingService.cs (new)
	 - Configuration in appsettings.json for API keys and provider.

3) Vector Storage (FR-5)
   - Integrate Qdrant client to store vectors plus metadata.
   - Suggested files:
	 - CodebaseAssistant.Infrastructure/Services/VectorStore/QdrantClient.cs (new)
	 - Persistence mappings or minimal local index for early dev/testing.

4) Semantic Search and Retrieval Pipeline (FR-6, FR-7)
   - Implement similarity search (Top-K) against Qdrant and pipeline to build prompt context.
   - Suggested files:
	 - CodebaseAssistant.Infrastructure/Services/SearchService.cs (new)
	 - Controllers/SearchController.cs (new)

5) Chat / LLM Response Generation (FR-8)
   - Implement ChatController and ChatService to accept user query, call embedding, retrieve contexts, build prompt and call LLM.
   - Suggested files:
	 - Controllers/ChatController.cs (new)
	 - CodebaseAssistant.Infrastructure/Services/ChatService.cs (new)

6) Source Citation (FR-9)
   - Ensure chunk metadata contains file, class, method, and optional line numbers. Return alongside LLM answers.

7) APIs and Endpoints (FR-10)
   - Implement endpoints for:
	 - POST /api/repository/index -> trigger indexing for an uploaded repository
	 - POST /api/chat/query -> ask question
	 - GET /api/search -> raw chunk search

8) Non-functional & infra
   - JWT authentication for APIs (NFR-3)
   - Secure storage for API keys (user secrets / Key Vault)
   - Serilog logging
   - Docker compose including Qdrant for local dev (optional)

Priority roadmap (MVP)
----------------------
1. Add IndexingService: extract files and generate code chunks with metadata.
2. Add EmbeddingService and a simple in-memory vector store (for local tests).
3. Add SearchService and endpoint to return top-N chunks.
4. Integrate Qdrant (replace in-memory store) and persist embeddings.
5. Add ChatService that composes prompt + sources and calls OpenAI GPT to generate answers.
6. Add /api/repository/index endpoint and background processing (queue/workers) for large repos.
7. Add JWT authentication, Serilog, and secrets management.

Quick dev notes / commands
--------------------------
- Run the API locally: dotnet run (from solution root). Program.cs is configured with DB connection.
- Upload flow: POST /api/repository/upload with form-data (file -> repository.zip)

References (files to review)
---------------------------
- Controllers/RepositoryController.cs
- CodebaseAssistant.Infrastructure/Services/RepositoryService.cs
- CodebaseAssistant.Domain/Entities/Repository.cs
- CodebaseAssistant.Infrastructure/Persistence/CodebaseAssistantDbContext.cs
- Program.cs
- Models/Requests/UploadRepositoryRequest.cs

Next action I can take
----------------------
- Implement the IndexingService and minimal CodeChunk entity and create the POST /api/repository/index endpoint.
- Or, if you prefer, I can scaffold all service interfaces and controllers for the remaining features so you can review before implementation.

If you want me to proceed, tell me which task to start first: "indexing", "embeddings", "qdrant", "search", or "chat".

Project structure
-----------------
See GRAPH.MD for a visual / file-tree summary of the repository and brief descriptions of each project and relevant folders.
