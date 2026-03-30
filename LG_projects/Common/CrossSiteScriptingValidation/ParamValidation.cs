using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace LG_projects.Common.CrossSiteScriptingValidation
{
    public static class CrossSiteScriptingValidation
    {
        private static readonly char[] StartingChars = { '<', '&' };

        #region Public methods
        public static class SqlReservedKeywords
        {
            public static bool IsExist(string param)
            {
                var data = false;
                foreach (var item in SqlServerKeywords)
                {
                    if (param.ToUpper().Contains(item))
                    {
                        return true;
                    }
                }
                return data;
            }

            private static HashSet<string> SqlServerKeywords = new HashSet<string>();

            static SqlReservedKeywords()
            {
                SqlServerKeywords.Add("ADD");
                SqlServerKeywords.Add("EXISTS");
                SqlServerKeywords.Add("PRECISION");
                SqlServerKeywords.Add("SELECT");
                SqlServerKeywords.Add("DELETE");
                SqlServerKeywords.Add("CREATE");
                SqlServerKeywords.Add("INSERT");
                SqlServerKeywords.Add("TRUNCATE");
                SqlServerKeywords.Add("UPDATE");
                SqlServerKeywords.Add("EXEC");
                SqlServerKeywords.Add("PIVOT");
                SqlServerKeywords.Add("WITH");
                SqlServerKeywords.Add("EXECUTE");
                SqlServerKeywords.Add("PLAN");
                SqlServerKeywords.Add("WRITETEXT");
                SqlServerKeywords.Add("CONCATE");
            }
        }
        private static bool ContainsSpecialChars(string value)
        {
            var list = new[] { "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "=", "\"", "<", ">", ",", ";", ":" };
            return list.Any(value.Contains);
        }
        public static bool RegexEmailCheck(string input)
        {
            // returns true if the input is a valid email
            return Regex.IsMatch(input, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
        public static bool IsDangerousString(string s, out int matchIndex)
        {

            matchIndex = 0;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            //bool inComment = false;

            bool stringIsValid = ContainsSpecialChars(s);
            if (stringIsValid)
            {
                return true;
            }

            for (var i = 0; ;)
            {

                // Look for the start of one of our patterns 
                var n = s.IndexOfAny(StartingChars, i);

                // If not found, the string is safe
                if (n < 0) return false;

                // If it's the last char, it's safe 
                if (n == s.Length - 1) return false;

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?')
                            return true;
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. S) 
                        if (s[n + 1] == '#')
                            return true;
                        break;

                }

                // Continue searching
                i = n + 1;
            }
        }
        public static bool IsDangerousParameter(string s, out int matchIndex)
        {

            matchIndex = 0;
            if (s == "null")
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            var validParam = SqlReservedKeywords.IsExist(s);
            s = s.Replace(":", "COL6");
            s = s.Replace(";", "COL7");
            s = s.Replace("&", "AND2");
            s = s.Replace("!", "S8N0O9");
            s = s.Replace("?", "why");
            s = s.Replace("/", "BCLS");
            if (validParam)
            {
                return validParam;
            }
            for (var i = 0; ;)
            {

                // Look for the start of one of our patterns 
                var n = s.IndexOfAny(StartingChars, i);

                // If not found, the string is safe
                if (n < 0) return false;

                // If it's the last char, it's safe 
                if (n == s.Length - 1) return false;

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?')
                            return true;
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. S) 
                        if (s[n + 1] == '#')
                            return true;
                        break;

                }

                // Continue searching
                i = n + 1;
            }
            return false;
        }

        #endregion

        #region Private methods

        private static bool IsAtoZ(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        #endregion

        public static void AddHeaders(this IHeaderDictionary headers)
        {
            if (headers["P3P"].IsNullOrEmpty())
            {
                headers.Add("P3P", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
        public static string ToJSON(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
