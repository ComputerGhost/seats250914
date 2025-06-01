# Manual Test: Localization
 
## For the public website

The following tests pertain to the website at <https://hyelinfanmeeting2025.com>. They should be performed for every route individually.

Language tests:

 * When the query string contains 'culture=en': the website renders in English.
 * When the query string contains 'culture=ko': the website renders in Korean.
 * When the user's primary language is English, and they are visiting the website for the first time: the website renders in English.
 * When the user's primary language is Korean, and they are visiting the website for the first time: the website renders in Korean.
 * When the user's primary language is English, but they have selected to use Korean: the website renders in Korean.
 * When the user's primary language is Korean, but they have selected to use English: the website renders in English.
 * When the user has selected to use a language, and the user navigates to another webpage: both the starting and ending webpage render in the selected language.

DateTime tests:

 * All dates and times are displayed in KST.
 * All date and time displays specify the timezone.

## For the CMS website

The following tests pertain to the website at <https://cms.hyelinfanmeeting2025.com>. They should be performed for every route individually.

Language tests:

 * All webpages render in Korean.

DateTime tests:

 * All timestamp displays are rendered in ISO 8601 format.
 * All timestamp displays are paired with a timezone converter widget.
 * All date and time form fields include a time zone component.
