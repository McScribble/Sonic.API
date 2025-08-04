# User Management System Guide

## Overview
The User Management System provides comprehensive user authentication, authorization, and profile management. It supports JWT-based authentication, Google OAuth integration, role-based permissions, and resource membership management.

## Entity Structure

### User Entity
- **Identity**: Username, email, display name
- **Authentication**: JWT tokens, Google OAuth integration
- **Roles**: System-wide permissions (Administrator, Member, User)
- **Relationships**: Artists, events, resource memberships
- **Profile**: Personal information, preferences, external integrations

## API Endpoints

### Authentication
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET /api/auth/me
```

### Google OAuth
```
GET /api/google-auth
POST /api/google-auth/callback
```

### User Management
```
GET /api/users
GET /api/users/{id}
GET /api/users/search?q=searchterm
PUT /api/users/{id}
DELETE /api/users/{id}
```

### User Registration
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "displayName": "John Doe"
}
```

### User Login
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "displayName": "John Doe",
    "role": "Member"
  }
}
```

### Google OAuth Login
```
GET /api/google-auth?returnUrl=/dashboard

Response: Redirects to Google OAuth consent screen
Callback: POST /api/google-auth/callback with authorization code
```

## Data Models

### User Entity
```csharp
public class User : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public Role Role { get; set; } = Role.User;
    public bool EmailVerified { get; set; } = false;
    public string? GoogleId { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Artist> Artists { get; set; }
    public virtual ICollection<Event> Events { get; set; }
    public virtual ICollection<ResourceMembership> ResourceMemberships { get; set; }
}
```

### UserReadDto (API Response)
```csharp
public class UserReadDto : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public Role Role { get; set; }
    public bool EmailVerified { get; set; }
    public string? GoogleId { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ArtistSummaryDto> Artists { get; set; }
    public List<EventSummaryDto> Events { get; set; }
}
```

### UserSummaryDto (Reference)
```csharp
public class UserSummaryDto : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePicture { get; set; }
    public Role Role { get; set; }
}
```

### Role Enum
```csharp
public enum Role
{
    User = 0,
    Member = 1,
    Administrator = 2
}
```

## Authentication System

### JWT Authentication
- **Access Tokens**: Short-lived (15 minutes) for API access
- **Refresh Tokens**: Long-lived (7 days) for token renewal
- **Token Storage**: Secure HTTP-only cookies recommended
- **Token Validation**: Automatic validation on protected endpoints

### Google OAuth Integration
- **OAuth 2.0**: Standard OAuth 2.0 authorization flow
- **Account Linking**: Link Google accounts to existing users
- **Profile Import**: Import basic profile information from Google
- **Security**: Secure token exchange and validation

### Password Security
- **Hashing**: bcrypt with salt for password hashing
- **Validation**: Strong password requirements
- **Reset**: Secure password reset via email tokens
- **Updates**: Current password verification for changes

## Permission System

### System Roles
- **Administrator**: Full system access, user management
- **Member**: Standard user with full feature access
- **User**: Basic user with limited access

### Resource Permissions
Users can have specific permissions on individual resources (Artists, Events, etc.):
- **Owner**: Full control over the resource
- **Manager**: Management permissions
- **Member**: Standard access permissions
- **Viewer**: Read-only access

### Permission Inheritance
- System roles provide baseline permissions
- Resource memberships grant specific access
- Higher roles include lower role permissions
- Administrators override all resource permissions

## Relationships

### Artists
- One-to-many relationship with Artists
- Users can create and manage multiple artists
- Artist ownership provides full control
- Artist memberships allow collaboration

### Events
- One-to-many relationship with Events
- Users can create and manage events
- Event ownership and membership permissions
- Event attendance tracking

### Resource Memberships
- Many-to-many relationships with all entities
- Granular permission control
- Invitation and approval workflows
- Role-based access within resources

## Features

### Profile Management
- **Personal Information**: Name, bio, location, website
- **Profile Pictures**: Avatar upload and management
- **Social Links**: External social media profiles
- **Privacy Settings**: Control visibility of information

### Account Security
- **Email Verification**: Verify email addresses
- **Two-Factor Authentication**: Optional 2FA setup
- **Login History**: Track account access
- **Session Management**: Active session control

### External Integrations
- **Google**: Account linking and OAuth login
- **Social Media**: LinkedIn, Twitter, Instagram links
- **Music Platforms**: Spotify, Apple Music, SoundCloud
- **Professional**: Band/artist social media

### Notification Preferences
- **Email Notifications**: Event updates, mentions, messages
- **Push Notifications**: Real-time updates
- **Digest Options**: Daily/weekly summary emails
- **Privacy Controls**: Who can contact you

## Example Usage Scenarios

### 1. User Registration
```javascript
POST /api/auth/register
{
  "username": "musiclover123",
  "email": "sarah@example.com",
  "password": "MySecurePassword123!",
  "firstName": "Sarah",
  "lastName": "Johnson",
  "displayName": "Sarah J",
  "bio": "Music enthusiast and event organizer"
}
```

### 2. Google OAuth Login
```javascript
// Redirect to Google OAuth
GET /api/google-auth?returnUrl=/dashboard

