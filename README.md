# FoodHub – Food Delivery Management System

A Windows Forms desktop application built with **C# and Microsoft SQL Server** for managing food delivery operations at FoodHub Company. The system handles customer orders, rider assignments, bike fleet management, staff operations, and role-based access control.

---

## Overview

FoodHub is a delivery-only food company. Customers place orders over the phone, staff enter them into the system, and Riders deliver using company motorbikes. This application manages the full delivery workflow from order placement to dispatch.

---

## Features

- Role-based login for Admin, Staff, and Riders
- Customer records management
- Food item catalogue management
- Order placement and dispatch tracking
- Rider profile and workforce management
- Bike fleet records and shift-based bike assignment (with meter readings)
- Staff profile management
- Rider dependent records

---

## Tech Stack

| Layer       | Technology                          |
|-------------|-------------------------------------|
| Language    | C# (.NET Framework)                 |
| UI          | Windows Forms                       |
| Database    | Microsoft SQL Server                |
| Data Access | ADO.NET (DataSets / DataAdapters)   |
| IDE         | Visual Studio                       |

---

## Database Tables

Database name: `FoodHub`

| Table            | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `Admin`          | Admin account credentials                                                   |
| `Staff`          | Staff member records with login credentials                                 |
| `Customer`       | Customer details including address used as delivery address                 |
| `Rider`          | Rider profiles including license number, NIC, DOB, contact, and login       |
| `Dependent`      | Rider dependent records (name, relationship, DOB)                           |
| `FoodItem`       | Food item catalogue with category, price, and ingredients                   |
| `Ingredients`    | Ingredient records linked to food items (minimum 3 per item)                |
| `Order`          | Order records with date, time, status, payment method, amount, dispatch time |
| `Bike`           | Company motorbike fleet records                                              |
| `BikeAssignment` | Shift-based bike assignments to riders with start/end meter readings         |
| `Users`          | Centralised login table with role-based access (Admin / Staff / Rider)      |

---

## Project Structure

```
Food Hub/
├── Images/                           # Application image assets
├── Admin.cs / Admin.Designer.cs      # Admin management form
├── Bike.cs / Bike.Designer.cs        # Bike fleet management form
├── Bike Assign.cs / ...              # Bike assignment form
├── Customer Details.cs / ...         # Customer management form
├── Food item.cs / ...                # Food item management form
├── LogIn.cs / LogIn.Designer.cs      # Login form
├── Order.cs / Order.Designer.cs      # Order management form
├── Order Accept.cs / ...             # Order acceptance form
├── Rider.cs / Rider.Designer.cs      # Rider management form
├── Rider Management.cs / ...         # Rider admin management form
├── Rider Profile.cs / ...            # Rider profile form
├── Staff.cs / Staff.Designer.cs      # Staff form
├── Staff Management.cs / ...         # Staff admin management form
├── Staff Profile.cs / ...            # Staff profile form
├── Food_HubDataSet.xsd               # Typed DataSet schema (x28 datasets for each module)
├── Program.cs                        # Application entry point
├── App.config                        # Application configuration and DB connection string
├── Food Hub.csproj                   # C# project file
├── Food Hub.sln                      # Visual Studio solution file
├── packages.config                   # NuGet packages
├── .gitignore
├── .gitattributes
└── README.md
```

---

## Getting Started

### Prerequisites

- Visual Studio 2019 or later
- Microsoft SQL Server (2019 or later recommended)
- SQL Server Management Studio (SSMS)
- .NET Framework 4.7.2 or later

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/food-hub.git
   ```

2. Set up the database:
   - Open SSMS and connect to your SQL Server instance
   - Create a new database named `FoodHub`
   - Run the SQL script to create all tables and insert sample data

3. Configure the connection string:
   - Open `App.config`
   - Update the connection string to match your SQL Server instance name and credentials:
     ```xml
     <connectionStrings>
       <add name="FoodHubConnectionString"
            connectionString="Data Source=YOUR_SERVER;Initial Catalog=FoodHub;Integrated Security=True"
            providerName="System.Data.SqlClient" />
     </connectionStrings>
     ```

4. Build and run:
   - Open `Food Hub.sln` in Visual Studio
   - Build the solution (`Ctrl + Shift + B`)
   - Run the application (`F5`)
   - Log in using Admin, Staff, or Rider credentials

---

## Author

**K.D. Kaveesha Amiru Nimnaka Fernando** | Student ID: 00272845  
Institution: ESOFT Metro Campus

> This project was developed for academic purposes.
