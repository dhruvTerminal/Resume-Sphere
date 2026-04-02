# Deploy Resume-Sphere on Railway (Full Monorepo)

This repository has **3 deployable services**:
1. **Frontend (Vite + React)**
2. **Backend API (.NET 8)**
3. **AI Service (FastAPI)**

Create one Railway project, then add all 3 services from the same GitHub repo.

## 1) Frontend service
- **Root Directory:** `/` (or `/Resume_Frontend` if you prefer that copy)
- **Build Command:** `npm ci && npm run build`
- **Start Command:** `npm run preview -- --host 0.0.0.0 --port $PORT`

### Frontend env vars
- `VITE_API_BASE_URL=https://<your-backend-service>.up.railway.app`

## 2) Backend API service (.NET)
- **Root Directory:** `/Resume_Backend/ResumeAPI`
- **Build Command:** `dotnet restore && dotnet publish -c Release -o out`
- **Start Command:** `dotnet out/ResumeAPI.dll`

### Backend env vars
- `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
- `ConnectionStrings__DefaultConnection=<railway postgres connection string>`
- `AppSettings__Token=<strong jwt secret>`
- `AiService__BaseUrl=https://<your-ai-service>.up.railway.app`
- `Cors__AllowedOrigins=https://<your-frontend-service>.up.railway.app`
- `EmailConfiguration__SmtpServer=smtp.gmail.com`
- `EmailConfiguration__Port=587`
- `EmailConfiguration__SenderEmail=<your gmail>`
- `EmailConfiguration__SenderPassword=<gmail app password>`
- `EmailConfiguration__SenderName=ResumeSphere OTP`

## 3) AI service (FastAPI)
- **Root Directory:** `/Resume_Backend/ai_service`
- **Build Command:** `pip install -r requirements.txt`
- **Start Command:** `uvicorn main:app --host 0.0.0.0 --port $PORT`

## 4) Networking wiring
1. Deploy AI service first.
2. Copy AI public URL into backend env var `AiService__BaseUrl`.
3. Deploy backend and copy backend public URL into frontend env var `VITE_API_BASE_URL`.
4. Set backend `Cors__AllowedOrigins` to your frontend URL.
5. Redeploy frontend + backend.

## 5) Database
- In Railway project, add **PostgreSQL** plugin.
- Copy the provided connection string into `ConnectionStrings__DefaultConnection` for backend.
- Run EF migrations from a Railway shell or CI step:
  - `dotnet ef database update`

## 6) Sanity checks after deploy
- Frontend loads without CORS errors.
- Backend health responds at `/swagger` (if environment is Development) or any known endpoint.
- AI health responds at `/health`.
- Upload + analysis flow works end-to-end.
