# Seat Reservation System

Hyelin will hold a fan event on September 14, 2025 at 15:00 KST.

This website allows attendees to select their seat from a map of avialable seats. After selecting their seat, the attendee will be redirected to the payment form.

Payments and notifications will be handled externally. The payment statuses will be relayed back to the website manually via the CMS by staff, and the attendees will be manually sent their tickets by staff.


## Running the website

The database needs to be created first. While creating the database, reference the development connection strings in an 'appsettings.json' file. To populate or update the database, use the scripts in 'database/'.

The two websites of this project are the CMS and Public projects (both in 'src/'). They can be run independently or together.


## Deploying

The web server is already configured. Just publish both website projects and copy/paste.

The database may need updated between releases. Nathan has the production database password if needed.

Verify the website works at <https://hyelinfanmeeting2025.com/>.


## Contributing

Just make a PR with the changes and Nathan will merge them in.
