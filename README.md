# Sitecore.LanguageFallback.StandardValueFix

This project provides an alternative processor to retrieve standard values. The processor will utilize the LanguageFallbackFieldValuesManager for fallback items to make sure that the standard values are also in the fallback language.

## Building

* Place your `Sitecore.Kernel.dll` file in the lib directory.

* Build the solution.

## Usage

* Drop the file `Sitecore.LanguageFallback.StandardValueFix.dll` into the bin directory of your sitecore installation

* Activate item-level language fallback. You can use the file `ActivateItemLevelFallback.config` to patch your configuration

* Drop the file `FixLanguageFallbackStandardValues.config` into your patch directory