using System;
using System.Collections.Generic;
using Atlassian.Jira;
using System.Diagnostics;
using JiraConnector.Common;
using System.Reflection;

namespace JiraConnector
{
    public class JiraConnection
    {
        private static JiraConnection mInstance = null;
        private Jira mJira = null;
        private bool mIsLogin = false;
        List<IssueInfo> mIssueList = new List<IssueInfo>();

        private JiraConnection()
        {
            Trace.WriteLine("[JiraConnection]+");
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                String resourceName = "JiraConnector." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Trace.WriteLine("JiraConnector.resourceName = " + resourceName);
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            mIssueList.Clear();
        }

        public static JiraConnection getInstance()
        {
            Trace.WriteLine("[JiraConnection.getInstance]+");
            if (mInstance == null)
            {
                mInstance = new JiraConnection();
            }
            return mInstance;
        }

        public bool isLogin()
        {
            return mIsLogin;
        }

        public bool login(string url, string username, string password)
        {
            try
            {
                mJira = Jira.CreateRestClient(url, username, password);
                Issue issue = mJira.GetIssue("JGR-11348");//login check
                Trace.WriteLine(String.Format("issue = {0}" ,issue.Key));
                mIsLogin = true;
            }
            catch (Exception ex)
            {
                mJira = null;
                Trace.WriteLine(String.Format("[login] Login failed({0}). ex = {1}", url, ex.GetBaseException()));
                throw new JiraException(JiraException.LOGIN_FAILURE);
            }
            return mIsLogin;
        }

        public bool signOut()
        {
            mJira = null;
            mIsLogin = false;
            Trace.WriteLine("Sign out success.");
            return true;
        }

        public List<IssueInfo> QueryIssue(string condition, int count=500)
        {
            if (!isLogin())
            {
                throw new JiraException(JiraException.NOT_LOGIN_YET);
            }
            else if(condition == String.Empty)
            {
                throw new JiraException(JiraException.EMPTY_QUERY_CONDITION);
            }

            IEnumerable<Issue> issues = null;
            try
            {
                issues = mJira.GetIssuesFromJql(condition, count);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[QueryIssue] ex = " + ex.GetBaseException());
                throw new JiraException(JiraException.QUERY_FAILURE);
            }
            return getIssueList(issues);
        }

        private List<IssueInfo> getIssueList(IEnumerable<Issue> issues)
        {
            mIssueList.Clear();
            if (issues == null) return mIssueList;
            foreach (Issue issue in issues)
            {
                mIssueList.Add(new IssueInfo(issue));
            }
            Trace.WriteLine("[getIssueList] mIssueList size ="+ mIssueList.Count);
            return mIssueList;
        }
    }
}
