# 🐉 D&D RPG API

A comprehensive **Dungeons & Dragons 5th Edition** character management and combat API built with **ASP.NET Core 8**.

## 🎯 **Project Overview**

This API provides complete CRUD operations for D&D character management, featuring robust validation, proper HTTP status codes, and enterprise-level error handling. Built as a learning project to demonstrate modern .NET development practices.

## ✨ **Features**

### **Character Management**
- ✅ Create, read, update, and delete characters
- ✅ Complete D&D 5e ability scores (Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma)
- ✅ Equipment tracking (weapons, armor, shields)
- ✅ Experience and leveling system
- ✅ Health and combat statistics

### **API Quality**
- ✅ Comprehensive input validation with Data Annotations
- ✅ Custom validation attributes for D&D-specific rules
- ✅ Proper HTTP status codes (200, 201, 400, 404, 409, 500)
- ✅ Structured error responses
- ✅ Entity Framework Core with SQL Server
- ✅ Swagger/OpenAPI documentation
- ✅ Professional logging with Serilog

### **Development Features**
- ✅ Health check endpoints
- ✅ Database connection testing
- ✅ Nullable reference types for better code safety
- ✅ Computed properties for D&D calculations (modifiers, bonuses)

## 🚀 **Getting Started**

### **Prerequisites**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### **Installation**

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/dnd-rpg-api.git
   cd dnd-rpg-api
   ```

2. **Navigate to the API project**
   ```bash
   cd backend/DndRpgApi
   ```

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Update database connection string** (if needed)
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DndRpgDb;Trusted_Connection=true"
     }
   }
   ```

5. **Create and apply database migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Open Swagger UI**
   
   Navigate to `https://localhost:7xxx` (check console output for exact port)

## 📊 **API Endpoints**

### **System**
- `GET /api/health` - Health check
- `GET /api/test-db` - Database connection test

### **Characters**
- `GET /api/character` - Get all characters
- `GET /api/character/{id}` - Get specific character
- `POST /api/character` - Create new character
- `PUT /api/character/{id}` - Update character
- `DELETE /api/character/{id}` - Delete character

## 🧪 **Example Usage**

### **Create a Character**
```bash
POST /api/character
Content-Type: application/json

{
  "name": "Aragorn",
  "level": 5,
  "maxHealth": 45,
  "health": 45,
  "strength": 18,
  "dexterity": 16,
  "constitution": 17,
  "intelligence": 14,
  "wisdom": 15,
  "charisma": 16,
  "weaponName": "Longsword",
  "weaponDamage": "1d8+4",
  "armorName": "Chainmail",
  "armorClass": 16
}
```

### **Response**
```json
{
  "id": 1,
  "name": "Aragorn",
  "level": 5,
  "health": 45,
  "maxHealth": 45,
  "experience": 0,
  "gold": 100,
  "strength": 18,
  "strengthModifier": 4,
  "dexterity": 16,
  "dexterityModifier": 3,
  "healthPercentage": 100.0,
  "createdAt": "2025-01-20T10:30:00Z"
}
```

## 🛡️ **Validation Features**

The API includes comprehensive validation:

- **Required fields**: Name, ability scores
- **Range validation**: Ability scores (1-20), Level (1-20), Health (1-999)
- **String length**: Character names (2-100 characters)
- **Custom validation**: Health cannot exceed max health
- **Dice notation**: Weapon damage must follow D&D format (e.g., "1d8+3")
- **Business rules**: Experience requirements, stat totals

## 🏗️ **Architecture**

### **Technology Stack**
- **Backend**: ASP.NET Core 8 Web API
- **Database**: Entity Framework Core with SQL Server
- **Documentation**: Swagger/OpenAPI
- **Logging**: Built-in .NET logging
- **Validation**: Data Annotations + Custom Attributes

### **Project Structure**
```
backend/DndRpgApi/
├── Controllers/         # API endpoints
├── Data/               # Entity Framework context
├── Models/             # Data models and validation
├── Migrations/         # Database migrations
└── Properties/         # Launch settings
```

## 📈 **Roadmap**

### **Phase 1: Core API** ✅
- [x] Character CRUD operations
- [x] Data validation
- [x] Error handling
- [x] Database setup

### **Phase 2: Enhanced Features** 🚧
- [ ] User authentication (JWT)
- [ ] Combat system
- [ ] Spells and abilities
- [ ] Equipment management

### **Phase 3: Advanced Features** 📋
- [ ] Character parties
- [ ] Campaign management
- [ ] Dice rolling API
- [ ] Integration with D&D 5e API

### **Phase 4: Frontend** 📋
- [ ] React frontend
- [ ] Character sheet interface
- [ ] Combat tracker
- [ ] Campaign dashboard

## 🤝 **Contributing**

This is a learning project, but contributions are welcome!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎓 **Learning Goals**

This project demonstrates:
- RESTful API design principles
- Entity Framework Core usage
- Data validation strategies
- Error handling best practices
- HTTP status code usage
- Swagger API documentation
- Git workflow and version control

## 📚 **Resources**

- [D&D 5e System Reference Document](https://dnd.wizards.com/resources/systems-reference-document)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

**Built with ❤️ and ⚔️ for the D&D community**
