using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Helper
{
    public static class CloneHelper
    {
        /// <summary>
        /// Extension method to Enitity Object .Cloning 
        /// Cloning the Object .If required this need to be followed by CleatEntity Objects
        /// 
        /// </summary>
        /// <param name="source">Entity Object to be cloned </param>
        /// <returns></returns>
        /// 
        //public static T Clone<T>(this T source) where T : EntityObject
        //{
        //    var obj = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
        //    using (var stream = new System.IO.MemoryStream())
        //    {
        //        obj.WriteObject(stream, source);
        //        stream.Seek(0, System.IO.SeekOrigin.Begin);
        //        return (T)obj.ReadObject(stream);
        //    }
        //}

        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }
        /// <summary>
        /// Extension method of Entity Object .
        /// This will be used  to load all the Releated Child Objects using load (LazyLoad)
        /// 
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// 
        public static EntityObject LoadAllChild(this EntityObject source)
        {
            List<PropertyInfo> PropList = (from a in source.GetType().GetProperties()
                                           where a.PropertyType.Name == "EntityCollection`1"
                                           select a).ToList();
            foreach (PropertyInfo prop in PropList)
            {
                object instance = prop.GetValue(source, null);
                bool isLoad = (bool)instance.GetType().GetProperty("IsLoaded").GetValue(instance, null);
                if (!isLoad)
                {

                    MethodInfo mi = (from a in instance.GetType().GetMethods()
                                     where a.Name == "Load" && a.GetParameters().Length == 0
                                     select a).FirstOrDefault();

                    mi.Invoke(instance, null);
                }
            }
            return (EntityObject)source;
        }



        /// <summary>
        ///  Extension method of Entity Object This will clear the Entity of Object and all related CHild Objects 
        /// 
        /// </summary>
        /// <param name="source">Entity Object on which audit property will be applied</param>
        /// <param name="bcheckHierarchy">This Parameter will define to clear enitty of all Child Object or not to set for child objects</param>
        /// <param name="sModifiedby">This will be used setting Createdby/Modifiedby Attribute</param>
        /// is to be changed </param>
        /// <returns></returns>
        public static EntityObject ClearEntityReference(this EntityObject source, bool bcheckHierarchy)
        {
            return source.ClearEntityObject(bcheckHierarchy);
        }
        private static T ClearEntityObject<T>(this T source, bool bcheckHierarchy) where T : class
        {
            //Throw if passed object has nothing
            if (source == null) { throw new Exception("Null Object cannot be cloned"); }
            // get the TYpe of passed object 
            Type tObj = source.GetType();
            // check object Passed does not have entity key Attribute 
            if (tObj.GetProperty("EntityKey") != null)
            {
                tObj.GetProperty("EntityKey").SetValue(source, null, null);


            }
            //bcheckHierarchy this flag is used to check and clear child object releation keys 
            if (!bcheckHierarchy)
            {
                return (T)source;
            }
            // Clearing the Entity for Child Objects 
            // Using the Linq get only Child Reference objects   from source object 
            List<PropertyInfo> PropList = (from a in source.GetType().GetProperties()
                                           where a.PropertyType.Name.ToUpper() == "ENTITYCOLLECTION`1"
                                           select a).ToList();

            // Loop thorough List of Child Object and Clear the Entity Reference 
            foreach (PropertyInfo prop in PropList)
            {


                IEnumerable keys = (IEnumerable)tObj.GetProperty(prop.Name).GetValue(source, null);

                foreach (object key in keys)
                {
                    //Clearing Entity Reference from Parnet Object
                    var ochildprop = (from a in key.GetType().GetProperties()
                                      where a.PropertyType.Name == "EntityReference`1"
                                      select a).SingleOrDefault();

                    ochildprop.GetValue(key, null).ClearEntityObject(false);

                    //Clearing the the Entity Reference from Child object .This will recrusive action
                    key.ClearEntityObject(true);
                }
            }
            return (T)source;
        }



    }
}