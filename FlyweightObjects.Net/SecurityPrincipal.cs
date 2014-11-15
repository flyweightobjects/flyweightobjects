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
    /// Represents information about the current user who has made a request to the server.
    /// </summary>
    [Serializable]
    public class SecurityPrincipal : MarshalByRefObject, ISecurityPrincipal
    {
        private GenericPrincipal _principal;
        private string _machineName;
        private Version _runtimeVersion;
        private HybridDictionary _properties;

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        public SecurityPrincipal(IIdentity identity, string[] roles)
        {
            _principal = new GenericPrincipal(identity, roles);
            _machineName = Environment.MachineName;
            _runtimeVersion = Environment.Version;
            _properties = new HybridDictionary();
        }
        
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        public SecurityPrincipal()
            : this(new GenericIdentity(WindowsIdentity.GetCurrent().Name, WindowsIdentity.GetCurrent().AuthenticationType), new string[0])
        {
            
        }

        /// <summary>
        /// Gets the local host name of the client's machine.
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
        }

        /// <summary>
        /// Gets the current version of the .NET runtime.
        /// </summary>
        public Version RuntimeVersion
        {
            get { return _runtimeVersion; }
        }

        /// <summary>
        /// Gets a serializable dictionary of dynamic properties.
        /// </summary>
        public HybridDictionary Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the <see cref="IIdentity"/> of the user represented by the current principal.
        /// </summary>
        public IIdentity Identity
        {
            get { return _principal.Identity; }
        }

        /// <summary>
        /// Determines whether the current GenericPrincipal belongs to the specified role.
        /// </summary>
        /// <param name="role">The name of the role for which to check membership.</param>
        public bool IsInRole(string role)
        {
            return _principal.IsInRole(role);
        }
    }
}
