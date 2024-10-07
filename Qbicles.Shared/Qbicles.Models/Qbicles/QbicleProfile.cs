using System;
using System.Collections.Generic;

namespace Qbicles.Models
{
    /// <summary>
    /// A first attempt at a user profile class
    /// </summary>
    public class QbicleProfile
    {
        // unique id - matches the Qbicle Id
        public int Id { get; set; }

        /// <summary>
        /// We need to do this because entity framework can't hold a list of primitives.  We therefore have a psudo column TaskAsString which serialises _tasks to a csv string when
        /// the value is got (ie when the object is stored in SqlServer.  Reverse is true when data read from the db.  We hide the property from intellisense to 
        /// try and avoid anyone accessing it directly.  It is possible now to map private properties using entity framework, but it is a bit of a faff.
        /// </summary>
        private List<int> _tasks { get; set; } = new List<int>();
        public List<int> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] // hide from intelisense
        public string TaskAsString
        {
            get { return String.Join(",", _tasks); }
            set {
                _tasks.Clear();
                if (value != "")
                {
                    foreach (string s in value.Split(','))
                        _tasks.Add(int.Parse(s));
                }
            }
        }
       

        private List<int> _posts { get; set; } = new List<int>();
        public List<int> Posts
        {
            get { return _posts; }
            set { _posts = value; }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] // hide from intelisense
        public string PostAsString
        {
            get { return String.Join(",", _posts); }
            set
            {
                _posts.Clear();
                if (value != "")
                {
                    foreach (string s in value.Split(','))
                        _posts.Add(int.Parse(s));
                }
            }
        }
    

        private List<int> _meetings { get; set; } = new List<int>();
        public List<int> Meetings
        {
            get { return _meetings; }
            set { _meetings = value; }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] // hide from intelisense
        public string MeetingAsString
        {
            get { return String.Join(",", _meetings); }
            set
            {
                _meetings.Clear();
                if (value != "")
                {
                    foreach (string s in value.Split(','))
                        _meetings.Add(int.Parse(s));
                }
            }
        }


        /// <summary>
        /// populate from the passed Qbicle profile
        /// </summary>
        /// <param name=""></param>
        public void CopyFrom(QbicleProfile source)
        {
            this.Id = source.Id;
            this.TaskAsString = source.TaskAsString;
            this.PostAsString = source.PostAsString;
            this.MeetingAsString = source.MeetingAsString;
        }
    }
}
