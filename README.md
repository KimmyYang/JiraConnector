The JIRA Adapter library
=======================================

This dll is coded by C#, and help to connected to open source libaray JIRA(Atlassian.Jira.dll).

-----------

API inculded:
* login(string url, string username, string password)
* signOut()
* QueryIssue(string condition, int count=500)
* assign(string assignee)
* DownloadAattchments(string destPath)
