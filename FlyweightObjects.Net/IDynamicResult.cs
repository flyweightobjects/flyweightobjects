//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET, LLC                                                           //
//  Copyright:      Copyright © FlyweightObjects.NET 2009, All rights reserved.                         //
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
using System.Text;

namespace FlyweightObjects
{
    /// <summary>
    /// Appends a property bag of key value pair data to an object when result sets contain 
    /// more output columns than those exposed by the object type.
    /// </summary>
    public interface IDynamicResult
    {
        /// <summary>
        /// Gets or sets the <see cref="DynamicFieldCollection"/> representing fields not exposed as properties on the requested object.
        /// </summary>
        DynamicFieldCollection Fields { get; set; }
    }
}