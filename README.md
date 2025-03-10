# PAS Referrals API

## Description
This project is a .NET Razor Pages application that manages a list of Referrals, allowing users to view a list of items and edit individual referrals.

## Prerequisites
Make sure you have the following installed and set up:
- [.NET SDK](https://dotnet.microsoft.com/download) version 8.0
- `az login --tenant <YOUR_TENNANT>`

## Required configuration for local development
To configure the project open user secrets file `secrets.json` (not included in project) and add following values:
```
{
  "Cosmos": {
    "ApimEndpoint": "<APIM_GATEWAY_URL>"
  }
}
```

## Project Structure
The core project structure is organized as follows:
```
WCCG.PAS.Referrals.UI/
│
├── Properties
│   └── launchSettings.json
|
├── Configuration
│   └── Configuration files and their validation
│
├── DbModels
│   └── Database models
|
├── Pages
│   └── Razor pages
|
├── Repositories
│   └── Data repositories
|
├── Services
│   └── Service classes
|
├── Validators
│   └── Validation classes
|
├── appsettings.json
|   └── appsettings.Development.json
|
└── Program.cs

```
## Running the Project
1. Clone the repository.
2. Don't forget `az login --tenant <YOUR_TENNANT>`
3. Setup local configuration according to `Required configuration for local development` section
4. Rebuild and run the project.

## Pages and Description

### Index.cshtml
- **Description:** Displays a list of items.
- **URL:** `/Index`
- **Functionality:** 
  - Lists all referrals.
  - Allows navigation to edit each referral.

### ItemEditor.cshtml
- **Description:** Allows editing of a single referral.
- **URL:** `/ItemEditor/{id}`
- **Functionality:** 
  - Loads referral based on `id`.
  - Saves changes made to the referral or creates a new one if unique Id provided.
