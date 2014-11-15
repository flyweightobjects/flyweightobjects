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
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Creates new instances of business logic classes given a type parameter and business logic assembly.
	/// </summary>
	public sealed class BusinessLogicFactory
	{
		private static Dictionary<Type, Type> _cache = new Dictionary<Type, Type>();

		/// <summary>
		/// Returns an <see cref="IBusinessLogic"/> using the supplied default business logic type.
		/// </summary>
		/// <param name="defaultBusinessLogicType">The default business logic type to create.</param>
		public static IBusinessLogic GetBusinessLogic(Type defaultBusinessLogicType)
		{
			if (defaultBusinessLogicType == null)
			{
				throw new ArgumentNullException("defaultBusinessLogicType");
			}
			Type logicType = defaultBusinessLogicType;
			if (logicType.IsGenericTypeDefinition)
			{
				logicType = logicType.MakeGenericType(typeof(FlyweightBase));
			}
			IBusinessLogic logic = Activator.CreateInstance(logicType) as IBusinessLogic;
			logic.DefaultBusinessLogicType = defaultBusinessLogicType;
			return logic;
		}

		/// <summary>
		/// Returns an <see cref="IBusinessLogic"/> using the supplied default business logic type.
		/// </summary>
		/// <param name="context">The current calling context.</param>
		public static IBusinessLogic GetBusinessLogic(IBusinessLogic context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			Type logicType = context.DefaultBusinessLogicType;
			if (logicType.IsGenericTypeDefinition)
			{
				logicType = logicType.MakeGenericType(typeof(FlyweightBase));
			}
			IBusinessLogic logic = Activator.CreateInstance(logicType) as IBusinessLogic;
			if (logic.DomainName == context.DomainName)
			{
				logic.StorageProvider = context.StorageProvider;
				logic.DefaultBusinessLogicType = context.DefaultBusinessLogicType;
			}
			return logic;
		}
		
		/// <summary>
        /// Returns an appropriate business logic instance for the specified type of T.
        /// </summary>
        /// <typeparam name="T">The type of business logic to create.</typeparam>
        /// <param name="context">The current calling context.</param>
        public static IBusinessLogic<T> GetBusinessLogic<T>(IBusinessLogic context) where T : class, IFlyweight, new()
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
			Type paramType = typeof(T).GetGenericTypeParameter();
            Type logicType = null;
            if (!TryGetCachedLogicType(paramType, out logicType))
            {
                logicType = context.DefaultBusinessLogicType;
                foreach (Type assyType in logicType.Assembly.GetTypes())
                {
                    if (typeof(IBusinessLogic<T>).IsAssignableFrom(assyType))
                    {
                        if (!assyType.IsGenericType)
                        {
                            Type containedType = assyType.BaseType.GetGenericArguments()[0];
                            if (paramType == containedType)
                            {
                                logicType = assyType;
                                break;
                            }
                        }
                    }
                }
                SetCachedLogicType(paramType, logicType);
            }
            if (logicType.IsGenericTypeDefinition)
            {
                logicType = logicType.MakeGenericType(paramType);
            }
            IBusinessLogic<T> logic = Activator.CreateInstance(logicType) as IBusinessLogic<T>;
			if (logic.DomainName == context.DomainName)
			{
				logic.StorageProvider = context.StorageProvider;
				logic.DefaultBusinessLogicType = context.DefaultBusinessLogicType;
			}
            return logic;
        }

        /// <summary>
        /// Returns an appropriate business logic instance for the specified type of T.
        /// </summary>
        /// <typeparam name="T">The type of business logic to create.</typeparam>
        /// <param name="defaultBusinessLogicType">The default business logic type to create if one for T cannot be found.</param>
        public static IBusinessLogic<T> GetBusinessLogic<T>(Type defaultBusinessLogicType) where T : class, IFlyweight, new()
        {
            if (defaultBusinessLogicType == null)
            {
                throw new ArgumentNullException("defaultBusinessLogicType");
            }
            Type paramType = typeof(T).GetGenericTypeParameter();
            Type logicType = null;
            if (!TryGetCachedLogicType(paramType, out logicType))
            {
                logicType = defaultBusinessLogicType;
                foreach (Type assyType in logicType.Assembly.GetTypes())
                {
                    if (typeof(IBusinessLogic<T>).IsAssignableFrom(assyType))
                    {
                        if (!assyType.IsGenericType)
                        {
                            Type containedType = assyType.BaseType.GetGenericArguments()[0];
                            if (paramType == containedType)
                            {
                                logicType = assyType;
                                break;
                            }
                        }
                    }
                }
                SetCachedLogicType(paramType, logicType);
            }
            if (logicType.IsGenericTypeDefinition)
            {
                logicType = logicType.MakeGenericType(paramType);
            }
            IBusinessLogic<T> logic = Activator.CreateInstance(logicType) as IBusinessLogic<T>;
            logic.DefaultBusinessLogicType = defaultBusinessLogicType;
            return logic;
        }

        private static bool TryGetCachedLogicType(Type paramType, out Type logicType)
		{
			logicType = null;
			lock (_cache)
			{
				if (_cache.TryGetValue(paramType, out logicType))
				{
					return true;
				}
			}
			return false;
		}
		
		private static void SetCachedLogicType(Type paramType, Type logicType)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(paramType))
				{
					_cache.Add(paramType, logicType);
				}
			}
		}
	}
}
