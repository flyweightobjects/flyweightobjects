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
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace FlyweightObjects
{
	internal class ThreadLocalStorage
	{
        private const string MAX_DEBUG_LIST_SIZE = "MaxDebugListSize";
        private static readonly object _syncRoot = new object();

        public static void RegisterContext(IDataContext context)
		{
			string key = BuildContextKey(context);
			if (!IsContextRegistered(key))
			{
				if (Thread.GetNamedDataSlot(key) == null)
				{
					Thread.AllocateNamedDataSlot(key);
				}
				LocalDataStoreSlot slot = Thread.GetNamedDataSlot(key);
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Registering context '{0}' with thread local storage", key));
				Thread.SetData(slot, context);
			}
		}

        public static void UnRegisterContext(IDataContext context)
		{
			string key = BuildContextKey(context);
			if (IsContextRegistered(key))
			{
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Unregistering context '{0}' with thread local storage", key));
				Thread.FreeNamedDataSlot(key);
			}
		}

        public static void UnRegisterContext(string key)
		{
			Thread.FreeNamedDataSlot(key);
		}

        public static bool IsContextRegistered(IDataContext context)
		{
			string key = BuildContextKey(context);
			return IsContextRegistered(key);
		}

        public static bool IsContextRegistered(string key)
		{
			LocalDataStoreSlot slot = Thread.GetNamedDataSlot(key);
			if (slot != null)
			{
				return Thread.GetData(slot) as IDataContext != null;
			}
			return false;
		}

        public static IDataContext GetRegisteredContext(string key)
		{
			LocalDataStoreSlot slot = Thread.GetNamedDataSlot(key);
			if (slot != null)
			{
				return Thread.GetData(slot) as IDataContext;
			}
			return null;
		}

        public static string BuildContextKey(IDataContext context)
		{
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return BuildKeyInternal(context.DomainName);
		}

        public static string BuildContextKey(Type type)
		{
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            DataTableAttribute attr = DataAttributeUtilities.GetDataTableAttribute(type);
            return BuildKeyInternal(attr.DomainName);
		}

        public static void SetMaxDebugListSize(int size)
        {
            lock (_syncRoot)
            {
                if (Thread.GetNamedDataSlot(MAX_DEBUG_LIST_SIZE) == null)
                {
                    Thread.AllocateNamedDataSlot(MAX_DEBUG_LIST_SIZE);
                }
                LocalDataStoreSlot slot = Thread.GetNamedDataSlot(MAX_DEBUG_LIST_SIZE);
                Thread.SetData(slot, size);
            }
        }

        public static int GetMaxDebugListSize()
        {
            int size = 100;
            lock (_syncRoot)
            {
                LocalDataStoreSlot slot = Thread.GetNamedDataSlot(MAX_DEBUG_LIST_SIZE);
                if (slot != null)
                {
                    object value = Thread.GetData(slot);
                    if (value != null && value is Int32)
                    {
                        size = (int)Thread.GetData(slot);
                    }
                }
            }
            return size;
        }

        private static string BuildKeyInternal(string domainName)
        {
            string key = string.Empty;
            if (!string.IsNullOrEmpty(domainName))
            {
                key = string.Format("{0}.{1}.{2}", typeof(DataContext).Name, domainName, Thread.CurrentThread.ManagedThreadId);
            }
            else
            {
                key = string.Format("{0}.{1}", typeof(DataContext).Name, Thread.CurrentThread.ManagedThreadId);
            }
            return key;
        }
	}
}
