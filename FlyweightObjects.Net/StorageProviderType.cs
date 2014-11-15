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

namespace FlyweightObjects
{
	/// <summary>
	/// Represents an enumerated value of supported <see cref="IStorageProvider"/>s.
	/// </summary>
	public enum StorageProviderType
	{
		/// <summary>
		/// Microsoft SQL Server database
		/// </summary>
		SqlServer,
        /// <summary>
        /// Microsoft SQL Server Compact Edition database
        /// </summary>
        SqlServerCe,
		/// <summary>
		/// Microsoft Jet database
		/// </summary>
		MsJet,
		/// <summary>
		/// MySQL database
		/// </summary>
		MySql,
		/// <summary>
		/// Oracle database
		/// </summary>
		Oracle
	}
}