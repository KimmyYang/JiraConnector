using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraConnector.Common
{
    public class JiraException : Exception
    {
        public const int GENERIC_FAILURE = -1;
        public const int LOGIN_FAILURE = -2;
        public const int NOT_LOGIN_YET = -3;
        public const int QUERY_FAILURE = -4;
        public const int EMPTY_QUERY_CONDITION = -5;
        public const int ASSIGN_ISSUE_FAILED = -6;
        public const int DOWNLOAD_ATTACH_FAILED = -7;
        private static Pair<int, string>[] EXCEPTION_ARRAY = { new Pair<int, string>(GENERIC_FAILURE, "Generic Failure."),
                                                               new Pair<int, string>(LOGIN_FAILURE, "Login Failed"),
                                                               new Pair<int, string>(NOT_LOGIN_YET, "Not login yet."),
                                                               new Pair<int, string>(QUERY_FAILURE, "Query Failed."),
                                                               new Pair<int, string>(EMPTY_QUERY_CONDITION, "No any filter condition."),
                                                               new Pair<int, string>(ASSIGN_ISSUE_FAILED, "Failed to assgin issue."),
                                                               new Pair<int, string>(DOWNLOAD_ATTACH_FAILED, "Failed to download attachment.")
                                                               };

        private int mCode = 0;

        public JiraException(int code): base(message(code))
        {
            mCode = code;
        }

        public static string message(int code)
        {
            for(int i=0; i< EXCEPTION_ARRAY.Length; ++i)
            {
                if(EXCEPTION_ARRAY[i].first == code)
                {
                    return EXCEPTION_ARRAY[i].second;
                }
            }
            return EXCEPTION_ARRAY[0].second;
        }
    }
}
