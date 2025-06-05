# Manual Test: Localization
 
## For the public website

The following tests pertain to the website at <https://hyelinfanmeeting2025.com>. They should be performed for every route individually.

Language tests:

 * When the user's language is English: the website renders in English.
 * When the user's langauge is Korean: the website renders in Korean.

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
