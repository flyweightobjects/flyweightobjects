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
using System.Security.Principal;
using System.Collections.Specialized;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents information about the current principal who has made a request to the server.
    /// </summary>
    public interface ISecurityPrincipal : IPrincipal
    {
        /// <summary>
        /// Gets the local host name of the client's machine.
        /// </summary>
        string MachineName { get; }

        /// <summary>
        /// Gets the current version of the .NET runtime.
        /// </summary>
        Version RuntimeVersion { get; }

        /// <summary>
        /// Gets a serializable dictionary of dynamic properties.
        /// </summary>
        HybridDictionary Properties { get; }
    }
}
