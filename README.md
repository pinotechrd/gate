# Gate
Venue admission system used to learn software architecture step by step.
It starts as a deliberately simple monolith and evolves part by part.- **Guide (Spanish):** [docs/guia-gate.md](docs/guia-gate.md)
- **Backend:** ASP.NET Core 10 Web API with controllers, EF Core 10, PostgreSQL 17 (- **Frontend:** Vue 3 + Vite + TypeScript (`frontend/`)
## Quick start (development)
```bash
docker compose up -d                                   
cd backend && dotnet run --project src/Gate.Api        
cd frontend && npm install && npm run dev              
# PostgreSQL on localhost:5543
# API on http://localhost:5080
# UI on http://localhost:5173
```
## Production-like build
```bash
cd frontend && npm run build                           
# writes the SPA into backend
cd backend && dotnet publish src/Gate.Api -c Release -o ../publish
