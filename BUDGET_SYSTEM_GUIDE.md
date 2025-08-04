# Budget Management System

## Overview
The Budget Management System provides comprehensive financial tracking for Artists and Venues, with optional associations to Tours and Events. The system includes cascading ownership, flexible permissions, and expense workflow management.

## Entities

### Budget Entity
- **Purpose**: Main budget container with total amounts and tracking
- **Ownership**: Cascade owned by either Artist OR Venue (enforced by database constraint)
- **Optional Associations**: Can be linked to specific Tour or Event
- **Features**: Automatic calculated fields for spent, remaining, pending, approved, and paid amounts

### Expense Entity  
- **Purpose**: Individual line items within a budget
- **Workflow**: Status progression from Pending → Approved/Void → Paid/Void
- **Permissions**: Different access levels for creation vs approval
- **Features**: Attachments, categorization, vendor tracking, approval workflow

## API Endpoints

### Budget Endpoints

#### Get All Budgets
```
GET /api/budgets
GET /api/budgets?page=1&pageSize=10
GET /api/budgets?include=Expenses,Artist,Venue,Tour,Event
```

#### Get Budget by ID
```
GET /api/budgets/{id}
GET /api/budgets/{id}?include=Expenses,Artist,Venue
```

#### Search Budgets
```
GET /api/budgets/search?q=searchterm
GET /api/budgets/search?q=tour name&include=Tour,Artist
```

#### Create Budget
```
POST /api/budgets
Content-Type: application/json

{
  "name": "2024 World Tour Budget",
  "description": "Complete budget for world tour",
  "totalAmount": 50000.00,
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "artistId": 1,        // Either artistId OR venueId required
  "tourId": 5           // Optional association
}
```

#### Update Budget
```
PUT /api/budgets/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Budget Name",
  "totalAmount": 75000.00,
  // ... other fields
}
```

### Expense Endpoints

#### Get All Expenses
```
GET /api/expenses
GET /api/expenses?include=Budget,SubmittedByUser,ApprovedByUser
```

#### Create Expense
```
POST /api/expenses
Content-Type: application/json

{
  "name": "Stage Equipment Rental",
  "description": "Sound system and lighting for show",
  "amount": 2500.00,
  "budgetId": 1,
  "category": "Equipment",
  "vendor": "Stage Pro Audio",
  "expenseDate": "2024-07-15T00:00:00Z",
  "notes": "For main stage setup",
  "attachments": ["https://receipts.example.com/receipt123.pdf"]
}
```

#### Update Expense (Approve/Modify)
```
PUT /api/expenses/{id}
Content-Type: application/json

{
  "id": 1,
  "status": "Approved",
  "approvedByUserId": 2,
  "approvedDate": "2024-07-16T12:00:00Z",
  // ... other fields
}
```

## Permission Model

### Budget Permissions
- **View**: Viewer, Member, Organizer, Manager, Owner, Administrator
- **Create**: Manager, Owner, Administrator only
- **Update**: Manager, Owner, Administrator only  
- **Delete**: Owner, Administrator only

### Expense Permissions
- **View**: Viewer, Member, Organizer, Manager, Owner, Administrator
- **Create**: Member, Organizer, Manager, Owner, Administrator (anyone can add expenses)
- **Update**: Manager, Owner, Administrator only (for approval workflow)
- **Delete**: Owner, Administrator only

## Cascading Ownership

Budgets inherit permissions from their owner entity:

### Artist-Owned Budget
```
Artist (Owner: User A) → Budget → User A has Owner permissions on Budget
```

### Venue-Owned Budget  
```
Venue (Manager: User B) → Budget → User B has Manager permissions on Budget
```

## Expense Status Workflow

```
Pending (default)
    ↓
[Manager/Owner/Admin approval]
    ↓
Approved ←→ Void
    ↓         ↑
[Payment]     [Can void approved expenses]
    ↓         ↑
Paid ←→ Void
```

