# Seat Reservation System

Hyelin will hold a fan event on September 14, 2025 at 15:00 KST.

This website allows attendees to select their seat from a map of avialable seats. After selecting their seat, the attendee will be redirected to the payment form.

Payments will be handled externally. The payment statuses will be relayed back to the website manually via the CMS by staff, and the attendees will pick up their tickets at the event.


## Running the website locally

For simply running the websites, follow these steps:

 1. Clone the repository.
 1. Set up a local SQL Express database.
	* The default connection string is `Server=localhost;Database=Hyelin2025;User Id=sa;Password=Passw0rd;Trust Server Certificate=True`.
	* The default can be overridden by an environment variable if needed.
 1. Run the "DatabaseMigrator" project in "database/".
 1. Run one or both of the website projects--"CMS" and "Public" in "src/".

To also run the email service, follow these steps:

 1. Set up a local SMTP server. If needed, override the default email settings.
	* Papercut SMTP is the easiest to set up and works out of the box.
	* Custom configurations, if needed, can be set by environment variables.
 1. Run the console app `EmailSender`.

Default settings are in the "appsettings.json" files. Custom settings should be set by environment variables. Please do not commit per-user configs or actual secrets into source control.


## Automated tests

The solution contains several types of tests. Not all of them are useful all of the time.

When making a pull request, run the unit tests as a quick check. Running the integration tests is optional for this scenario.

When deploying a website, run all of the automated tests and perform the manual tests. Do this before deployment. After deployment, configure integration tests for production and run them again, because they include some production smoke tests.


## Deploying

Nathan will do these steps. He's the only one with the server password.

If multiple projects will be deployed, it is advised to deploy in this order:
 1. Staging database
 1. Staging Public website
 1. Staging CMS website
 1. Staging EmailSender service
 1. Production database
 1. Production Public website (green or blue)
 1. Production CMS website
 1. Production EmailSender service

### Publishing the database

First, publish the migrator to the server.

 1. Run all of the automated tests, then do the manual tests locally.
 1. Publish the migrator to a local folder.
 1. Copy the files to the staging location.
	* Server path: ~/tools/migrate/

Next, migrate the staging database.

 1. Run the migrator for the staging database.
	* Command: `~/tools/staging-migrate.sh`
 1. Run the smoke tests while targetting the staging websites.
	* URL: https://staging.hyelinfanmeeting2025.com
	* URL: https://staging-cms.hyelinfanmeeting2025.com

After receiving a green light to finish publishing, continue to migrate the production database.

 1. Run the migrator for the production database.
	* Command: `~/tools/migrate.sh`
 1. Run the smoke tests while targetting the production websites.
	* URL: https://hyelinfanmeeting2025.com
	* URL: https://cms.hyelinfanmeeting2025.com

### Publishing the public website

First, publish to the staging website for the changes to be previewed.

 1. Run all of the automated tests, then do the manual tests locally.
 1. Publish the website to a local folder.
 1. Copy the files to the staging wbsite.
	* Server path: ~/www/staging.hyelinfanmeeting2025.com/
 1. Restart the website service.
	* Command: `systemctl restart staging.hyelinfanmeeting2025.com`
 1. Do the smoke tests while targetting the staging website.
	* URL: https://staging.hyelinfanmeeting2025.com

After receiving a green light to finish publishing, continue to publish to the production website.

 1. Run the deployment tool to discover whether to deploy to green or to blue.
	* Command: `~/tools/deploy-website.sh`
	* The "Next target" line will provide the next target (green or blue).
 1. Copy the files from the staging website to the next target.
	* Source server path: ~/www/staging.hyelinfanmeeting2025.com/
	* Destination server path: ~/www/_[next target]_.hyelinfanmeeting2025.com/
 1. Do the smoke tests while targetting the new deployment.
	* URL: https://[next_target].hyelinfanmeeting2025.com
 1. Run the deployment tool again to switch to the new deployment.
	* Command: `~/tools/deploy-website --deploy`
 1. Run the smoke tests while targetting the live website.
	* URL: https://hyelinfanmeeting2025.com

### Publishing the CMS

First, publish to the staging website for the changes to be previewed.

 1. Run all of the automated tests, then do the manual tests locally.
 1. Publish the website to a local folder.
 1. Copy the files to the staging website.
	* Server path: ~/www/staging-cms.hyelinfanmeeting2025.com/
 1. Restart the website service.
	* Command: `systemctl restart staging-cms.hyelinfanmeeting2025.com`
 1. Do the smoke tests while targetting the staging website.
	* URL: https://staging-cms.hyelinfanmeeting2025.com

After receiving a green light to finish publishing, continue to publish to the production website.

 1. Copy the staging files to the production website.
	* Source server path: ~/www/staging-cms.hyelinfanmeeting2025.com/
	* Destination server path: ~/www/cms.hyelinfanmeeting2025.com/
 1. Restart the website service.
	* Command: `systemctl restart cms.hyelinfanmeeting2025.com`
 1. Do the smoke smoke tests while targetting the production website.
	* URL: https://cms.hyelinfanmeeting2025.com

### Publishing the email sender

First, publish to the staging location for the changes to be previewed.

 1. Run all of the automated tests, then do the manual tests locally.
 1. Publish the service to a local folder.
 1. Copy the files to the staging location.
	* Server path: ~/services/staging-email-sender
 1. Restart the service.
	* Command: `systemctl restart staging.hyelinfanmeeting2025.com-emailsender`
 1. Test the service by doing something that generates an email.

After receiving a green light to finish publishing, continue to publish to the production location.

 1. Copy the staging files to the production location.
	* Source server path: ~/services/staging-email-sender
	* Destination server path: ~/services/email-sender
 1. Restart the service.
	* Command: `systemctl restart hyelinfanmeeting2025.com-emailsender`
 1. Cross fingers and hope it continues to work... there is no smoke test for this one.

## Contributing

 1. Make your changes in a new branch.
 2. Run the unit tests. (Ignore the manual tests and integration tests.)
 3. Make a PR, and link the issue.
 4. Nathan will merge it in.
