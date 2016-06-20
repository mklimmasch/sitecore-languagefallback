using Sitecore.Data.LanguageFallback;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetFieldValue;

namespace Sitecore.LanguageFallback.StandardValueFix
{
    public class GetLanguageFallbackStandardValue
    {
        public void Process(GetFieldValueArgs args)
        {
            if (!args.AllowStandardValue)
            {
                return;
            }

            // We use the original code if the item is not a fallback item
            if (!args.Field.Item.IsFallback)
            {
                string standardValue = args.Field.GetStandardValue();
                if (standardValue != null)
                {
                    args.ContainsStandardValue = true;
                    args.Value = standardValue;
                    args.AbortPipeline();
                }
                return;
            }

            // If we have a FallbackItem, we want the standard value to be also in the fallback language
            // (Code taken from Sitecore.Pipelines.GetFieldValue.GetLanguageFallbackValue)
            LanguageFallbackFieldValue languageFallbackValue = LanguageFallbackFieldValuesManager.GetLanguageFallbackValue(args.Field, true);
            if (languageFallbackValue.Value != null)
            {
                args.Value = languageFallbackValue.Value;
                args.ContainsStandardValue = languageFallbackValue.ContainsStandardValue;
                args.ContainsFallbackValue = true;
                args.FallbackSource = languageFallbackValue.FallbackSource;
                args.AbortPipeline();
            }
        }
    }
}