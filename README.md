Codebase Assistant using RAG (Retrieval-Augmented Generation)
===========================================================
Overview
--------
This solution implements a .NET 8-based backend for a Codebase Assistant that will index source code, create embeddings, store vectors in a vector DB, and answer developer queries using a LLM (RAG pipeline). The current repository includes repository upload and basic DB tracking. Remaining RAG components are scaffolded in PROJECT_STATUS.md and GRAPH.MD.

Key projects
------------
- CodebaseAssistant.Api      - ASP.NET Core Web API (controllers and request models)
- CodebaseAssistant.Infrastructure - Implementations, EF persistence, upload service
- CodebaseAssistant.Application - Interfaces and DTOs
- CodebaseAssistant.Domain    - Domain entities and enums
- CodebaseAssistant.Shared    - Shared utilities

Getting started (local)
-----------------------
Prerequisites:
- .NET 8 SDK
- SQL Server (localdb is used by default connection string)

Run API:

1. From solution root:
   dotnet build
   dotnet run --project CodebaseAssistant.Api

2. API will run at the configured address (see CodebaseAssistantRAG.http for an example request file).

Upload repository (sample):

- POST /api/repository/upload
- Form-data: file -> repository.zip

Files to review
---------------
- PROJECT_STATUS.md  -> project status and roadmap
- GRAPH.MD           -> project structure and discovered files

Next development tasks
----------------------
Pick a next area to implement (I can scaffold or implement): indexing, embeddings, qdrant, search, chat.

Security & config
-----------------
- Store API keys and secrets securely (user-secrets or Azure Key Vault).
- Add JWT authentication before exposing non-demo endpoints in production.

License
-------
Proprietary to the current workspace owner. Consult repository owner for licensing.
