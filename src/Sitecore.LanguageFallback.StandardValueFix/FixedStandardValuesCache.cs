using Sitecore.Caching;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.LanguageFallback.StandardValueFix
{
    /// <summary>
    /// Reimplementation of the StandardValuesCache that utilizes item.OriginalLanguage to build the Key instead of item.Language
    /// </summary>
    public class FixedStandardValuesCache : CustomCache
    {
        #region private properties
        private readonly long _averageValueSize = Settings.Caching.StandardValues.AverageValueSize;
        #endregion

        #region Constructors
        public FixedStandardValuesCache(Database database, long maxSize) : base(database.Name + "[fixedStandardValues]", maxSize)
		{
        }
        #endregion

        /// <summary>
        /// Original Implementation of GetStandardValues
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public SafeDictionary<ID, string> GetStandardValues(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            object key = this.GetKey(item);
            return base.GetObject(key) as SafeDictionary<ID, string>;
        }

        /// <summary>
        ///  Original Implementation of AddStandardValues
        /// </summary>
        /// <param name="item"></param>
        /// <param name="values"></param>
        public void AddStandardValues(Item item, SafeDictionary<ID, string> values)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(values, "values");
            object key = this.GetKey(item);
            base.SetObject(key, values, (long)values.Count * this._averageValueSize);
        }

        /// <summary>
        /// Reimplementation of GetKey. This implementation uses item.OriginalLanguage instead of item.Language to build the cache key
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private object GetKey(Item item)
        {
            return item.TemplateID.ToString() + item.OriginalLanguage;
        }
    }
}
