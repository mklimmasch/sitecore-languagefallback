# Sitecore.LanguageFallback.StandardValueFix

This project tries to fix Sitecores behaviour of retrieving standard values in the given language in case of item language fallback.

In the current Sitecore version retrieving a fallback item can lead to mixed-language content because sitecore returns the standard values always in the passed language instead of the fallback language

## How it works

We simply replace the default StandardValuesProvider with our own implementation that uses item.OriginalLanguage instead of item.Language. We also have to use our own implementation of the StandardValuesCache, because otherwise the cache key is still using item.Langugae

This behaviour is based on the following assumptions:
* Item Level fallback should never lead to mixed-language content
* If item language fallback is deactivated, item.OriginalLanguage is always identical to item.Language
* If an item is not a fallback variant, item.OriginalLanguage is always identical to item.Language
* If an item is the fallback variant, OriginalLanguage should be taken to determine the standard value

## Building

* Place your `Sitecore.Kernel.dll` file in the lib\Sitecore directory

* Build the solution

## Usage

* Drop the file `Sitecore.LanguageFallback.StandardValueFix.dll` into the bin directory of your sitecore installation

* Activate item-level language fallback. You can use the file `ActivateItemLevelFallback.config` to patch your configuration

* Drop the file `FixLanguageFallbackStandardValues.config` into your patch directory