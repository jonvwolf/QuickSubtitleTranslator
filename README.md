# A quick subtitle file translator
A simple .NET Core 3.1 console app that translate subtitles files (.srt, .sub, etc.) using Google and Microsoft APIs translator services.

# Instructions

## Setup language translator services

### Google API steps
- Create a Google cloud account
- Create a project
- Enable Cloud Translation API
- Create API key
- (For testing only) Create system environment variable:
    - name: qsubtranslator_google_key
    - value: value#api_key_value_goes_here
- Set your limits based on your subtitles files

**Note**: Free 500k characters

### Microsoft API
- Create azure account
- Create cognitive translator text resource
- Copy Api Key
- (For testing only) Create system environment variable:
    - name: qsubtranslator_microsoft_key
    - value: value#api_key_value_goes_here

**Note**: Free 2 million characters

## Remarks
This application needs polishing. Code was rushed so it needs refactoring. There are several improvements to be done but for now, it gets the job done.

## Command line
--path "folder_that_has_subtitles" --output-folder "folder_that_will_have_translated_files" --from-lang "en" --to-lang "es" --api "Google"

`from-lang`: Language that subtitles are in `path`
`to-lang`: Translate subtitiles files to this language
`api`: Translator API (Google and Microsoft supported only)

`from-lang` and `to-lang` must match the supported language from translator provider
 - Google: https://cloud.google.com/translate/docs/languages
 - Microsoft: https://docs.microsoft.com/en-us/azure/cognitive-services/translator/language-support

### Optional
`overwrite`: Default true. Overwrite translated subtitles files

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