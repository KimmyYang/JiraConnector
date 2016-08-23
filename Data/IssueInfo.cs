using Atlassian.Jira;
using System;
using System.Collections.Generic;
using JiraConnector.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JiraConnector
{
    public class IssueInfo
    {
        private Issue issue;
        public string key { get; }
        public string assigenee { set; get; }
        public string project { set; get; }
        public string summary { get; }
        public string description { get; }
        public string[] labels { get; }
        public List<Attachment> mAttachList = new List<Attachment>();

        public IssueInfo(Issue issue)
        {
            Trace.WriteLine("[IssueInfo] Created issue = " + issue.Key);
            this.issue = issue;
            this.key = issue.Key.Value;
            this.assigenee = issue.Assignee;
            this.project = issue.Project;
            this.summary = issue.Summary;
            this.description = issue.Description;
        }

        public void DownloadAattchments(string destPath)
        {
            Trace.WriteLine("[DownloadAattchments]+");
            try {
                mAttachList.Clear();//init first
                IEnumerable<Attachment> attachList = issue.GetAttachments();
                foreach (Attachment attach in attachList)
                {
                    Trace.WriteLine("[DownloadAattchments] file = "+ attach.FileName);
                    string target = destPath + "\\" + attach.FileName;
                    attach.Download(target);
                    mAttachList.Add(attach);
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("ex = " + ex.GetBaseException());
                throw new JiraException(JiraException.DOWNLOAD_ATTACH_FAILED);
            }

        }

        public void assign(string assignee)
        {
            if (assignee == issue.Assignee)
            {
                Trace.WriteLine(String.Format("assigenee = {0}, issue.Assignee = {1}", assignee, issue.Assignee));
                //throw new JiraException(JiraException.ASSIGN_ISSUE_FAILED);
                return;
            }
            try
            {
                issue.Assign(assignee);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[assign] ex = " + ex.GetBaseException());
                throw new JiraException(JiraException.ASSIGN_ISSUE_FAILED);
            }
        }
    }
}
