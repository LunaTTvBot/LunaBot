using System;
using System.Collections.Generic;
using System.Linq;
using IBot.Database;
using NLog;

namespace IBot.Models
{
    internal class Extension
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public string ClassName { get; set; }

        public string Id { get; set; }

        public string PropertyName { get; set; }

        public string Value { get; set; }

        public static T Get<T>(IExtendable extendable, string propertyName)
        {
            try
            {
                var db = DatabaseContext.Get();

                var result = db.ObjectExtensions
                               .Where(e => e.ClassName == extendable.ClassName)
                               .Where(e => e.Id == extendable.Id)
                               .FirstOrDefault(e => e.PropertyName == propertyName);

                if (result == null)
                    throw new KeyNotFoundException($"{extendable.ClassName}::{extendable.Id} => {propertyName} not found");

                var objVal = (object) result.Value;

                try
                {
                    return (T) Convert.ChangeType(objVal, typeof(T));
                }
                catch (Exception e)
                {
                    _logger.Warn(e);
                    return default(T);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                return default(T);
            }
        }

        public static void Set<T>(IExtendable extendable, string propertyName, T value)
        {
            try
            {
                var db = DatabaseContext.Get();

                var extension = new Extension()
                {
                    ClassName = extendable.ClassName,
                    Id = extendable.Id,
                    PropertyName = propertyName,
                    Value = Convert.ToString(value)
                };

                db.ObjectExtensions.Add(extension);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
