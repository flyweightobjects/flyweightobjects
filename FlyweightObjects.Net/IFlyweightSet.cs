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
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents an interface for a lightweight set of objects optimized for high speed access.
    /// </summary>
    /// <typeparam name="T">The type parameter of the domain type.</typeparam>
    internal interface IFlyweightSet<T> : IList<T>, ICollection<T>, IEnumerable<T>, IResponseMessage where T : class, IFlyweight, new()
    {
        ResultSet DataSource { get; }
        T Flyweight { get; }
        T InstanceAt(int index);
        IList<T> DebugList { get; }
    }
}
