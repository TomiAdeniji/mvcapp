using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.BusinessRules.Model
{
    public class TB_Column
    {
        /// <summary>
        /// Gets the data component (bind property name).
        /// </summary>
        public string Data { get; private set; }
        /// <summary>
        /// Get's the name component (if any provided on client-side script).
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Indicates if the column is searchable or not.
        /// </summary>
        public bool Searchable { get; private set; }
        /// <summary>
        /// Indicates if the column is orderable or not.
        /// </summary>
        public bool Orderable { get; private set; }
        /// <summary>
        /// Gets the search component for the column.
        /// </summary>
        public Search Search { get; private set; }
        /// <summary>
        /// Indicates if the current column should be ordered on server-side or not.
        /// </summary>
        public bool IsOrdered { get { return OrderNumber != -1; } }
        /// <summary>
        /// Indicates the column' position on the ordering (multi-column ordering).
        /// </summary>
        public int OrderNumber { get; private set; }
        /// <summary>
        /// Indicates the column's sort direction.
        /// </summary>
        public OrderDirection SortDirection { get; private set; }
        /// <summary>
        /// Sets the columns ordering.
        /// </summary>
        /// <param name="orderNumber">The column's position on the ordering (multi-column ordering).</param>
        /// <param name="orderDirection">The column's sort direction.</param>
        /// <exception cref="System.ArgumentException">Thrown if the provided orderDirection is not valid.</exception>
        public void SetColumnOrdering(int orderNumber, string orderDirection)
        {
            this.OrderNumber = orderNumber;

            if (orderDirection.ToLower().Equals("asc")) this.SortDirection = TB_Column.OrderDirection.Ascendant;
            else if (orderDirection.ToLower().Equals("desc")) this.SortDirection = TB_Column.OrderDirection.Descendant;
            else throw new ArgumentException("The provided ordering direction was not valid. Valid values must be 'asc' or 'desc' only.");
        }
        /// <summary>
        /// Creates a new DataTables column.
        /// </summary>
        /// <param name="data">The data component (bind property name).</param>
        /// <param name="name">The name of the column (if provided).</param>
        /// <param name="searchable">True if the column allows searching, false otherwise.</param>
        /// <param name="orderable">True if the column allows ordering, false otherwise.</param>
        /// <param name="searchValue">The searched value for the column, or an empty string.</param>
        /// <param name="isRegexValue">True if the search value is a regex value, false otherwise.</param>
        public TB_Column(string data, string name, bool searchable, bool orderable, string searchValue, bool isRegexValue)
        {
            this.Data = data;
            this.Name = name;
            this.Searchable = searchable;
            this.Orderable = orderable;
            this.Search = new Search(searchValue, isRegexValue);

            // Default - indicates that the column should not be ordered on server-side.
            this.OrderNumber = -1;
        }
        /// <summary>
        /// Defines order directions for proper use.
        /// </summary>
        public enum OrderDirection
        {
            /// <summary>
            /// Represents an ascendant (A-Z) ordering.
            /// </summary>
            Ascendant = 0,
            /// <summary>
            /// Represents a descendant (Z-A) ordering.
            /// </summary>
            Descendant = 1
        }
    }
    public class ColumnCollection : IEnumerable<TB_Column>
    {
        /// <summary>
        /// For internal use only.
        /// Stores data.
        /// </summary>
        private IReadOnlyList<TB_Column> Data;
        /// <summary>
        /// Created a new ReadOnlyColumnCollection with predefined data.
        /// </summary>
        /// <param name="columns">The column collection from DataTables.</param>
        public ColumnCollection(IEnumerable<TB_Column> columns)
        {
            if (columns == null) throw new ArgumentNullException("The provided column collection cannot be null", "columns");
            Data = columns.ToList().AsReadOnly();
        }
        /// <summary>
        /// Get sorted columns on client-side already on the same order as the client requested.
        /// The method checks if the column is bound and if it's ordered on client-side.
        /// </summary>
        /// <returns>The ordered enumeration of sorted columns.</returns>
        public IOrderedEnumerable<TB_Column> GetSortedColumns()
        {
            return Data
                .Where(_column => !String.IsNullOrWhiteSpace(_column.Data) && _column.IsOrdered)
                .OrderBy(_c => _c.OrderNumber);
        }
        /// <summary>
        /// Get filtered columns on client-side.
        /// The method checks if the column is bound and if the search has a value.
        /// </summary>
        /// <returns>The enumeration of filtered columns.</returns>
        public IEnumerable<TB_Column> GetFilteredColumns()
        {
            return Data
                .Where(_column => !String.IsNullOrWhiteSpace(_column.Data) && _column.Searchable && !String.IsNullOrWhiteSpace(_column.Search.Value));
        }
        /// <summary>
        /// Returns the enumerable element as defined on IEnumerable.
        /// </summary>
        /// <returns>The enumerable elemento to iterate through data.</returns>
        public IEnumerator<TB_Column> GetEnumerator()
        {
            return Data.GetEnumerator();
        }
        /// <summary>
        /// Returns the enumerable element as defined on IEnumerable.
        /// </summary>
        /// <returns>The enumerable element to iterate through data.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)Data).GetEnumerator();
        }
    }
    public class DataTablesBinder : IModelBinder
    {
        /// <summary>
        /// Formatting to retrieve data for each column.
        /// </summary>
        protected readonly string COLUMN_DATA_FORMATTING = "columns[{0}][data]";
        /// <summary>
        /// Formatting to retrieve name for each column.
        /// </summary>
        protected readonly string COLUMN_NAME_FORMATTING = "columns[{0}][name]";
        /// <summary>
        /// Formatting to retrieve searchable indicator for each column.
        /// </summary>
        protected readonly string COLUMN_SEARCHABLE_FORMATTING = "columns[{0}][searchable]";
        /// <summary>
        /// Formatting to retrieve orderable indicator for each column.
        /// </summary>
        protected readonly string COLUMN_ORDERABLE_FORMATTING = "columns[{0}][orderable]";
        /// <summary>
        /// Formatting to retrieve search value for each column.
        /// </summary>
        protected readonly string COLUMN_SEARCH_VALUE_FORMATTING = "columns[{0}][search][value]";
        /// <summary>
        /// Formatting to retrieve search regex indicator for each column.
        /// </summary>
        protected readonly string COLUMN_SEARCH_REGEX_FORMATTING = "columns[{0}][search][regex]";
        /// <summary>
        /// Formatting to retrieve ordered columns.
        /// </summary>
        protected readonly string ORDER_COLUMN_FORMATTING = "order[{0}][column]";
        /// <summary>
        /// Formatting to retrieve columns order direction.
        /// </summary>
        protected readonly string ORDER_DIRECTION_FORMATTING = "order[{0}][dir]";
        /// <summary>
        /// Binds a new model with the DataTables request parameters.
        /// You should override this method to provide a custom type for internal binding to procees.
        /// </summary>
        /// <param name="controllerContext">The context for the controller.</param>
        /// <param name="bindingContext">The context for the binding.</param>
        /// <returns>Your model with all it's properties set.</returns>
        public virtual object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return Bind(controllerContext, bindingContext, typeof(DefaultDataTablesRequest));
        }
        /// <summary>
        /// Binds a new model with both DataTables and your custom parameters.
        /// You should not override this method unless you're using request methods other than GET/POST.
        /// If that's the case, you'll have to override ResolveNameValueCollection too.
        /// </summary>
        /// <param name="controllerContext">The context for the controller.</param>
        /// <param name="bindingContext">The context for the binding.</param>
        /// <param name="modelType">The type of the model which will be created. Should implement IDataTablesRequest.</param>
        /// <returns>Your model with all it's properties set.</returns>
        protected virtual object Bind(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            var model = (IDataTablesRequest)Activator.CreateInstance(modelType);

            // We could use the `bindingContext.ValueProvider.GetValue("key")` approach but
            // directly accessing the HttpValueCollection will improve performance if you have
            // more than 2 registered value providers.
            NameValueCollection requestParameters = ResolveNameValueCollection(request);

            // Populates the model with the draw count from DataTables.
            model.Draw = Get<int>(requestParameters, "draw");

            // Populates the model with page info (server-side paging).
            model.Start = Get<int>(requestParameters, "start");
            model.Length = Get<int>(requestParameters, "length");

            // Populates the model with search (global search).
            var searchValue = Get<string>(requestParameters, "search[value]");
            var searchRegex = Get<bool>(requestParameters, "search[regex]");
            model.Search = new Search(searchValue, searchRegex);

            // Get's the column collection from the request parameters.
            var columns = GetColumns(requestParameters);

            // Parse column ordering.
            ParseColumnOrdering(requestParameters, columns);

            // Attach columns into the model.
            model.Columns = new ColumnCollection(columns);

            // Map aditional properties into your custom request.
            MapAditionalProperties(model, requestParameters);

            // Returns the filled model.
            return model;
        }
        /// <summary>
        /// Map aditional properties (aditional fields sent with DataTables) into your custom implementation of IDataTablesRequest.
        /// You should override this method to map aditional info (non-standard DataTables parameters) into your custom 
        /// implementation of IDataTablesRequest.
        /// </summary>
        /// <param name="requestModel">The request model which will receive your custom data.</param>
        /// <param name="requestParameters">Parameters sent with the request.</param>
        protected virtual void MapAditionalProperties(IDataTablesRequest requestModel, NameValueCollection requestParameters) { }
        /// <summary>
        /// Resolves the NameValueCollection from the request.
        /// Default implementation supports only GET and POST methods.
        /// You may override this method to support other HTTP verbs.
        /// </summary>
        /// <param name="request">The HttpRequestBase object that represents the MVC request.</param>
        /// <returns>The NameValueCollection with request variables.</returns>
        protected virtual NameValueCollection ResolveNameValueCollection(HttpRequestBase request)
        {
            if (request.HttpMethod.ToLower().Equals("get")) return request.QueryString;
            else if (request.HttpMethod.ToLower().Equals("post")) return request.Form;
            else throw new ArgumentException(String.Format("The provided HTTP method ({0}) is not a valid method to use with DataTablesBinder. Please, use HTTP GET or POST methods only.", request.HttpMethod), "method");
        }
        /// <summary>
        /// Get's a typed value from the collection using the provided key.
        /// This method is provided as an option for you to override the default behavior and add aditional
        /// check or change the returned value.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key to access the collection item.</param>
        /// <returns>The stringly-typed object.</returns>
        protected virtual T Get<T>(NameValueCollection collection, string key)
        {
            return collection.G<T>(key);
        }
        /// <summary>
        /// Return's the column collection from the request values.
        /// This method is provided as an option for you to override the default behavior and add aditional
        /// check or change the returned value.
        /// </summary>
        /// <param name="collection">The request value collection.</param>
        /// <returns>The collumn collection or an empty list. For default behavior, do not return null!</returns>
        protected virtual List<TB_Column> GetColumns(NameValueCollection collection)
        {
            try
            {
                var columns = new List<TB_Column>();

                // Loop through every request parameter to avoid missing any DataTable column.
                for (int i = 0; i < collection.Count; i++)
                {
                    var columnData = Get<string>(collection, String.Format(COLUMN_DATA_FORMATTING, i));
                    var columnName = Get<string>(collection, String.Format(COLUMN_NAME_FORMATTING, i));

                    if (columnData != null && columnName != null)
                    {
                        var columnSearchable = Get<bool>(collection, String.Format(COLUMN_SEARCHABLE_FORMATTING, i));
                        var columnOrderable = Get<bool>(collection, String.Format(COLUMN_ORDERABLE_FORMATTING, i));
                        var columnSearchValue = Get<string>(collection, String.Format(COLUMN_SEARCH_VALUE_FORMATTING, i));
                        var columnSearchRegex = Get<bool>(collection, String.Format(COLUMN_SEARCH_REGEX_FORMATTING, i));

                        columns.Add(new TB_Column(columnData, columnName, columnSearchable, columnOrderable, columnSearchValue, columnSearchRegex));
                    }
                    else break; // Stops iterating because there's no more columns.
                }

                return columns;
            }
            catch
            {
                // Returns an empty column collection to avoid null exceptions.
                return new List<TB_Column>();
            }
        }
        /// <summary>
        /// Configure column's ordering.
        /// This method is provided as an option for you to override the default behavior.
        /// </summary>
        /// <param name="collection">The request value collection.</param>
        /// <param name="columns">The column collection as returned from GetColumns method.</param>
        protected virtual void ParseColumnOrdering(NameValueCollection collection, IEnumerable<TB_Column> columns)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                var orderColumn = Get<int>(collection, String.Format(ORDER_COLUMN_FORMATTING, i));
                var orderDirection = Get<string>(collection, String.Format(ORDER_DIRECTION_FORMATTING, i));

                if (orderColumn > -1 && orderDirection != null)
                    columns.ElementAt(orderColumn).SetColumnOrdering(i, orderDirection);
            }
        }
    }
    public abstract class DataTablesJsonBinder : IModelBinder
    {
        /// <summary>
        /// Get's the JSON parameter name to retrieve data. 
        /// You may override this to change to your parameter.
        /// </summary>
        protected virtual string JSON_PARAMETER_NAME { get { return "json"; } }
        /// <summary>
        /// Binds a new model with the DataTables request parameters.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            var requestMethod = request.HttpMethod.ToLower();
            var requestType = request.ContentType;

            // If the request type does not contains "JSON", it's supposed not to be a JSON request so we skip.
            if (!IsJsonRequest(request))
                throw new ArgumentException("You must provide a JSON request and set it's content type to match a JSON request content type.");

            // Desserializes the JSON request using the .Net Json implementation.
            return Deserialize(bindingContext.ValueProvider.GetValue(JSON_PARAMETER_NAME).AttemptedValue);
        }
        /// <summary>
        /// Checks if a request is a JsonRequest or not. 
        /// You may override this to check for other values or indicators.
        /// </summary>
        /// <param name="request">The HttpRequestBase object representing the MVC request.</param>
        /// <returns>True if the ContentType contains "json", False otherwise.</returns>
        public virtual bool IsJsonRequest(HttpRequestBase request)
        {
            return request.ContentType.ToLower().Contains("json");
        }
        /// <summary>
        /// When overriden, deserializes the JSON data into a DataTablesRequest object.
        /// </summary>
        /// <param name="jsonData">The JSON data to be deserialized.</param>
        /// <returns>The DataTablesRequest object.</returns>
        protected abstract IDataTablesRequest Deserialize(string jsonData);
    }
    /// <summary>
    /// Represents a server-side response for use with DataTables.
    /// </summary>
    /// <remarks>
    /// Variable syntax matches DataTables names to avoid error and avoid aditional parse.
    /// </remarks>
    public class DataTablesResponse
    {
        /// <summary>
        /// Gets the draw counter for DataTables.
        /// </summary>
        public int draw { get; private set; }
        /// <summary>
        /// Gets the data collection.
        /// </summary>
        public IEnumerable data { get; private set; }
        /// <summary>
        /// Gets the total number of records (without filtering - total dataset).
        /// </summary>
        public int recordsTotal { get; private set; }
        /// <summary>
        /// Gets the resulting number of records after filtering.
        /// </summary>
        public int recordsFiltered { get; private set; }

        public object customResponse { get; private set; }
        /// <summary>
        /// Creates a new DataTables response object with it's elements.
        /// </summary>
        /// <param name="draw">The draw counter as received from the DataTablesRequest.</param>
        /// <param name="data">The data collection (data page).</param>
        /// <param name="recordsFiltered">The resulting number of records after filtering.</param>
        /// <param name="recordsTotal">The total number of records (total dataset).</param>
        public DataTablesResponse(int draw, IEnumerable data, int recordsFiltered, int recordsTotal, object customResponse = null)
        {
            this.draw = draw;
            this.data = data;
            this.recordsFiltered = recordsFiltered;
            this.recordsTotal = recordsTotal;
            this.customResponse = customResponse;
        }
    }
    /// <summary>
    /// Implements a default DataTables request.
    /// </summary>
    public class DefaultDataTablesRequest : IDataTablesRequest
    {
        /// <summary>
        /// Gets/Sets the draw counter from DataTables.
        /// </summary>
        public virtual int Draw { get; set; }
        /// <summary>
        /// Gets/Sets the start record number (jump) for paging.
        /// </summary>
        public virtual int Start { get; set; }
        /// <summary>
        /// Gets/Sets the length of the page (paging).
        /// </summary>
        public virtual int Length { get; set; }
        /// <summary>
        /// Gets/Sets the global search term.
        /// </summary>
        public virtual Search Search { get; set; }
        /// <summary>
        /// Gets/Sets the column collection.
        /// </summary>
        public virtual ColumnCollection Columns { get; set; }
    }
    /// <summary>
    /// Defines a server-side request for use with DataTables.
    /// </summary>
    /// <remarks>
    /// Variable syntax does NOT match DataTables names because auto-mapping won't work anyway.
    /// Use the DataTablesModelBinder or provide your own binder to bind your model with DataTables's request.
    /// </remarks>
    public interface IDataTablesRequest
    {
        /// <summary>
        /// Gets and sets the draw counter from client-side to give back on the server's response.
        /// </summary>
        int Draw { get; set; }
        /// <summary>
        /// Gets and sets the start record number (count) for paging.
        /// </summary>
        int Start { get; set; }
        /// <summary>
        /// Gets and sets the length of the page (max records per page).
        /// </summary>
        int Length { get; set; }
        /// <summary>
        /// Gets and sets the global search pagameters.
        /// </summary>
        Search Search { get; set; }
        /// <summary>
        /// Gets and sets the read-only collection of client-side columns with their options and configs.
        /// </summary>
        ColumnCollection Columns { get; set; }
    }
    public class DataTablesRequest
    {/// <summary>
     /// Gets and sets the draw counter from client-side to give back on the server's response.
     /// </summary>
        public int Draw { get; set; }
        /// <summary>
        /// Gets and sets the start record number (count) for paging.
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// Gets and sets the length of the page (max records per page).
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// Gets and sets the global search pagameters.
        /// </summary>
        public Search Search { get; set; }
        /// <summary>
        /// Gets and sets the read-only collection of client-side columns with their options and configs.
        /// </summary>
        public ColumnCollection Columns { get; set; }
    }
    /// <summary>
    /// Provides extension methods for use with NameValueCollections.
    /// </summary>
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Gets a typed item from the collection using the provided key.
        /// If there's no corresponding item on the collection, returns default(T).
        /// </summary>
        /// <typeparam name="T">The type to cast the collection item.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key to access the item inside the collection.</param>
        /// <returns>The typed item.</returns>
        public static T G<T>(this NameValueCollection collection, string key) { return G<T>(collection, key, default(T)); }
        /// <summary>
        /// Gets a typed item from the collection using the provided key.
        /// If there's no corresponding item on the collection, returns the provided defaultValue.
        /// </summary>
        /// <typeparam name="T">The type to cast the collection item.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key to access the item inside the collection.</param>
        /// <param name="defaultValue">The default value to return if there's no corresponding item on the collection.</param>
        /// <returns>The typed item.</returns>
        public static T G<T>(this NameValueCollection collection, string key, object defaultValue)
        {
            if (collection == null) throw new ArgumentNullException("collection", "The provided collection cannot be null.");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("The provided key cannot be null or empty.", "key");

            var collectionItem = collection[key];
            if (collectionItem == null) return (T)defaultValue;
            return (T)Convert.ChangeType(collectionItem, typeof(T));
        }
        /// <summary>
        /// Sets or updates a value inside the provided collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key to access the item inside the collection.</param>
        /// <param name="value">The value to be set or updated.</param>
        public static void S(this NameValueCollection collection, string key, object value)
        {
            if (collection == null) throw new ArgumentNullException("collection", "The provided collection cannot be null.");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("The provided key cannot be null or empty.", "key");
            if (value == null) throw new ArgumentNullException("value", "The provided value cannot be null.");

            if (collection.Keys.Cast<string>().Any(_k => _k.Equals(key)))
                collection[key] = value.ToString();
            else
                collection.Add(key, value.ToString());
        }
        /// <summary>
        /// Stores parameters and configs from DataTables search engine.
        /// </summary>
        public class Search
        {
            /// <summary>
            /// Gets the value of the search.
            /// </summary>
            public string Value { get; private set; }
            /// <summary>
            /// Indicates if the value of the search is a regex value or not.
            /// </summary>
            public bool IsRegexValue { get; private set; }
            /// <summary>
            /// Creates a new search values holder object.
            /// </summary>
            /// <param name="value">The value of the search.</param>
            /// <param name="isRegexValue">True if the value is a regex value or false otherwise.</param>
            /// <exception cref="System.ArgumentNullException">Thrown when the provided search value is null.</exception>
            public Search(string value, bool isRegexValue)
            {
                if (value == null) throw new ArgumentNullException("value", "The value of the search cannot be null. If there's no search performed, provide an empty string.");

                this.Value = value;
                this.IsRegexValue = isRegexValue;
            }
        }
    }
    /// <summary>
    /// Stores parameters and configs from DataTables search engine.
    /// </summary>
    public class Search
    {
        /// <summary>
        /// Gets the value of the search.
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// Indicates if the value of the search is a regex value or not.
        /// </summary>
        public bool IsRegexValue { get; private set; }
        /// <summary>
        /// Creates a new search values holder object.
        /// </summary>
        /// <param name="value">The value of the search.</param>
        /// <param name="isRegexValue">True if the value is a regex value or false otherwise.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the provided search value is null.</exception>
        public Search(string value, bool isRegexValue)
        {
            if (value == null) //throw new ArgumentNullException("value", "The value of the search cannot be null. If there's no search performed, provide an empty string.");
                value = "";
            this.Value = value != string.Empty ? value.ToLower().Trim() : "";
            this.IsRegexValue = isRegexValue;
        }
    }
}
