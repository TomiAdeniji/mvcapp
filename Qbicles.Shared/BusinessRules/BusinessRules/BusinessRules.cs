using CleanBooksData;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;

namespace Qbicles.BusinessRules
{
    public static class BusinessLogic
    {
        public static List<T> BusinessMapping<T>(this List<T> objList, string timeZone)
        {
            if (typeof(T) == typeof(Topic))
            {
                foreach (var item in objList as List<Topic>)
                {
                    item.Name = item.Name;
                }
                return objList;
            }

            if (string.IsNullOrWhiteSpace(timeZone))
                return objList;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);


            if (typeof(T) == typeof(Qbicle))
            {
                foreach (var item in objList as List<Qbicle>)
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    item.LastUpdated = item.LastUpdated.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleActivity))
            {
                foreach (var item in objList as List<QbicleActivity>)
                {
                    if (item is QbicleTask)
                    {
                        var tk = (QbicleTask)item;
                        tk.StartedDate = tk.StartedDate.ConvertTimeFromUtc(tz);
                        tk.TimeLineDate = tk.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (tk.ClosedDate.HasValue)
                            tk.ClosedDate = (DateTime)tk.ClosedDate.Value.ConvertTimeFromUtc(tz);
                        if (tk.DueDate.HasValue)
                            tk.DueDate = (DateTime)tk.DueDate.Value.ConvertTimeFromUtc(tz);
                        if (tk.ProgrammedStart.HasValue)
                            tk.ProgrammedStart = (DateTime)tk.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                        if (tk.ProgrammedEnd.HasValue)
                            tk.ProgrammedEnd = (DateTime)tk.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                        if (tk.ActualStart.HasValue)
                            tk.ActualStart = (DateTime)tk.ActualStart.Value.ConvertTimeFromUtc(tz);
                        if (tk.ActualEnd.HasValue)
                            tk.ActualEnd = (DateTime)tk.ActualEnd.Value.ConvertTimeFromUtc(tz);
                    }
                    else if (item is QbicleAlert)
                    {
                        var al = (QbicleAlert)item;
                        al.StartedDate = al.StartedDate.ConvertTimeFromUtc(tz);
                        al.TimeLineDate = al.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (al.ClosedDate.HasValue)
                            al.ClosedDate = (DateTime)al.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    }

                    else if (item is QbicleEvent)
                    {
                        var ev = (QbicleEvent)item;
                        ev.Start = ev.Start.ConvertTimeFromUtc(tz);
                        ev.End = ev.End.ConvertTimeFromUtc(tz);
                        ev.StartedDate = ev.StartedDate.ConvertTimeFromUtc(tz);
                        ev.TimeLineDate = ev.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (ev.ClosedDate.HasValue)
                            ev.ClosedDate = (DateTime)ev.ClosedDate.Value.ConvertTimeFromUtc(tz);
                        if (ev.ProgrammedStart.HasValue)
                            ev.ProgrammedStart = (DateTime)ev.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                        if (ev.ProgrammedEnd.HasValue)
                            ev.ProgrammedEnd = (DateTime)ev.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                        if (ev.ActualStart.HasValue)
                            ev.ActualStart = (DateTime)ev.ActualStart.Value.ConvertTimeFromUtc(tz);
                        if (ev.ActualEnd.HasValue)
                            ev.ActualEnd = (DateTime)ev.ActualEnd.Value.ConvertTimeFromUtc(tz);
                    }
                    else if (item is QbicleMedia)
                    {
                        var me = (QbicleMedia)item;
                        me.StartedDate = me.StartedDate.ConvertTimeFromUtc(tz);
                        me.TimeLineDate = me.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (me.ClosedDate.HasValue)
                            me.ClosedDate = (DateTime)me.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    }
                    else if (item is ApprovalReq)
                    {
                        var app = (ApprovalReq)item;
                        app.StartedDate = app.StartedDate.ConvertTimeFromUtc(tz);
                        app.TimeLineDate = app.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (app.ClosedDate.HasValue)
                            app.ClosedDate = (DateTime)app.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    }
                    else if (item is QbicleLink)
                    {
                        var lk = (QbicleLink)item;
                        lk.StartedDate = lk.StartedDate.ConvertTimeFromUtc(tz);
                        lk.TimeLineDate = lk.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (lk.ClosedDate.HasValue)
                            lk.ClosedDate = (DateTime)lk.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    }
                    else if (item is QbicleDiscussion)
                    {
                        var dc = (QbicleDiscussion)item;
                        dc.StartedDate = dc.StartedDate.ConvertTimeFromUtc(tz);
                        dc.TimeLineDate = dc.TimeLineDate.ConvertTimeFromUtc(tz);
                        if (dc.ClosedDate.HasValue)
                            dc.ClosedDate = (DateTime)dc.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    }
                }
            }

