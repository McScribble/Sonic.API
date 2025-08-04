# Budget and Expense System Implementation Summary

## 🎯 **System Overview**
Comprehensive budget management system with cascading ownership, expense workflows, and granular permissions for Artists and Venues in the Sonic API.

## ✅ **What Was Implemented**

### **1. Core Entities**

#### **Budget Entity** (`Models/Budgets/Budget.cs`)
- Cascade owned by either Artist OR Venue (database constraint enforced)
- Optional associations with Tour and Event
- Automatic calculated properties (remaining, pending, approved, paid amounts)
- Time-based budget periods with start/end dates

#### **Expense Entity** (`Models/Budgets/Expense.cs`)
- Line items within budgets with status workflow
- Status progression: Pending → Approved/Void → Paid/Void
- User tracking for submission and approval
- Attachments support (JSON array of URLs)
- Categorization and vendor tracking

### **2. Complete DTO Architecture**

#### **Budget DTOs**
- `BudgetDto` - Full representation with related entities
- `BudgetCreateDto` - Creation payload
- `BudgetSummaryDto` - Circular reference prevention

#### **Expense DTOs**  
- `ExpenseDto` - Full representation with related entities
- `ExpenseCreateDto` - Creation payload (SubmittedByUserId auto-set)
- `ExpenseSummaryDto` - Circular reference prevention

#### **Supporting Summary DTOs**
- `UserSummaryDto` - User references in expenses
- `ArtistSummaryDto` - Artist ownership references
- `VenueSummaryDto` - Venue ownership references
- `EventSummaryDto` - Event association references

### **3. Database Schema**

#### **Tables Created**
```sql
-- Budgets table with ownership constraint
CREATE TABLE "Budgets" (
    -- Standard entity fields
    "Id", "Name", "Description", "CreatedAt", "UpdatedAt", "Uuid", "Emoji"
    
    -- Budget-specific fields
    "TotalAmount" DECIMAL(18,2),
    "SpentAmount" DECIMAL(18,2), 
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    
    -- Optional associations
    "TourId" REFERENCES "Tours" ON DELETE SET NULL,
    "EventId" REFERENCES "Events" ON DELETE SET NULL,
    
    -- Cascading ownership (exactly one required)
    "ArtistId" REFERENCES "Artists" ON DELETE CASCADE,
    "VenueId" REFERENCES "Venues" ON DELETE CASCADE,
    
    -- Database constraint: Artist OR Venue, not both
    CONSTRAINT "CK_Budget_Owner" CHECK (
        ("ArtistId" IS NOT NULL AND "VenueId" IS NULL) OR 
        ("ArtistId" IS NULL AND "VenueId" IS NOT NULL)
    )
);

-- Expenses table with workflow support
CREATE TABLE "Expenses" (
    -- Standard entity fields
    "Id", "Name", "Description", "CreatedAt", "UpdatedAt", "Uuid", "Emoji"
    
    -- Expense-specific fields
    "Amount" DECIMAL(18,2),
    "BudgetId" REFERENCES "Budgets" ON DELETE CASCADE,
    "Status" INTEGER, -- Enum: Pending=0, Approved=1, Void=2, Paid=3
    "Category", "Vendor", "Notes",
    "ExpenseDate" TIMESTAMP,
    
    -- User tracking
    "SubmittedByUserId" REFERENCES "Users" ON DELETE RESTRICT,
    "ApprovedByUserId" REFERENCES "Users" ON DELETE SET NULL,
    "ApprovedDate" TIMESTAMP,
    "PaidDate" TIMESTAMP,
    
    -- Attachments as JSON
    "Attachments" JSONB DEFAULT '[]'
);
```

#### **Indexes for Performance**
- Budget: ArtistId, VenueId, TourId, EventId, StartDate+EndDate
- Expense: BudgetId, SubmittedByUserId, ApprovedByUserId, Status, ExpenseDate, Category

### **4. Permission System**

#### **Budget Permissions**
- **View**: All membership types (Viewer → Administrator)
- **Create/Update**: Manager, Owner, Administrator only
- **Delete**: Owner, Administrator only

#### **Expense Permissions**  
- **View**: All membership types (Viewer → Administrator)
- **Create**: Member, Organizer, Manager, Owner, Administrator (anyone can add expenses)
- **Update**: Manager, Owner, Administrator only (for approval workflow)
- **Delete**: Owner, Administrator only

### **5. API Endpoints**