### Status Descriptions
- **Pending**: Newly submitted, awaiting approval
- **Approved**: Approved for payment by manager/owner/admin
- **Paid**: Payment has been processed
- **Void**: Cancelled/rejected (can happen at any stage)

## Database Schema

### Budgets Table
```sql
CREATE TABLE "Budgets" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000),
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "SpentAmount" DECIMAL(18,2) NOT NULL,
    "StartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "TourId" INTEGER REFERENCES "Tours"("Id") ON DELETE SET NULL,
    "EventId" INTEGER REFERENCES "Events"("Id") ON DELETE SET NULL,
    "ArtistId" INTEGER REFERENCES "Artists"("Id") ON DELETE CASCADE,
    "VenueId" INTEGER REFERENCES "Venues"("Id") ON DELETE CASCADE,
    -- Constraint: Must be owned by either Artist OR Venue, not both
    CONSTRAINT "CK_Budget_Owner" CHECK (
        ("ArtistId" IS NOT NULL AND "VenueId" IS NULL) OR 
        ("ArtistId" IS NULL AND "VenueId" IS NOT NULL)
    )
);
```

### Expenses Table
```sql
CREATE TABLE "Expenses" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "BudgetId" INTEGER NOT NULL REFERENCES "Budgets"("Id") ON DELETE CASCADE,
    "SubmittedByUserId" INTEGER NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "ApprovedByUserId" INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL,
    "Status" INTEGER NOT NULL, -- Enum: 0=Pending, 1=Approved, 2=Void, 3=Paid
    "Category" VARCHAR(100),
    "Vendor" VARCHAR(100),
    "ExpenseDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Attachments" JSONB NOT NULL DEFAULT '[]'
);
```

## Example Usage Scenarios

### 1. Tour Budget Management
```javascript
// Create a tour budget owned by an artist
POST /api/budgets
{
  "name": "Summer Tour 2024",
  "totalAmount": 100000,
  "artistId": 1,
  "tourId": 3,
  "startDate": "2024-06-01",
  "endDate": "2024-08-31"
}

// Add venue rental expense
POST /api/expenses  
{
  "name": "Madison Square Garden Rental",
  "amount": 15000,
  "budgetId": 1,
  "category": "Venue",
  "vendor": "MSG Entertainment"
}
```

### 2. Venue Event Budget
```javascript
// Create event budget owned by venue
POST /api/budgets
{
  "name": "Summer Festival Production",
  "totalAmount": 25000,
  "venueId": 2,
  "eventId": 5,
  "startDate": "2024-07-01", 
  "endDate": "2024-07-05"
}
```

### 3. Expense Approval Workflow
```javascript
// Manager approves expense
PUT /api/expenses/123
{
  "id": 123,
  "status": "Approved",
  "notes": "Approved for immediate payment"
}

// Mark as paid
PUT /api/expenses/123  
{
  "id": 123,
  "status": "Paid",
  "paidDate": "2024-07-20T14:30:00Z"
}
```

## Integration with Other Systems

### Resource Membership
- Budgets and Expenses participate in the resource membership system
- Permissions are inherited from Artist/Venue ownership
- Users gain access based on their role in the owning organization

### Cascading Ownership
- Budgets use `[CascadeOwnershipFrom]` attributes to inherit from Artist/Venue
- Automatic permission propagation ensures proper access control
- No manual membership management required

### Generic Entity System
- Both Budget and Expense use the generic entity endpoints
- Consistent API patterns with pagination, search, includes
- Automatic CRUD operations with proper authorization

## Benefits

✅ **Flexible Ownership**: Artist or Venue ownership with database constraints
✅ **Permission Control**: Granular permissions for different operations  
✅ **Workflow Management**: Clear expense approval process
✅ **Financial Tracking**: Automatic calculations and status tracking
✅ **Integration Ready**: Works with existing Tour/Event/Artist/Venue systems
✅ **Audit Trail**: Full tracking of who submitted, approved, and paid expenses
✅ **Scalable**: Generic entity system handles all CRUD operations
