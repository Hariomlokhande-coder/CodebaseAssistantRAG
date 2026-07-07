Codebase Assistant RAG - Detailed Project Structure
=================================================

Below is a deeper scan of the repository showing projects, folders and key files discovered in the workspace. Use this as the working map when adding new features.

Root
----
- CodebaseAssistantRAG.slnx
- CodebaseAssistant.Api.csproj
- appsettings.json
- CodebaseAssistantRAG.http
- PROJECT_STATUS.md
- GRAPH.MD

CodebaseAssistant.Api/ (API project)
-----------------------------------
- CodebaseAssistant.Api.csproj
- Program.cs
- Controllers/
  - RepositoryController.cs  -> POST /api/repository/upload uses IRepositoryService
- Models/
  - Requests/
	- UploadRepositoryRequest.cs  -> IFormFile model used by the controller

Description: API layer that exposes endpoints. Currently contains the repository upload endpoint. Add ChatController, SearchController here later.

CodebaseAssistant.Infrastructure/ (Infrastructure implementations)
----------------------------------------------------------------
- CodebaseAssistant.Infrastructure.csproj
- Services/
  - RepositoryService.cs            -> Implements IRepositoryService: accepts ZIP, validates size/extension, extracts into Uploads/{id}/Source
  - (Place to add) IndexingService.cs -> extract files, parse with Roslyn, create chunks
  - (Place to add) EmbeddingService.cs -> call OpenAI/Azure embeddings
  - (Place to add) VectorStore/ Qdrant client -> persist vectors and metadata
  - (Place to add) SearchService.cs -> handle similarity search & retrieval
- Persistence/
  - CodebaseAssistantDbContext.cs   -> EF DbContext with DbSet<Repository>
  - Configurations/
	- RepositoryConfiguration.cs    -> Entity configuration for Repository
  - Migrations/
	- 20260626065205_InitialCreate.cs
	- 20260626065205_InitialCreate.Designer.cs
	- 20260706094219_AddRepositoryUploadFields.cs
	- 20260706094219_AddRepositoryUploadFields.Designer.cs
	- CodebaseAssistantDbContextModelSnapshot.cs
- DependencyInjection/
  - DependencyInjection.cs          -> service registrations (AddInfrastructure)
- Configuration/
  - UploadSettings.cs               -> settings related to uploading (max size, allowed extensions)

Description: Infrastructure houses implementations, EF persistence and configuration. Recommended place to add Qdrant client and embedding service.

CodebaseAssistant.Application/ (Application layer)
-----------------------------------------------
- CodebaseAssistant.Application.csproj
- Interfaces/
  - IRepositoryService.cs           -> interface for upload service
- DTOs/
  - Repository/
	- UploadRepositoryRequest.cs    -> placeholder (internal) in Application layer (API uses the Api project model)
	- UploadRepositoryResponse.cs   -> used for API responses (exists)

Description: Define application-level interfaces, DTOs, use-cases. Add interfaces for IIndexingService, IEmbeddingService, ISearchService, IChatService here.

CodebaseAssistant.Domain/ (Domain models)
-----------------------------------------
- CodebaseAssistant.Domain.csproj
- Common/
  - BaseEntity.cs
- Entities/
  - Repository.cs                    -> domain model tracked in DB
  - (Place to add) CodeChunk.cs      -> entity to persist chunk metadata (id, file, class, method, startLine, endLine, contentHash, repositoryId)
- Enums/
  - RepositoryStatus.cs              -> enum for repository status

Description: Domain models and enums. Add CodeChunk and Embedding entity here for persistence.

CodebaseAssistant.Shared/
-------------------------
- CodebaseAssistant.Shared.csproj
- Class1.cs

Description: Shared utilities, helpers, common constants.

Build artifacts / IDE files
--------------------------
- bin/ obj/                         -> build outputs
- .vs/                              -> Visual Studio workspace data

Missing / Next recommended additions (where to add them)
-------------------------------------------------------
- IndexingService (Infrastructure/Services): use Roslyn to parse C# files and extract chunks with metadata (file, namespace, class, method, line ranges).
- CodeChunk entity (Domain/Entities) + EF configuration (Infrastructure/Persistence/Configurations): store chunk metadata and optionally vector id.
- EmbeddingService (Infrastructure/Services): call OpenAI/Azure embeddings for each chunk.
- VectorStore (Infrastructure/Services/VectorStore): Qdrant client integration to persist vectors and metadata.
- SearchService and ChatService (Infrastructure/Services) and corresponding controllers (Api/Controllers): implement semantic search and chat endpoints.
- Add configuration entries in appsettings.json for OpenAI/Azure keys, Qdrant connection, indexing options.
- Add JWT authentication and Serilog configuration in Program.cs and Infrastructure/DependencyInjection.

Quick mapping of FRs -> suggested files to create
------------------------------------------------
- FR-2 / FR-3 (Parsing & Chunking): Infrastructure/Services/IndexingService.cs, Domain/Entities/CodeChunk.cs
- FR-4 (Embeddings): Infrastructure/Services/EmbeddingService.cs
- FR-5 (Vector DB): Infrastructure/Services/VectorStore/QdrantClient.cs
- FR-6 / FR-7 (Search / Retrieval): Infrastructure/Services/SearchService.cs, Api/Controllers/SearchController.cs
- FR-8 (Chat): Infrastructure/Services/ChatService.cs, Api/Controllers/ChatController.cs

How I can proceed now
---------------------
Choose one of the following and I will implement a scaffold or initial implementation:
- "indexing"  -> add CodeChunk entity, IndexingService scaffold, and POST /api/repository/index endpoint
- "embeddings" -> scaffold EmbeddingService and settings
- "qdrant" -> scaffold Qdrant client with DTOs
- "search" -> scaffold SearchService and SearchController
- "chat" -> scaffold ChatService and ChatController

