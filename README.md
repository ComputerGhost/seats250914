# Seat Reservation System

Hyelin will hold a fan event on September 14, 2025 at 15:00 KST.

This website allows attendees to select their seat from a map of avialable seats. After selecting their seat, the attendee will be redirected to the payment form.

Payments and notifications will be handled externally. The payment statuses will be relayed back to the website manually via the CMS by staff, and the attendees will be manually sent their tickets by staff.


## Running the website

For simply running the websites, follow these steps:

 1. Clone the repository.
 1. Set up a local SQL Express database. If needed, override the connection string.
	* The default connection string is `Server=localhost;Database=Hyelin2025;User Id=sa;Password=Passw0rd;Trust Server Certificate=True`.
	* A custom connection string can be specified with the `InfrastructureOptions__DatabaseConnectionString` environment variable.
 1. Run the "DatabaseMigrator" project in "database/".
 1. Run one or both of the website projects--"CMS" and "Public" in "src/".

Optionally, to also run the email service, follow these steps:

 1. Set up a local SMTP server. If needed, override the default email settings.
	* Papercut SMTP is the easiest to set up and works out of the box.
	* Custom configurations, if needed, can be set via environment variables.
 1. Run the console app `EmailSender`.

Default settings are in the "appsettings.json" files. Custom settings should be set via environment variables. Please do not commit per-user configs or actual secrets into source control.


## Automated tests

The solution contains several types of tests. Not all of them are useful all of the time.

When making a pull request, run the unit tests as a quick check. Running the integration tests is optional for this scenario.

When deploying a website, run all of the automated tests and perform the manual tests. Do this before deployment. After deployment, configure integration tests for production and run them again, because they include some production smoke tests.


## Deploying

Nathan will do these steps. He's the only one with the server password...

 1. Run the automated tests, then do the manual tests locally.
 1. Publish all of the website and tool projects.
 1. Copy/paste files to web server.
 1. Update database by running the migrator on it.
 1. Restart the website and tool services.
 1. Do the manual tests and smoke tests on the live websites.


## Contributing

 1. Make your changes in a new branch.
 2. Run the unit tests. (Ignore the manual tests and integration tests.)
 3. Make a PR, and link the issue.
 4. Nathan will merge it in.
