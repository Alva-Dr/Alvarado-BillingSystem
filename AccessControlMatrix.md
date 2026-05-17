# Access Control Matrix

| System Page / Feature | Cashier | Marketing Staff | Manager | Admin | SuperAdmin |
| :--- | :--- | :--- | :--- | :--- | :--- |
| Home (Index, Privacy) | Allowed | Allowed | Allowed | Allowed | Allowed |
| Account (Login, Forgot/Reset Password) | Denied | Denied | Denied | Denied | Denied |
| Account (Profile, Setup MFA, Notifications) | Allowed | Allowed | Allowed | Allowed | Allowed |
| POS Terminal (Index) | Allowed | Denied | Allowed | Allowed | Allowed |
| Cashier (History, Receipt, Shift Management) | Allowed | Denied | Allowed | Allowed | Allowed |
| Marketing Dashboard & Sales Trends | Denied | Allowed | Allowed | Allowed | Allowed |
| Campaigns & Loyalty Overview | Denied | Allowed | Allowed | Allowed | Allowed |
| Promotions (Create, Performance) | Denied | Allowed | Allowed | Allowed | Allowed |
| Manager Dashboard & Finance | Denied | Denied | Allowed | Allowed | Allowed |
| Inventory (Products, Recipes, Archived) | Denied | Denied | Allowed | Allowed | Allowed |
| Reports (Manager views, Transactions) | Denied | Denied | Allowed | Allowed | Allowed |
| Admin Dashboard | Denied | Denied | Denied | Allowed | Allowed |
| Users Management (View/Edit) | Denied | Denied | Denied | Allowed | Allowed |
| Supervising Reports & Inventory Overview | Denied | Denied | Denied | Allowed | Allowed |
| Audit Logs & Security Logs | Denied | Denied | Denied | Allowed | Allowed |
| SuperAdmin Dashboard | Denied | Denied | Denied | Denied | Allowed |
| System Settings & Backups | Denied | Denied | Denied | Denied | Allowed |
