# ContactManager (Contact Manager)

[🇭🇺 Hungarian version](README.md)

The **ContactManager** is a WPF desktop application that automatically extracts candidate data from application emails received in a Gmail account and stores them in a local SQLite database. The goal of the app is to make HR / recruitment work easier: all applicants are visible, filterable, and can be managed with statuses in one place.

## Main Features

- **Gmail sign-in (OAuth)**  
  - Sign in with a Google account.
  - The signed-in email address is displayed in the header.

- **Downloading and processing `.eml` attachments**
  - Searches for emails coming from the specified *sender* email address.
  - Processes only emails that have attachments and saves the `.eml` files from them.
  - From the `.eml` files (HTML / text body) it automatically extracts:
    - name
    - phone number
    - email address
    - application date

- **Saving contacts into a SQLite database**
  - EF Core–based `contacts.db` SQLite database.
  - Normalization of phone / email / name (whitespace, lowercase email, `+36` → `06`, etc.).
  - Duplicate handling: the same Name + Phone + Email combination is only stored once.
  - Automatic database migration for the new `IsForeign` column.

- **Managing data in the UI**
  - Displaying contacts in a DataGrid.
  - Multiple selection support (custom `SelectedItemsBehavior`).
  - **Copy email addresses to the clipboard** with a single button click (based on selected rows).
  - **Foreign candidate flag**:
    - The `IsForeign` field can be toggled for a selected record.
    - In the table, the "Külföldi" (Foreign) column shows `"külföldi"` if `true`, and is empty if `false`.
    - Filter: "Csak külföldiek" (Only foreign) checkbox.

- **Status management (HR workflow)**
  - `Megjelent` / `Nem jelent meg` (Published)
  - `Felvett` / `Visszautasítva` (Hired)
  - Buttons adapt to the current selection (they prevent contradictory bulk operations).
  - "Hiányos kontaktok" (Incomplete contacts) filter: shows those contacts where it has not yet been decided whether they appeared / were hired.

- **Filtering and search**
  - Filtering by name
  - Filtering by phone number
  - Filtering by email address
  - Filtering by application date interval (From / To)
  - Can be combined with status and "foreign" filters.

- **Deleting selected records**
  - Before deleting contact(s), a confirmation dialog is shown.

## Technology Stack

- **.NET / C#**
  - .NET 8
  - WPF (Windows Presentation Foundation)

- **Database**
  - SQLite
  - Entity Framework Core

- **Email processing**
  - Gmail API (Google.Apis.Gmail.v1)
  - OAuth 2.0 sign-in
  - Handling attachments and downloading `.eml` files
  - MimeKit for processing `.eml` files

- **UI / MVVM**
  - Architecture based on the MVVM pattern
  - `MainWindowViewModel` for the main view
  - Custom `RelayCommand` implementation for commands
  - `SelectedItemsBehavior` to bind the DataGrid’s multiple selection to the ViewModel
  - Simple, clean, “card-style” filter layout

## Usage

1. **Start the application**
   - Run the `ContactManager.exe` file.

2. **Sign in with a Gmail account**
   - Click the **Bejelentkezés** (Sign in) button.
   - Choose the desired Google account and allow Gmail access.
   - After a successful sign-in, the header shows: `Bejelentkezve: <email address>`.

3. **Specify the sender**
   - In the **Feladó** (Sender) field, enter the email address from which the application emails arrive  
     (e.g. the address used in job ads or on the careers page).
   - The value is stored automatically and will be restored next time the app is opened.

4. **Refresh (read emails)**
   - Click the **Frissítés** (Refresh) button.
   - The application:
     - finds emails from the given sender that have attachments,
     - downloads the `.eml` attachments into a temporary folder,
     - processes them and inserts new contacts into the database.

5. **Browsing and filtering contacts**
   - You can filter the rows displayed in the table using the filter bar (name, phone, email, date interval).
   - The "Hiányos kontaktok" checkbox shows only those who do not yet have a Published/Hired status.
   - The "Csak külföldiek" checkbox shows only those where `IsForeign = true`.

6. **Statuses and flags**
   - **Foreign flag**: select a row and click the *Külföldi jelölés* (Mark as foreign) button.
   - **Status buttons**:
     - *Megjelent / Nem jelent meg* → Published field
     - *Felvett / Visszautasítva* → Hired field
   - Button availability is dynamic: they are only enabled when the current selection state makes sense for the given action.

7. **Copying email addresses**
   - Select the desired rows in the table.
   - Click the **E-mailek másolása vágólapra** (Copy emails to clipboard) button.
   - The email addresses of the selected contacts are copied to the clipboard, one per line.

8. **Deleting selected records**
   - Select the contacts you want to delete.
   - Click the **Kijelölt törlése** (Delete selected) button.
   - Confirm the deletion in the confirmation dialog.

## Known Limitations / Future Ideas

- Currently only Gmail integration is supported.
- HTML parsing is tuned to specific templates – for differently structured application emails, adjustments may be needed.
- There is no advanced permission / role management yet (designed for local desktop usage).
- Possible future improvements:
  - handling multiple senders / campaigns,
  - export (CSV / Excel),
  - statistics (how many applied, how many were hired, etc.),
  - improved logging and error reporting.
