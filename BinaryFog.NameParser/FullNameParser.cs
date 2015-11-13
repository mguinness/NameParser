﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BinaryFog.NameParser
{
    /// <summary>
    /// Parse a person full name 
    /// </summary>
    /// <example>
    /// 1. Mr Jack Johnson  => Title = "Mr", First Name = "Jack" Last Name = "Johnson"
    /// 2. Jack Johnson  => First Name = "Jack" Last Name = "Johnson"
    /// 3. Jack => First Name = "Jack"
    /// 4. Jack Johnson Enterprises => ignored
    /// 5. Pasquale (Pat) Vacoturo  =>  First Name = "Pasquale" Last Name = "Vacoturo" Nickname = Pat 
    /// 6. Mr Giovanni Van Der Hutte  => Title = "Mr", First Name = "Giovanni" Last Name = "Van Der Hutte"
    /// 7. Giovanni Van Der Hutte  => First Name = "Giovanni" Last Name = "Van Der Hutte"
    /// </example>
    /// <remarks>
    /// 1. The prefix "ATTN:" is removed if exists and the parsing proceeds on the new string
    /// </remarks>
    public class FullNameParser
    {
        Dictionary<Type, ParsedName> results;

        string fullName;

        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }
        public string Title { get; private set; }
        public string NickName { get; private set; }
        public string Suffix { get; private set; }
        public string DisplayName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullNameParser"/> class.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public FullNameParser(string fullName)
        {
            this.fullName = fullName;
            this.results = new Dictionary<Type, ParsedName>();

        }

        /// <summary>
        /// Parses this instance.
        /// </summary>
        public void Parse()
        {
            DisplayName = fullName;
            if (String.IsNullOrEmpty(fullName))
                return;

            RemoveATTNPrefixIfNeeded();

            var type = typeof(IPattern);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass);
            foreach (var patternType in types)
            {
                ParsedName result = ((IPattern)Activator.CreateInstance(patternType)).Parse(fullName);
                results.Add(patternType, result);
            }

            List<ParsedName> possibleAnswers = (from c in results.Values.AsEnumerable<ParsedName>()
                                                 where c != null
                                                 orderby c.Score descending
                                                 select c).ToList<ParsedName>();

            var v = (from c in possibleAnswers select c).FirstOrDefault<ParsedName>();

            FirstName = v != null ? v.FirstName :null;
            MiddleName = v != null ? v.MiddleName : null;
            LastName = v != null ? v.LastName:null;
            Title = v != null ? v.Title:null;
            NickName = v != null ? v.NickName:null;
            Suffix = v != null ? v.Suffix:null;
            DisplayName = v != null ? v.DisplayName : fullName;
               
            
        }

        /// <summary>
        /// Removes the attn prefix if needed.
        /// </summary>
        private void RemoveATTNPrefixIfNeeded()
        {
            if (fullName.StartsWith("ATTN:"))
            {
                fullName = fullName.Substring(5).Trim();
            }

        }


        
    }
}
