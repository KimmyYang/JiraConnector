using Atlassian.Jira;
using System;
using System.Collections.Generic;
using JiraConnector.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.IO;

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
 
        private Queue mAttachQueue = null;
        private List<Thread> mThreadPool = new List<Thread>();
        const int THREAD_COUNT = 2;//web client just can support 2 thread only
        //action
        const int ACTION_DOWNLOAD = 1;
        const int ACTION_ASSIGN = 2;

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

        public void DownloadAattchments(string destPath)
        {
            Trace.WriteLine("[DownloadAattchments]+");
            try
            {
                IEnumerable<Attachment> attachList = issue.GetAttachments();
                Dictionary<string, Attachment> attachDictionary = new Dictionary<string, Attachment>();

                foreach (Attachment attach in attachList)
                {
                    if (!attachDictionary.ContainsKey(attach.FileName))
                    {
                        attachDictionary.Add(attach.FileName, attach);
                    }
                    else
                    {//handle the same attach file name
                        Attachment _attach = attachDictionary[attach.FileName];
                        DateTime _time = (DateTime)_attach.CreatedDate;
                        DateTime time = (DateTime)attach.CreatedDate;
                        Trace.WriteLine(String.Format("Same attache name : {0} , _time = {1} , time = {2}", attach.FileName, _time, time));
                        if (DateTime.Compare(_time, time) < 0)
                        {//get the newest
                            attachDictionary[attach.FileName] = attach;
                        }
                    } 
                }
                mAttachQueue = new Queue(attachDictionary.Values);//assign value to queue

                if (mAttachQueue.Count > 0)
                {
                    initThreadPool(ACTION_DOWNLOAD, destPath);//init thread
                    Start();//start download
                    Join();//wait
                }
                attachDictionary.Clear();
                Trace.WriteLine("[DownloadAattchments] Done");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ex = " + ex.GetBaseException());
                throw new JiraException(JiraException.DOWNLOAD_ATTACH_FAILED);
            }
        }

        private void download(object destPath)
        {
            Attachment attach = null;
            string target = String.Empty;
            int threadId = Thread.CurrentThread.ManagedThreadId;

            while (mAttachQueue.Count > 0)
            {
                lock (this)
                {
                    Trace.WriteLine(threadId + " : mAttachQueue.Count = " + mAttachQueue.Count);
                    if (mAttachQueue.Count > 0) attach = (Attachment)mAttachQueue.Dequeue();
                    else attach = null;
                }
                if (attach != null)
                {
                    Trace.WriteLine(threadId + " : download : " + attach.FileName);
                    target = destPath + "\\" + attach.FileName;
                    try {
                        //if(!File.Exists(target)) 
                        attach.Download(target);
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine("ex = " + ex.GetBaseException());

                    }
                }
            }
        }

        /*
         * Using for specfic task
         * ex. download, assign ..
        */
        private void initThreadPool(int action, string data)
        {
            if (mThreadPool.Count > 0) return;//need add release thread method
            for (int i = 0; i < THREAD_COUNT; ++i)
            {
                ThreadStart starter = delegate { Task(action, data); };
                mThreadPool.Add(new Thread(starter));
            }
        }

        private void Task(int action, string data)
        {
            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + " : start action "+ action);
            switch (action)
            {
                case ACTION_DOWNLOAD:
                    download(data);
                    break;
                case ACTION_ASSIGN:
                    assign(data);
                    break;
            }
            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + " : done action " + action);
        }

        private void Start()
        {
            foreach (Thread thread in mThreadPool)
            {
                thread.Start();
            }
        }

        private void Join()
        {
            foreach (Thread thread in mThreadPool)
            {
                thread.Join();
            }
        }
    }
}
