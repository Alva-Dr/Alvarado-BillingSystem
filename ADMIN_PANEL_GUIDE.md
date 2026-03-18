# Super Admin Management Panel - User Guide

## Overview
The Admin Panel is a Super Admin-only feature that allows managing user accounts and roles in the system.

## Accessing the Admin Panel

### Method 1: Navigation Menu (Recommended)
- When logged in as Super Admin, an **"Admin Panel"** link appears in the top navigation bar (in red)
- Click it to access the Admin Dashboard

### Method 2: Direct URL
- Navigate to: `/Admin/Dashboard`

## Admin Dashboard Features

### Dashboard Home (`/Admin/Dashboard`)
Shows system overview:
- **Total Users**: Count of all registered users
- **Active Users**: Number of active accounts
- **Administrators**: Count of Admin role users
- **Employees**: Count of Employee role users
- **Role Distribution**: Visual progress bars showing role breakdown
- **Account Status**: Active vs Inactive users

### User Management (`/Admin/Users`)

#### View All Users
- Complete list of all users in the system
- Shows: Email, Full Name, Role, Status, Join Date
- Users marked as inactive appear highlighted

#### Edit User
- Click the **edit button** (pencil icon) on any user
- Modify:
  - **Full Name**: Add or update user's display name
  - **Role**: Change user role (Employee → Admin → Super Admin)
  - **Status**: Activate/Deactivate account

#### Activate/Deactivate User
- Click the **lock/unlock button** to toggle account status
- Deactivated users cannot login
- User data is preserved (not deleted)

#### Delete User
- Click the **trash button** to permanently delete user
- Cannot delete if they're the last Super Admin
- This action is permanent and cannot be undone

## User Roles

### Employee
- Basic access to billing features
- Can view their own data
- Limited operational access

### Administrator (Admin)
- Full operational access to billing system
- Can manage invoices, payments, transactions
- Can view reports
- Cannot manage users or system settings

### Super Admin
- **Full system access**
- Can manage all users and assign roles
- Can access Admin Panel
- Full billing system access
- Can deactivate but only ONE must remain active at all times

## Security Features

### Protected Operations
- **Cannot downgrade the last Super Admin** to another role
- **Cannot deactivate the last active Super Admin**
- **Cannot delete the last Super Admin** account
- These safeguards ensure system access is never completely locked

### Role Changes Take Effect
- Role changes apply on user's **next login**
- Current sessions are not affected
- Super Admin role grants admin panel access on login

## Step-by-Step: Assigning Roles

### To Create a New Admin:
1. User must register or be created in system
2. Go to Admin Panel → Manage Users
3. Find the user in the list
4. Click the edit button (pencil icon)
5. Change Role from "Employee" to "Administrator"
6. Click "Save Changes"
7. User will have Admin role on next login

### To Promote to Super Admin:
1. Go to Admin Panel → Manage Users
2. Find the user
3. Click edit button
4. Change Role to "Super Admin"
5. Click "Save Changes"
6. User gains access to Admin Panel on next login

### To Deactivate a User:
1. Go to Admin Panel → Manage Users
2. Click the lock icon next to the user
3. User cannot login anymore
4. Click lock icon again to reactivate

## Technical Details

### Database Table: Users
```sql
CREATE TABLE [Users] (
    [UserId] int PRIMARY KEY IDENTITY,
    [Email] nvarchar(256) UNIQUE NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,  -- 0=Employee, 1=Admin, 2=SuperAdmin
    [CreatedDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [FullName] nvarchar(256)
);
```

### User Roles Enum
```csharp
enum UserRole
{
    Employee = 0,
    Admin = 1,
    SuperAdmin = 2
}
```

### Routes
- `/Admin/Dashboard` - Admin Home
- `/Admin/Users` - User Management List
- `/Admin/EditUser/{id}` - Edit User Details
- `/Admin/DeactivateUser/{id}` - Toggle User Status (AJAX)
- `/Admin/DeleteUser/{id}` - Delete User (AJAX)

## Super Admin Bypass Login

**For initial setup only**, there's a hardcoded Super Admin bypass:
- Email: `543556`
- Use this to login and create additional Super Admin accounts
- Replace this bypass with proper user accounts once system is operational

## Important Notes

⚠️ **WARNING**: 
- The Admin Panel is **ONLY accessible to Super Admin users**
- Attempting to access as any other role will show "Access Denied"
- Users changed to Super Admin gain access on **next login**
- All user management actions are **immediately saved to the database**
- Changes to user roles take effect on **next user login**

## Support

For issues with user management:
1. Verify you're logged in as Super Admin
2. Check the "Admin Panel" link appears in navigation
3. Ensure there's at least one active Super Admin

Questions? Contact system administrator.
