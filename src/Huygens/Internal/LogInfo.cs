//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

using System;

namespace Huygens.Internal
{
    /// <summary>
    /// Server log line
    /// </summary>
    [Serializable]
    public class LogInfo : ICloneable
    {
        ///<summary>
        /// Body
        ///</summary>
        public byte[] Body { get; set; }

        ///<summary>
        /// Unique id
        ///</summary>
        public Guid ConversationId { get; set; }

        ///<summary>
        /// Log time
        ///</summary>
        public DateTime Created { get; set; }

        ///<summary>
        /// Error, if any
        ///</summary>
        public string Exception { get; set; }

        ///<summary>
        /// Headers
        ///</summary>
        public string Headers { get; set; }

        ///<summary>
        /// Identity
        ///</summary>
        public string Identity { get; set; }

        ///<summary>
        /// Virtual path
        ///</summary>
        public string PathTranslated { get; set; }

        ///<summary>
        /// Site file path
        ///</summary>
        public string PhysicalPath { get; set; }

        ///<summary>
        /// unique id
        ///</summary>
        public long RowId { get; set; }

        ///<summary>
        /// row type
        ///</summary>
        public long RowType { get; set; }

        ///<summary>
        /// status code, if a response
        ///</summary>
        public long? StatusCode { get; set; }

        ///<summary>
        /// url requested
        ///</summary>
        public string Url { get; set; }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        ///<summary>
        /// Clone this log entry
        ///</summary>
        public LogInfo Clone()
        {
            var result = (LogInfo) ((ICloneable) this).Clone();
            if (Body != null)
            {
                result.Body = new byte[Body.Length];
                Body.CopyTo(result.Body, 0);
            }

            return result;
        }
    }
}