# Test Accounts and Seed Data

## User Accounts

### Super Administrator
- **Email**: superadmin@example.com
- **Password**: SuperAdmin@123
- **Role**: SuperAdmin (Full system access)
- **Note**: This account is excluded from organizational task management

### Administrator
- **Email**: admin@example.com
- **Password**: Admin@123
- **Role**: Administrator
- **Access**: Blog management, general administration

### Blog Author
- **Email**: blog.author@example.com
- **Password**: BlogAuthor@123
- **Role**: Administrator
- **Access**: Content creation and blog management

## Customer Support Team

### Support Administrator
- **Email**: support.admin@example.com
- **Password**: Support@123
- **Role**: CustomerSupportAdmin
- **Name**: Sarah Johnson
- **Access**: Full administrative access to Customer Support area

### Support Manager
- **Email**: support.manager@example.com
- **Password**: Manager@123
- **Role**: CustomerSupportManager
- **Name**: Michael Davis
- **Access**: Can view all tickets and assign work

### Support Agents
1. **Agent 1**
   - **Email**: agent1@example.com
   - **Password**: Agent@123
   - **Name**: Emily Wilson
   - **Role**: CustomerSupportAgent

2. **Agent 2**
   - **Email**: agent2@example.com
   - **Password**: Agent@123
   - **Name**: James Brown
   - **Role**: CustomerSupportAgent

3. **Agent 3**
   - **Email**: agent3@example.com
   - **Password**: Agent@123
   - **Name**: Lisa Martinez
   - **Role**: CustomerSupportAgent

4. **Agent 4**
   - **Email**: agent4@example.com
   - **Password**: Agent@123
   - **Name**: Robert Taylor
   - **Role**: CustomerSupportAgent

## Test Customers
All test customers have the role **CustomerSupportCustomer** and can create/view their own tickets.

1. **Test User 1**
   - **Email**: testuser1@example.com
   - **Password**: TestUser@123
   - **Name**: Test User 1

2. **Test User 2**
   - **Email**: testuser2@example.com
   - **Password**: TestUser@123
   - **Name**: Test User 2

3. **Test User 3**
   - **Email**: testuser3@example.com
   - **Password**: TestUser@123
   - **Name**: Test User 3

4. **Test User 4**
   - **Email**: testuser4@example.com
   - **Password**: TestUser@123
   - **Name**: Test User 4

5. **Test User 5**
   - **Email**: testuser5@example.com
   - **Password**: TestUser@123
   - **Name**: Test User 5

## Seeded Data

### Tickets
The system includes 8 pre-seeded tickets with various statuses:
- **TKT-2024-001**: Cannot login to my account (Open, High Priority)
- **TKT-2024-002**: Billing issue - duplicate charge (In Progress, Critical)
- **TKT-2024-003**: Feature request - Export to PDF (Open, Low Priority, Unassigned)
- **TKT-2024-004**: Application crashes on startup (Resolved, High Priority)
- **TKT-2024-005**: Slow performance issues (In Progress, Medium Priority)
- **TKT-2024-006**: Request for training materials (Closed, Low Priority)
- **TKT-2024-007**: Integration with third-party API failing (Open, Critical, Unassigned)
- **TKT-2024-008**: Update payment method (On Hold, Medium Priority)

### Blog Posts
5 blog posts are seeded:
1. **Getting Started with ASP.NET Core MVC** (Published, Tutorial category)
2. **10 Best Practices for Entity Framework Core** (Published, Technology category)
3. **Building a Customer Support System** (Published, Business category)
4. **Understanding Dependency Injection in .NET** (Published, Tutorial category)
5. **Microservices Architecture Guide** (Draft, Technology category)

### Blog Categories
- Technology
- Business
- Tutorial

### Ticket Comments
Sample comments are added to resolved and in-progress tickets to demonstrate the conversation flow between customers and support agents.

### Ticket History
Complete audit trail for ticket status changes, assignments, and resolutions.

## Testing Scenarios

### Customer Support Testing
1. **As a Customer**: Login with testuser1@example.com to create and view tickets
2. **As an Agent**: Login with agent1@example.com to handle assigned tickets
3. **As a Manager**: Login with support.manager@example.com to oversee all tickets
4. **As Support Admin**: Login with support.admin@example.com for full support administration

### Blog Management Testing
1. **As Blog Author**: Login with blog.author@example.com to create and manage blog posts
2. **As Admin**: Login with admin@example.com to manage all blog content

### User Management Testing
1. **As SuperAdmin**: Login with superadmin@example.com for complete system control
2. Note: SuperAdmin users are automatically filtered out from organizational tasks

## Important Notes
- All passwords follow the pattern: RoleName@123 or TestUser@123
- The SuperAdmin account has IsSuperAdmin flag set to true
- All other accounts have IsSuperAdmin flag set to false
- Customer Support roles are properly mapped in the CustomerServiceRoleMappings table
- All test accounts have EmailConfirmed set to true for immediate access