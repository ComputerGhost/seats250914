# Manual Tests: Hosting

DNS tests:

 * The domain name <hyelinfanmeeting2025.com> resolves to <209.59.168.80>.

SSL tests:

 * The URL <https://hyelinfanmeeting2025.com> is delivered over a secure connection.
 * The URL <https://cms.hyelinfanmeeting2025.com> is delivered over a secure connection.
 * The URL <https://www.hyelinfanmeeting2025.com> is delivered over a secure connection.
 * The URL <http://hyelinfanmeeting2025.com> redirects to <https://hyelinfanmeeting2025.com>.
 * The URL <http://cms.hyelinfanmeeting2025.com> redirects to <https://cms.hyelinfanmeeting2025.com>.
 * The URL <http://www.hyelinfanmeeting2025.com> redirects to <https://hyelinfanmeeting2025.com>.
	* The "www" subdomain should be omitted for this result because of the hosting rules.
 * The URL <https://hyelinfanmeeting2025.com> scores an "A" in the Qualys SSL Labs test.
 * The URL <https://cms.hyelinfanmeeting2025.com> scores an "A" in the Qualys SSL Labs test.

Hosting tests:

 * The URL <https://www.hyelinfanmeeting2025> redirects to <https://hyelinfanmeeting2025.com>.
 * The URL <https://hyelinfanmeeting2025.com> renders the public website.
 * The URL <https://cms.hyelinfanmeeting2025.com> renders the CMS website.
 * The URL <http://invalid.hyelinfanmeeting2025.com> does not render content.
 * The URL <https://invalid.hyelinfanmeeting2025.com> does not return content.
 * The website at <https://hyelinfanmeeting2025.com> can be accessed from South Korea.
 * The website at <https://cms.hyelinfanmeeting2025.com> can be accessed from South Korea.

Database tests:

 * The public website is able to connect to the database.
 * The CMS is able to connect to the database.
 * The database can only be accessed from the server--no remote connections.
 * The database can only be accessed by the website.