// Handle callback
POST /api/google-auth/callback
{
  "code": "google_auth_code",
  "state": "security_state"
}
```

### 3. Update Profile
```javascript
PUT /api/users/1
{
  "id": 1,
  "firstName": "Sarah",
  "lastName": "Johnson",
  "displayName": "Sarah Johnson",
  "bio": "Professional event organizer specializing in indie music",
  "location": "Austin, TX",
  "website": "https://sarahjohnsonevents.com",
  "externalSources": [
    {
      "name": "LinkedIn",
      "url": "https://linkedin.com/in/sarahjohnson",
      "platform": "LinkedIn"
    }
  ]
}
```

### 4. Get User with Relationships
```javascript
GET /api/users/1?include=Artists,Events
```

### 5. Search Users
```javascript
GET /api/users/search?q=Sarah&include=Artists
```

## Database Schema

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FirstName" VARCHAR(100),
    "LastName" VARCHAR(100),
    "DisplayName" VARCHAR(200),
    "Bio" TEXT,
    "ProfilePicture" TEXT,
    "DateOfBirth" DATE,
    "Location" VARCHAR(200),
    "Website" TEXT,
    "Role" INTEGER NOT NULL DEFAULT 0,
    "EmailVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "GoogleId" VARCHAR(100) UNIQUE,
    "ExternalSources" JSONB DEFAULT '[]',
    
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Indexes
```sql
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_GoogleId" ON "Users" ("GoogleId");
CREATE INDEX "IX_Users_Role" ON "Users" ("Role");
CREATE INDEX "IX_Users_DisplayName" ON "Users" ("DisplayName");
```

## Integration Points

### Artist System
Users can create and manage artist profiles, with full ownership and collaboration capabilities.

### Event System
Users organize events, manage attendance, and coordinate with venues and artists.

### Resource Membership
Users can be granted specific permissions on any resource in the system.

### External Services
Integration with Google OAuth, social media platforms, and music streaming services.

## Security Considerations

### Authentication Security
- **Password Hashing**: Use bcrypt with appropriate salt rounds
- **Token Security**: Short-lived access tokens, secure refresh tokens
- **Session Management**: Proper session invalidation and cleanup
- **OAuth Security**: Validate OAuth state parameters and tokens

### Data Protection
- **Personal Information**: Encrypt sensitive personal data
- **Privacy Controls**: User-controlled visibility settings
- **Data Retention**: Comply with data protection regulations
- **Audit Logging**: Track access to sensitive user data

### API Security
- **Rate Limiting**: Prevent brute force attacks
- **Input Validation**: Sanitize and validate all user inputs
- **HTTPS Only**: Enforce secure connections
- **CORS Policy**: Configure appropriate CORS settings

## User Lifecycle

### Registration
1. User provides registration information
2. System validates email uniqueness
3. Password is hashed and stored
4. Email verification sent (optional)
5. User account created with default role

### Authentication
1. User provides credentials (email/password or OAuth)
2. System validates credentials
3. JWT tokens generated and returned
4. User session established
5. Access granted to protected resources

### Profile Management
1. User updates profile information
2. System validates changes
3. Profile updated in database
4. Related entities updated if necessary
5. Notifications sent if configured

### Deactivation
1. User requests account deletion
2. System validates request
3. Related data handled per retention policy
4. Account marked as deleted or removed
5. Sessions invalidated

## Best Practices

### 1. Security
- Implement strong password policies
- Use secure token storage
- Validate all user inputs
- Monitor for suspicious activities
- Keep security dependencies updated

### 2. User Experience
- Provide clear error messages
- Implement progressive registration
- Offer multiple authentication options
- Maintain responsive design
- Optimize profile management flows

### 3. Privacy
- Respect user privacy preferences
- Provide clear privacy policies
- Allow data export and deletion
- Minimize data collection
- Secure personal information

### 4. Performance
- Optimize user queries
- Cache frequently accessed data
- Implement pagination for user lists
- Use efficient authentication checks
- Monitor system performance

### 5. Maintenance
- Regular security audits
- Monitor authentication logs
- Update user communication preferences
- Maintain external integrations
- Clean up inactive accounts

The User Management System provides a comprehensive foundation for user authentication, authorization, and profile management, with robust security features and flexible permission systems that integrate seamlessly with all other system components.