            else if (typeof(T) == typeof(QbicleTask))
            {
                foreach (var item in (objList as List<QbicleTask>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    if (item.DueDate.HasValue)
                        item.DueDate = (DateTime)item.DueDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                    if (item.ProgrammedStart.HasValue)
                        item.ProgrammedStart = (DateTime)item.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                    if (item.ProgrammedEnd.HasValue)
                        item.ProgrammedEnd = (DateTime)item.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                    if (item.ActualStart.HasValue)
                        item.ActualStart = (DateTime)item.ActualStart.Value.ConvertTimeFromUtc(tz);
                    if (item.ActualEnd.HasValue)
                        item.ActualEnd = (DateTime)item.ActualEnd.Value.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleAlert))
            {
                foreach (var item in (objList as List<QbicleAlert>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleEvent))
            {
                foreach (var item in (objList as List<QbicleEvent>))
                {

                    item.Start = item.Start.ConvertTimeFromUtc(tz);
                    item.End = item.End.ConvertTimeFromUtc(tz);

                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                    if (item.ProgrammedStart.HasValue)
                        item.ProgrammedStart = (DateTime)item.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                    if (item.ProgrammedEnd.HasValue)
                        item.ProgrammedEnd = (DateTime)item.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                    if (item.ActualStart.HasValue)
                        item.ActualStart = (DateTime)item.ActualStart.Value.ConvertTimeFromUtc(tz);
                    if (item.ActualEnd.HasValue)
                        item.ActualEnd = (DateTime)item.ActualEnd.Value.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleMedia))
            {
                foreach (var item in (objList as List<QbicleMedia>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(Approval))
            {
                foreach (var item in (objList as List<Approval>))
                {
                    item.CreatedDate = item.CreatedDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(ApprovalReq))
            {
                foreach (var item in (objList as List<ApprovalReq>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbiclePost))
            {
                foreach (var item in objList as List<QbiclePost>)
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.TimeLineDate == null)
                        item.TimeLineDate = item.StartedDate;
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleLink))
            {
                foreach (var item in (objList as List<QbicleLink>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }

            else if (typeof(T) == typeof(QbicleDiscussion))
            {
                foreach (var item in (objList as List<QbicleDiscussion>))
                {
                    item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                    if (item.ClosedDate.HasValue)
                        item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                }
            }
            return objList;
        }
        public static T BusinessMapping<T>(this T objList, string timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                return objList;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            if (typeof(T) == typeof(QbicleActivity))
            {
                var item = (objList as QbicleActivity);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                if (item.ProgrammedStart.HasValue)
                    item.ProgrammedStart = (DateTime)item.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                if (item.ProgrammedEnd.HasValue)
                    item.ProgrammedEnd = (DateTime)item.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                if (item.ActualStart.HasValue)
                    item.ActualStart = (DateTime)item.ActualStart.Value.ConvertTimeFromUtc(tz);
                if (item.ActualEnd.HasValue)
                    item.ActualEnd = (DateTime)item.ActualEnd.Value.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(Qbicle))
            {
                var item = (objList as Qbicle);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                item.LastUpdated = item.LastUpdated.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
            }


            else if (typeof(T) == typeof(QbicleTask))
            {
                var item = (objList as QbicleTask);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                if (item.DueDate.HasValue)
                {
                    item.DueDate = (DateTime)item.DueDate.Value.ConvertTimeFromUtc(tz);
                }
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                if (item.ProgrammedStart.HasValue)
                    item.ProgrammedStart = (DateTime)item.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                if (item.ProgrammedEnd.HasValue)
                    item.ProgrammedEnd = (DateTime)item.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                if (item.ActualStart.HasValue)
                    item.ActualStart = (DateTime)item.ActualStart.Value.ConvertTimeFromUtc(tz);
                if (item.ActualEnd.HasValue)
                    item.ActualEnd = (DateTime)item.ActualEnd.Value.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(QbicleAlert))
            {
                var item = (objList as QbicleAlert);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(QbicleEvent))
            {
                var item = (objList as QbicleEvent);
                item.Start = item.Start.ConvertTimeFromUtc(tz);
                item.End = item.End.ConvertTimeFromUtc(tz);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                if (item.ProgrammedStart.HasValue)
                    item.ProgrammedStart = (DateTime)item.ProgrammedStart.Value.ConvertTimeFromUtc(tz);
                if (item.ProgrammedEnd.HasValue)
                    item.ProgrammedEnd = (DateTime)item.ProgrammedEnd.Value.ConvertTimeFromUtc(tz);
                if (item.ActualStart.HasValue)
                    item.ActualStart = (DateTime)item.ActualStart.Value.ConvertTimeFromUtc(tz);
                if (item.ActualEnd.HasValue)
                    item.ActualEnd = (DateTime)item.ActualEnd.Value.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(QbicleMedia))
            {
                var item = (objList as QbicleMedia);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
                foreach (var verfile in item.VersionedFiles)
                {
                    verfile.UploadedDate = verfile.UploadedDate.ConvertTimeFromUtc(tz);
                }
            }
            else if (typeof(T) == typeof(QbicleLink))
            {
                var item = (objList as QbicleLink);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
            }
            else if (typeof(T) == typeof(QbicleDiscussion))
            {
                var item = (objList as QbicleDiscussion);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
            }
            else if (typeof(T) == typeof(Approval))
            {
                var item = (objList as Approval);
                item.CreatedDate = item.CreatedDate.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(ApprovalReq))
            {
                var item = (objList as ApprovalReq);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                if (item.ClosedDate.HasValue)
                    item.ClosedDate = (DateTime)item.ClosedDate.Value.ConvertTimeFromUtc(tz);
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
            }

            else if (typeof(T) == typeof(QbiclePost))
            {
                var item = (objList as QbiclePost);
                item.StartedDate = item.StartedDate.ConvertTimeFromUtc(tz);
                
                item.TimeLineDate = item.TimeLineDate.ConvertTimeFromUtc(tz);
            }
            else if (typeof(T) == typeof(VersionedFile))
            {
                var item = (objList as VersionedFile);
                item.UploadedDate = item.UploadedDate.ConvertTimeFromUtc(tz);
            }
            else if (typeof(T) == typeof(Account))
            {
                var item = (objList as Account);
                item.CreatedDate = item.CreatedDate.Value.ConvertTimeFromUtc(tz);
            }
            return objList;
        }
        public static List<QbicleActivity> QbicleActivityTimeUtc<T>(this List<T> objList, string timeZone) where T : QbicleActivity
        {
            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            List<QbicleActivity> myList = new List<QbicleActivity>();
            myList.AddRange(objList);

            if (typeof(T) == typeof(QbicleTask))
            {
                foreach (var item in myList)
                {
                    var dt = item.StartedDate.ConvertTimeFromUtc(tz);
                    item.StartedDate = dt;
                    var dt2 = item.TimeLineDate.ConvertTimeFromUtc(tz);
                    item.TimeLineDate = dt2;

                }
            }
            else if (typeof(T) == typeof(QbicleDiscussion))
            {
                myList.Add(new QbicleDiscussion());
            }
            return myList;
        }

        public static DateTime ConvertTimeFromUtc(this DateTime objList, string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return objList.ConvertTimeFromUtc(tz);
        }
        public static string ConvertTimeFromUtc(this DateTime objList, string timeZone, string dateTimeFormat)
        {
            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return objList.ConvertTimeFromUtc(tz).ToString(dateTimeFormat);
        }
        public static DateTime? ConvertTimeFromUtc(this DateTime? objList, string timeZone)
        {
            if (objList == null) return null;

            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return ((DateTime)objList).ConvertTimeFromUtc(tz);
        }
        public static string ConvertTimeFromUtc(this DateTime? objList, string timeZone, string dateTimeFormat)
        {
            if (objList == null) return null;

            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return ((DateTime)objList).ConvertTimeFromUtc(tz).ToString(dateTimeFormat);
        }

        public static DateTime ConvertTimeFromUtc(this DateTime objList, TimeZoneInfo timeZone)
        {
            objList = new DateTime(objList.Ticks, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeFromUtc(objList, timeZone);
        }

        public static string AddSpacesToSentence(this string str)
        {
            return string.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
        public static DateTime ConvertTimeToUtc(this DateTime objList, string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return objList.ConvertTimeToUtc(tz);
        }
        public static DateTime? ConvertTimeToUtc(this DateTime? objList, string timeZone)
        {
            if (objList == null) return null;

            if (string.IsNullOrEmpty(timeZone))
                timeZone = "W. Europe Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return ((DateTime)objList).ConvertTimeToUtc(tz);
        }

        public static DateTime ConvertTimeToUtc(this DateTime objList, TimeZoneInfo timeZone)
        {
            objList = new DateTime(objList.Ticks, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(objList, timeZone);
        }

        public static string FormatDateTimeByUser(this DateTime objectList, string dateFomat = "", string timeFormat = "")
        {
            if (!string.IsNullOrEmpty(dateFomat))
            {
                dateFomat = dateFomat.Replace('D', 'd').Replace('Y', 'y');
                if (!string.IsNullOrEmpty(timeFormat))
                {
                    dateFomat += (' ' + timeFormat);
                }
                return objectList.ToString(dateFomat);
            }

            return objectList.ToString("dd/MM/yyyy");
        }
        public static IQueryable<ProductUnit> SortOrderItemUnitDetails(this IQueryable<ProductUnit> productUnits, IOrderedEnumerable<TB_Column> sortedColumns)
        {
            foreach (var column in sortedColumns)
            {
                switch (column.Name)
                {
                    case "Item":
                    case "Unit":
                        switch (column.SortDirection)
                        {
                            case TB_Column.OrderDirection.Ascendant:
                                productUnits = productUnits.OrderBy(column.Name.GenerateSortItemExp<string>());
                                break;
                            case TB_Column.OrderDirection.Descendant:
                                productUnits = productUnits.OrderByDescending(column.Name.GenerateSortItemExp<string>());
                                break;
                            default:
                                productUnits = productUnits.OrderBy("CreatedDate".GenerateSortItemExp<DateTime>());
                                break;
                        }
                        break;
                    default:
                        productUnits = productUnits.OrderBy("CreatedDate".GenerateSortItemExp<DateTime>());
                        break;
                }
            }

            return productUnits;
        }

        public static Expression<Func<ProductUnit, TResult>> GenerateSortItemExp<TResult>(this string orderBy)
        {
            object result;
            switch (orderBy)
            {
                case "Item":
                    Expression<Func<ProductUnit, string>> itemName = t => t.Item.Name;
                    result = itemName;
                    break;
                case "Unit":
                    Expression<Func<ProductUnit, string>> unitName = t => t.Name;
                    result = unitName;
                    break;
                default:
                    Expression<Func<ProductUnit, DateTime>> createdDate = t => t.Item.CreatedDate;
                    result = createdDate;
                    break;
            }
            return (Expression<Func<ProductUnit, TResult>>)result;
        }

        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            yield return value;
        }
    }
}
