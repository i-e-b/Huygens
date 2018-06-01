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
    ///<summary>
    /// Event exposing a completed request
    ///</summary>
    public class RequestEventArgs : EventArgs
    {
        ///<summary>
        /// Create event
        ///</summary>
        public RequestEventArgs(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            RequestLog = requestLog;
            ResponseLog = responseLog;
            Id = id;
        }

        ///<summary>
        /// Unique ID of request
        ///</summary>
        public Guid Id { get; }

        ///<summary>
        /// Details of client request
        ///</summary>
        public LogInfo RequestLog { get; }

        ///<summary>
        /// Details of server response
        ///</summary>
        public LogInfo ResponseLog { get; }
    }
}