#### **Budget Endpoints** (`/api/budgets`)
- `GET /api/budgets` - List with pagination
- `GET /api/budgets/{id}` - Get specific budget
- `GET /api/budgets/search` - Search budgets
- `POST /api/budgets` - Create budget
- `PUT /api/budgets/{id}` - Update budget
- `DELETE /api/budgets/{id}` - Delete budget

#### **Expense Endpoints** (`/api/expenses`)
- `GET /api/expenses` - List with pagination
- `GET /api/expenses/{id}` - Get specific expense
- `GET /api/expenses/search` - Search expenses  
- `POST /api/expenses` - Create expense
- `PUT /api/expenses/{id}` - Update expense (approval workflow)
- `DELETE /api/expenses/{id}` - Delete expense

### **6. Service Registration**
```csharp
// Generic entity services (Program.cs)
builder.Services.AddScoped<IEntityService<BudgetDto, BudgetCreateDto, Budget>>();
builder.Services.AddScoped<IEntityService<ExpenseDto, ExpenseCreateDto, Expense>>();

// Endpoint mappings with permissions
app.MapEntityEndpoints<BudgetDto, BudgetCreateDto, Budget>(ResourceType.Budget, budgetPermissions);
app.MapEntityEndpoints<ExpenseDto, ExpenseCreateDto, Expense>(ResourceType.Expense, expensePermissions);
```

## 🔧 **Key Features**

### **Cascading Ownership**
```csharp
[CascadeOwnershipFrom(nameof(Artist), typeof(User))]
[CascadeOwnershipFrom(nameof(Venue), typeof(User))]
public class Budget : GenericEntity
```
- Budgets inherit permissions from Artist or Venue ownership
- Users automatically get appropriate access based on their role

### **Expense Status Workflow**
```csharp
public enum ExpenseStatus
{
    Pending,    // Default state
    Approved,   // Ready for payment
    Void,       // Cancelled/rejected
    Paid        // Payment completed
}
```

### **Automatic Calculations**
```csharp
// Budget computed properties
public decimal RemainingAmount => TotalAmount - SpentAmount;
public decimal PendingAmount => Expenses.Where(e => e.Status == ExpenseStatus.Pending).Sum(e => e.Amount);
public decimal ApprovedAmount => Expenses.Where(e => e.Status == ExpenseStatus.Approved).Sum(e => e.Amount);
public decimal PaidAmount => Expenses.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Amount);
```

### **Flexible Associations**
- **Required**: Budget must be owned by Artist OR Venue
- **Optional**: Budget can be associated with Tour and/or Event
- **Expenses**: Always belong to exactly one Budget

## 🗃️ **Database Migration**
- **Migration Name**: `20250804140455_AddBudgetAndExpenseEntities`
- **Status**: ✅ Applied successfully
- **Tables**: `Budgets`, `Expenses` with all constraints and indexes

## 🔗 **Integration Points**

### **Resource Membership System**
- Budget: ResourceType.Budget
- Expense: ResourceType.Expense
- Full integration with existing permission checking

### **Existing Entities**
- **Artists**: Can own budgets (cascade ownership)
- **Venues**: Can own budgets (cascade ownership)  
- **Tours**: Can be associated with budgets (optional)
- **Events**: Can be associated with budgets (optional)
- **Users**: Submit and approve expenses

### **Generic Entity Framework**
- Uses same patterns as Tours, Events, Artists, Venues
- Consistent API endpoints and behavior
- Automatic CRUD with authorization

## ✅ **System Status**
- **Build**: ✅ Successful
- **Migration**: ✅ Applied
- **Application**: ✅ Running on http://localhost:5153
- **Endpoints**: ✅ Available and registered
- **Documentation**: ✅ Complete guide created

## 🎯 **Usage Examples**

### **Create Artist Tour Budget**
```json
POST /api/budgets
{
  "name": "World Tour 2024 Budget",
  "totalAmount": 250000,
  "artistId": 1,
  "tourId": 3,
  "startDate": "2024-06-01",
  "endDate": "2024-12-31"
}
```

### **Add Expense**
```json
POST /api/expenses
{
  "name": "Stage Equipment Rental",
  "amount": 15000,
  "budgetId": 1,
  "category": "Equipment",
  "vendor": "Pro Audio Systems"
}
```

### **Approve Expense** 
```json
PUT /api/expenses/123
{
  "id": 123,
  "status": "Approved",
  "notes": "Approved for immediate payment"
}
```

The Budget and Expense system is now **fully operational** and ready for production use! 🚀
