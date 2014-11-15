//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET                                                                //
//  Copyright:      Copyright © FlyweightObjects.NET 2011, All rights reserved.                         //
//  Date Created:   06/04/2008                                                                          //
//                                                                                                      //
//  Disclaimer:                                                                                         //
//  ===========                                                                                         //
//  This code file is provided "as is" with no expressed or implied warranty. The author accepts no     //
//  liability for any damage or loss that the code file may cause as a result of its use. Any           //
//  modification, copying, or reverse engineering of this code file, or the underlying architectural    //
//  foundation it supports, is strictly prohibited without the express written consent of the author.   //
//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Data.Common;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a <see cref="DatabaseManager"/> for Oracle specific storage operations.
    /// </summary>
    public class OracleDatabaseManager : DatabaseManager
    {
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="connectionString"></param>
        public OracleDatabaseManager(string connectionString)
            : base(connectionString, StorageProviderType.Oracle)
        {

        }

        /// <summary>
        /// Returns a new instance of an <see cref="IDbCommand"/> object.
        /// </summary>
        /// <param name="command">The <see cref="IStorageCommand"/> used to create the command.</param>
        protected override DbCommand CreateCommand(IStorageCommand command)
        {
            DbCommand cmd = base.CreateCommand(command);
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                if (command.Parameters[i].IsResultSet)
                {
                    ((OracleParameter)cmd.Parameters[i]).OracleType = OracleType.Cursor;
                }
            }
            return cmd;
        }
    }
}
