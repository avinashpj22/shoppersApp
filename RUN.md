# 🚀 Running the E-Commerce App

**All configuration files generated!** The app is ready to run.

## ✅ System Check
- ✅ .NET 9.0.304 (need 8+)
- ✅ Node.js v22.16.0
- ✅ npm 11.6.0

---

## ⚡ Quick Start (2 minutes - Frontend Only)

### Terminal 1 - Start Frontend:
```powershell
cd frontend
npm install
npm start
```

**Expected:** Opens http://localhost:4200 with product catalog

---

## 🎯 Full Setup (5 minutes - Backend + Frontend)

### Terminal 1 - Backend API:
```powershell
cd backend/Gateway/ApiGateway
dotnet restore
dotnet run
```
**Expected:** `Now listening on: http://localhost:8000`

### Terminal 2 - Frontend:
```powershell
cd frontend
npm install
npm start
```
**Expected:** `Build complete` + opens http://localhost:4200

### Terminal 3 - Product Service (runs auto via gateway):
```powershell
cd backend/Services/ProductService/ProductService.API
dotnet restore
dotnet run
```

### Terminal 4 - Order Service (runs auto via gateway):
```powershell
cd backend/Services/OrderService/OrderService.API
dotnet restore
dotnet run
```

---

## 🌐 Access Points

| What | URL |
|------|-----|
| App | http://localhost:4200 |
| API Gateway | http://localhost:8000 |
| Product Service | http://localhost:5000 |
| Order Service | http://localhost:5002 |

---

## � Features to Try

1. **Browse** → Scroll through products
2. **Details** → Click a product
3. **Cart** → Add items to cart
4. **Checkout** → Fill form & place order
5. **Orders** → View order history
6. **Filter** → Filter orders by status

---

## 🛑 Stop All

Press `Ctrl+C` in each terminal

---

## ⚠️ Troubleshooting

**"Port 8000 in use"**
```powershell
netstat -ano | findstr :8000
taskkill /PID <PID> /F
```

**"npm install fails"**
```powershell
npm cache clean --force
npm install
```

**"No products showing"**
- Refresh browser (F5)
- Check backend is running
- Wait 5-10 seconds

**"Module not found" errors**
- Normal during startup
- Wait for "Build complete" message

---

## 📚 Learn More

- [README.md](README.md) - Project overview
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) - System design
- [docs/API_SPECIFICATION.md](docs/API_SPECIFICATION.md) - All endpoints
- [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) - Dev guide

---

**Ready? Run:** `cd frontend && npm install && npm start` → Open http://localhost:4200 🚀
