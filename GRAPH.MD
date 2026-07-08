Codebase Assistant RAG - Full Folder Structure (for deletion)
=============================================================

Below is a complete folder/file tree of the repository as discovered. Use it to locate and delete folders/files directly.

Root (repository root)
-----------------------
- .vs/
- .git/
- bin/
- obj/
- CodebaseAssistantRAG.slnx
- CodebaseAssistant.Api.csproj
- CodebaseAssistant.Infrastructure.csproj
- CodebaseAssistant.Application.csproj
- CodebaseAssistant.Domain.csproj
- CodebaseAssistant.Shared.csproj
- appsettings.json
- CodebaseAssistantRAG.http
- PROJECT_STATUS.md
- GRAPH.MD

CodebaseAssistant.Api/
----------------------
- CodebaseAssistant.Api.csproj
- Program.cs
- LaunchSettings.json (if present)
- Controllers/
  - RepositoryController.cs
  - (other controllers if added)
- Models/
  - Requests/
	- UploadRepositoryRequest.cs
  - Responses/
	- UploadRepositoryResponse.cs
  - (other models)

CodebaseAssistant.Infrastructure/
--------------------------------
- CodebaseAssistant.Infrastructure.csproj
- Services/
  - RepositoryService.cs
  - (indexing, embedding, search services may be added here)
- Persistence/
  - CodebaseAssistantDbContext.cs
  - Configurations/
	- RepositoryConfiguration.cs
  - Migrations/
	- 20260626065205_InitialCreate.cs
	- 20260626065205_InitialCreate.Designer.cs
	- 20260706094219_AddRepositoryUploadFields.cs
	- 20260706094219_AddRepositoryUploadFields.Designer.cs
	- CodebaseAssistantDbContextModelSnapshot.cs
- DependencyInjection/
  - DependencyInjection.cs
- Configuration/
  - UploadSettings.cs

CodebaseAssistant.Application/
------------------------------
- CodebaseAssistant.Application.csproj
- Interfaces/
  - IRepositoryService.cs
  - (other interfaces)
- DTOs/
  - Repository/
	- UploadRepositoryRequest.cs
	- UploadRepositoryResponse.cs

CodebaseAssistant.Domain/
-------------------------
- CodebaseAssistant.Domain.csproj
- Common/
  - BaseEntity.cs
- Entities/
  - Repository.cs
  - (CodeChunk.cs may be added)
- Enums/
  - RepositoryStatus.cs

CodebaseAssistant.Shared/
-------------------------
- CodebaseAssistant.Shared.csproj
- Class1.cs

Other common folders
--------------------
- Uploads/                -> runtime uploaded repository contents (created at runtime)
- scripts/                -> utility scripts if present

Build artifacts you can safely remove (if you want to free space)
----------------------------------------------------------------
- bin/ (project-level bin folders)
- obj/ (project-level obj folders)
- .vs/ (Visual Studio user data)

How to delete a folder quickly (PowerShell)
-------------------------------------------
Example: to remove the Uploads folder from the repo root

Remove-Item -Recurse -Force .\Uploads

To delete a project folder (example: CodebaseAssistant.Infrastructure) from disk

Remove-Item -Recurse -Force .\CodebaseAssistant.Infrastructure

Be careful: these commands permanently delete files from disk. Make a backup or commit changes before deleting.

If you want, tell me which top-level folders you want removed and I can create a PowerShell script or run commands for you to execute locally.

