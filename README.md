# A quick subtitle file translator
A simple .NET Core 3.1 console app that translate subtitles files (.srt, .sub, etc.) using external translation services.

# Screenshot (Translating using Microsoft API)
![alt text](https://raw.githubusercontent.com/jonwolfdev/QuickSubtitleTranslator/master/microsoft_screenshot.jpg)

## Version 2 Update
- Improved failure handling when calling external services
- Cleaned up code
- Better SRT validation for input and output
- Line breaks are respected
- Better character count
- Character limit option
- Slightly better translation (because texts are not splitted up, except for IBM)
- More integration tests (mocking external services)
- Peek option

## Ranking (personal choice)
- Amazon
- Google / Microsoft
- IBM

## Improvements to be made:
- Clode clean up
- Graceful failures (internet disconnected, rate exceeded, etc.)
- Better limit control (quotas)
- Improve Amazon API implementation (it's slow)
- Remove hardcoded delays
- Ability to pause/stop processing
- Validation after each translated SRT file (make sure times are exact, etc.)
- Logs
- Configurable options (delays, limit control, etc.)

## Setup language translator services
Supported APIs: Google, Microsoft, Amazon, IBM

### Google API
- Create a Google cloud account
- Create a project
- Enable Cloud Translation API
- Create API key
- (For testing only) Create system environment variable:
    - name: qsubtranslator_google_key
    - value: value#api_key_value_goes_here

**Note**: Free 500k characters per month

### Microsoft API
- Create azure account
- Create cognitive translator text resource
- Copy Api Key
- (For testing only) Create system environment variable:
    - name: qsubtranslator_microsoft_key
    - value: value#api_key_value_goes_here

**Note**: Free 2 million characters per month

### Amazon API
- Create IAM user with read only permission to translate text
- Generate access key and secret access key
- (For testing only) Create system environment variable:
    - name: qsubtranslator_amazon_key
    - value: value#access_key||1||secret_access_key

**Note**: Free 2 million characters per month per 12 months

### IBM API
- Create an IBM Cloud account
- Create Language Translator Service
- Copy api key and url
- (For testing only) Create system environment variable:
    - name: qsubtranslator_ibm_key
    - value: value#api_key||1||url

**Note**: Free 1 million character per month (account gets deleted for inactivity after 30 days)

## Remarks
This application needs polishing. Code was rushed so it needs refactoring. There are several improvements to be done but for now, it gets the job done.

## Command line (how to use it)
--path "folder_that_has_subtitles" --output-folder "folder_that_will_have_translated_files" --from-lang "en" --to-lang "es" --api "Google" --api-key "key"

`from-lang`: Language that subtitles are in `path`
`to-lang`: Translate subtitiles files to this language
`api`: Translator API (Google and Microsoft supported only)
`api-key`: APi Key for the service provider
- For Amazon `api-key` must follow the following format: `access_key||1||secret_access_key`. In other words: `string.Format("{0}||1||{1}", accessKey, secretAccessKey)`
- For IBM `api-key` must follow the following format: `api_key||1||url`. In other words: `string.Format("{0}||1||{1}", apiKey, url)`

`from-lang` and `to-lang` must match the supported language from translator provider
 - Google: https://cloud.google.com/translate/docs/languages
 - Microsoft: https://docs.microsoft.com/en-us/azure/cognitive-services/translator/language-support
 - IBM: https://cloud.ibm.com/docs/services/language-translator?topic=language-translator-translation-models
 - Amazon: https://docs.aws.amazon.com/translate/latest/dg/what-is.html


### Optional
- `ask-for-retry`: If enabled, it will wait for input if http/service fails before continuing
- `char-limit`: It will stop processing files if it reaches the character limit (helpful if you don't want to go over a certain limit)
- `peek`: If true, it will display a peek of the translated text (last line of each block). Do not set this as true, if you care about spoilers

**Note:** Characters are counted with `new StringInfo(str).LengthInTextElements`

# Supported files
- srt (SubRip)
- sub (Subtitile files like microdvd)
- ssa (substation alpha)
- ttml (timed text markup language)
- vtt (webvtt subtitle)
- xml (youtube subtitle)

(It uses library https://github.com/AlexPoint/SubtitlesParser to parse)

# This work uses
https://github.com/AlexPoint/SubtitlesParser to parse subtitle files (MIT License)