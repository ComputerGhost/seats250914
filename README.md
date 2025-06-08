# Seat Reservation System

Hyelin will hold a fan event on September 14, 2025 at 15:00 KST.

This website allows attendees to select their seat from a map of avialable seats. After selecting their seat, the attendee will be redirected to the payment form.

Payments and notifications will be handled externally. The payment statuses will be relayed back to the website manually via the CMS by staff, and the attendees will be manually sent their tickets by staff.


## Running the website

 1. Clone the repository.
 1. Set up a local SQL Express database. If needed, override the connection string.
	* The default connection string is `Server=localhost;Database=Hyelin2025;User Id=sa;Password=Passw0rd;Trust Server Certificate=True`.
	* A custom connection string can be specified with the `InfrastructureOptions__DatabaseConnectionString` environment variable.
 1. Run the "DatabaseMigrator" project in "database/".
 1. Run one or both of the website projects--"CMS" and "Public" in "src/".


## Automated tests

The project contains unit tests, integration tests, smoke tests, and manual tests.

The unit tests and integration tests can be run automatically from Visual Studio.

The smoke tests can be run automatically from Visual Studio, but they require the website to be running at the same time. Running the website "without debugging" or configuring the tests to target Production will set up the required test environment. This is complicated, so they are optional for PR's.

The manual tests are only required for Production deployment. They are not a requirement for PR's.


## Deploying

Nathan will do these steps. He's the only one with the server password...

 1. Run the automated tests, then do the manual tests locally.
 1. Publish both website projects.
 1. Copy/paste files to web server.
 1. Update database. (How? Just do it manually for now.)
 1. Restart the website processes.
 1. Do the manual tests on the live websites.


## Contributing

 1. Make your changes in a new branch.
 2. Run the unit tests and integration tests. (Ignore the manual tests and smoke tests.)
 3. Make a PR, and link the issue number.
 4. Nathan will merge it in.
