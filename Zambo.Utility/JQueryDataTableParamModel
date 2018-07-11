using System;

namespace Value.Site.Models
{
    public class JQueryDataTableParamModel
    {
        public columns[] columns { get; set; }
        public filters filters { get; set; }
        public int draw { get; set; }
        public int length { get; set; }
        public int start { get; set; }
        public order[] order { get; set; }
        public search search { get; set; }
    }

    public class order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class search
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }

    public enum GoalCompletionStatus
    {
        OnTrack,
        Behind,
        Overdue,
        Complete
    }

    public class filters
    {
        public string AppraiserUser { get; set; }
        public string CompanyId { get; set; }
        public string GetAssetTypes { get; set; }
        public string MainStatus { get; set; }
        public Nullable<GoalCompletionStatus> GoalCompletionStatus { get; set; }
        public string MaxApprduedate { get; set; }
        public string MaxRequestcreatedate { get; set; }
        public string MaxRequestduedate { get; set; }
        public string MethodId { get; set; }
        public string MinApprduedate { get; set; }
        public string MinRequestcreatedate { get; set; }
        public string MinRequestduedate { get; set; }
        public string reference { get; set; }
        public string ReferenceAdv { get; set; }
        public string request_id { get; set; }
        public string requestduedate { get; set; }
        public string appraiserduedate { get; set; }
        public string countyid { get; set; }
        public string[] CountyIdList { get; set; }
        public string StatusId { get; set; }
        public string TechnicianUser { get; set; }
        public string[] AppraiserUserList { get; set; }
        public string[] AssetSubTypesList { get; set; }
        public string[] CompanyIdList { get; set; }
        public string[] GetAssetTypesList { get; set; }
        public string[] MethodIdList { get; set; }
        public string[] StatusIdList { get; set; }
        public string[] TechnicianUserList { get; set; }
        public string[] RequestersList { get; set; }
        public string[] TeamsList { get; set; }
    }


    public class columns
    {
        public bool orderable { get; set; }
        public bool searchable { get; set; }
        public search search { get; set; }
        public string data { get; set; }
        public string name { get; set; }
    }
}
