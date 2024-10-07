using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Reflection;

namespace Qbicles.BusinessRules.Helper
{
    public static class JsonHelper
    {
        private static bool IsShallowType(Type type)
        {
            return
                type.IsPrimitive ||
                new Type[] {
                            typeof(List<int>),
                            typeof(List<string>),
                            typeof(Enum),
                            typeof(String),
                            typeof(Decimal),
                            typeof(DateTime),
                            typeof(DateTimeOffset),
                            typeof(TimeSpan),
                            typeof(Guid),
                            typeof(Boolean)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsShallowType(type.GetGenericArguments()[0]))
                ;
        }

        public static object ShallowCopy(this object obj)
        {
            if (obj == null)
                return null;
            try
            {

                Type objType = obj.GetType();

                if (IsShallowType(objType))
                {
                    return obj;
                }

                PropertyInfo[] properties = objType.GetProperties();
                var simpleTypes = properties.Where(t => IsShallowType(t.PropertyType));

                var newObj = new List<string>();


                foreach (var item in simpleTypes)
                    newObj.Add($"{item.Name} : {item.GetValue(obj)}");

                return newObj;
            }
            catch
            {
                return obj;
            }
        }

        public static string ToJson(this object obj)
        {
            if (obj == null) return "";
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static string ToJson(this object[] obj)
        {
            if (obj == null) return "";
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }


        public static string ToJsonIgnoreReferenceLoop(this object obj)
        {
            if (obj == null) return "";           

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                //ContractResolver= new CustomResolver2(),
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }

        
        public static T ParseAs<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static T ReadAs<T>(this HttpContent content)
        {
            using (var stream = content.ReadAsStreamAsync().Result)
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }

        public static StringContent ToJsonStringContent(this object obj)
        {
            return new StringContent(obj.ToJson(), Encoding.UTF8, "application/json");
        }

        public static Dictionary<string, object> ToDictionary<TInput>(TInput obj)
        {
            if (obj == null) return new Dictionary<string, object>();
            return typeof(TInput).GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(obj, null));
        }
    }
}
