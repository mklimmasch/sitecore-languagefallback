using Sitecore.Collections;
using Sitecore.Common;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.LanguageFallback;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System.Collections.Generic;

namespace Sitecore.LanguageFallback.StandardValueFix
{
    /// <summary>
    /// Replacement implementation for the Sitecore StandardValuesProvider that uses OriginalLanguage instead of Language.
    /// 
    /// Reasons:
    /// - Item Level fallback should never lead to mixed-language content
    /// - If item language fallback is deactivated, OriginalLanguage = Language
    /// - If an item is not a fallback variant, OriginalLanguage = Language
    /// - If an item is the fallback variant, OriginalLanguage should be taken to determine the standard value
    /// </summary>
    public class FixedStandardValuesProvider : StandardValuesProvider
    {
        #region private properties
        private Dictionary<Database, FixedStandardValuesCache> Caches;
        #endregion

        #region Constructors
        public FixedStandardValuesProvider() : base() {
            Caches = new Dictionary<Database, FixedStandardValuesCache>();
        }
        #endregion

        #region private helpers

        /// <summary>
        /// Retrieves an instance of the FixedStandardValueCache, depending on the database
        /// </summary>
        /// <param name="database">The database</param>
        /// <returns>an instance of FixedStandardValuesCache</returns>
        private FixedStandardValuesCache GetCache(Database database)
        {
            FixedStandardValuesCache cache;
            if (Caches.TryGetValue(database, out cache))
                return cache;

            cache = new FixedStandardValuesCache(database, database.Caches.StandardValuesCache.InnerCache.MaxSize);
            Caches.Add(database, cache);
            return cache;
        }

        #endregion

        /// <summary>
        /// Override of GetStandardValue to utilize the new implementation of GetStandardValues
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override string GetStandardValue(Field field)
        {
            if (field.ID == FieldIDs.SourceItem || field.ID == FieldIDs.Source)
            {
                return string.Empty;
            }
            SafeDictionary<ID, string> standardValues = this.GetStandardValues(field.Item);
            if (standardValues == null)
            {
                return null;
            }
            return standardValues[field.ID];
        }

        /// <summary>
        /// Reimplementation of GetStandardValues. This implementation uses item.OriginalLanguage to determine the standard values
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>SafeDictionary of standard values</returns>
        private SafeDictionary<ID, string> GetStandardValues(Item item)
        {
            if (ID.IsNullOrEmpty(item.TemplateID))
            {
                return null;
            }

            SafeDictionary<ID, string> safeDictionary = null;
            this.GetStandardValuesFromCache(item);
            if (safeDictionary != null)
            {
                return safeDictionary;
            }

            safeDictionary = this.ReadStandardValues(item.TemplateID, item.Database, item.OriginalLanguage);

            this.AddStandardValuesToCache(item, safeDictionary);
            return safeDictionary;
        }

        /// <summary>
        /// Original Implementation of ReadStandardValues.
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="database"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private SafeDictionary<ID, string> ReadStandardValues(ID templateId, Database database, Language language)
        {
            SafeDictionary<ID, string> result = new SafeDictionary<ID, string>();
            Template template = TemplateManager.GetTemplate(templateId, database);
            if (template == null)
            {
                return result;
            }
            this.AddStandardValues(template, database, language, result);
            TemplateList baseTemplates = template.GetBaseTemplates();
            foreach (Template template2 in baseTemplates)
            {
                this.AddStandardValues(template2, database, language, result);
            }
            return result;
        }

        /// <summary>
        /// Original Implementation of AddStandardValues
        /// </summary>
        /// <param name="template"></param>
        /// <param name="database"></param>
        /// <param name="language"></param>
        /// <param name="result"></param>
        private void AddStandardValues(Template template, Database database, Language language, SafeDictionary<ID, string> result)
        {
            ID standardValueHolderId = template.StandardValueHolderId;
            if (ID.IsNullOrEmpty(standardValueHolderId))
            {
                return;
            }
            bool? currentValue = Switcher<bool?, LanguageFallbackItemSwitcher>.CurrentValue;
            Item item;
            if (currentValue == false)
            {
                try
                {
                    Switcher<bool?, LanguageFallbackItemSwitcher>.Exit();
                    item = ItemManager.GetItem(standardValueHolderId, language, Version.Latest, database, SecurityCheck.Disable);
                    goto IL_5A;
                }
                finally
                {
                    Switcher<bool?, LanguageFallbackItemSwitcher>.Enter(currentValue);
                }
            }
            item = ItemManager.GetItem(standardValueHolderId, language, Version.Latest, database, SecurityCheck.Disable);
        IL_5A:
            if (item == null)
            {
                return;
            }
            foreach (Field field in item.Fields)
            {
                if (!result.ContainsKey(field.ID))
                {
                    string value = field.GetValue(false, true);
                    if (value != null)
                    {
                        result[field.ID] = value;
                    }
                }
            }
            if (!result.ContainsKey(FieldIDs.StandardValueHolderId))
            {
                result[FieldIDs.StandardValueHolderId] = standardValueHolderId.ToString();
            }
        }

        /// <summary>
        /// Reimplementation of GetStandardValuesFromCache to utilize the new Cache
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private SafeDictionary<ID, string> GetStandardValuesFromCache(Item item)
        {
            return GetCache(item.Database).GetStandardValues(item);
        }

        /// <summary>
        /// Reimplementation of AddStandardValuesToCache to utilize the new Cache
        /// </summary>
        /// <param name="item"></param>
        /// <param name="values"></param>
        private void AddStandardValuesToCache(Item item, SafeDictionary<ID, string> values)
        {
            GetCache(item.Database).AddStandardValues(item, values);
        }
    }
}