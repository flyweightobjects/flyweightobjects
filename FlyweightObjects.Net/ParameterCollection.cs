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
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a collection of <see cref="Parameter"/>.
	/// </summary>
	[Serializable()]
	[DebuggerDisplay("Count = {Count}")]
	public class ParameterCollection : CollectionBase 
	{
        /// <summary>
		/// Gets or sets the <see cref="Parameter"/> at the specified integer index.
		/// </summary>
		/// <param name="index">The zero-based index by which to retrieve the <see cref="Parameter"/>.</param>
		public Parameter this[int index] 
		{
			get { return (Parameter)this.List[index]; }
			set { this.List[index] = value; }
		}

        /// <summary>
		///  Gets or sets the <see cref="Parameter"/> at the specified string index.
		/// </summary>
		/// <param name="index">The index by which to retrieve the <see cref="Parameter"/>.</param>
		public Parameter this[string index] 
		{
			get 
			{
				foreach(Parameter parameter in base.List) 
				{
                    if (parameter.Name == index)
                    {
                        return parameter;
                    }
				}
				return null;
			}
			set 
			{
				for(int i = 0; i < this.List.Count; i++) 
				{
                    Parameter parameter = (Parameter)this.List[i];
                    if (parameter.Name == index)
					{
						this.List[i] = value;
					}
				}
			}
		}
		
		/// <summary>
		/// Adds the specified <see cref="Parameter"/> to the collection.
		/// </summary>
		/// <param name="parameter">An instance of <see cref="Parameter"/>.</param>
		public int Add(Parameter parameter) 
		{
			int iRetVal = List.IndexOf(parameter);
			if(iRetVal == -1) 
			{
				iRetVal = List.Add(parameter);
			}
			else 
			{
				((Parameter)List[iRetVal]).Value = parameter.Value;
			}
			return iRetVal;
		}

        /// <summary>
        /// Adds the array of <see cref="Parameter"/> to the collection.
        /// </summary>
        /// <param name="values">An array of <see cref="Parameter"/>.</param>
        public void AddRange(Parameter[] values)
        {
            foreach (Parameter parameter in values)
            {
                this.Add(parameter);
            }
        }

		/// <summary>
		/// Adds the array of <see cref="Parameter"/> to the collection.
		/// </summary>
		/// <param name="collection">A <see cref="ParameterCollection"/> of <see cref="Parameter"/>.</param>
		public void AddRange(ParameterCollection collection) 
		{
            foreach (Parameter parameter in collection)
            {
                this.Add(parameter);
            }
		}

        /// <summary>
		/// Determines whether the colelction contains the specified <see cref="Parameter"/>.
		/// </summary>
		/// <param name="value">An instance of <see cref="Parameter"/>.</param>
		public bool Contains(Parameter value) 
		{
			return List.Contains(value);
		}

		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the beginning of the target array.
		/// </summary>
		/// <param name="array">The array to be copied to.</param>
		/// <param name="index">The starting index.</param>
		public void CopyTo(Parameter[] array, int index)
		{
			List.CopyTo(array, index);
		}

        /// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire collection.
		/// </summary>
		/// <param name="value">The <see cref="Parameter"/> to search.</param>
		public int IndexOf(Parameter value) 
		{
			return List.IndexOf(value);
		}
		
		/// <summary>
		/// Inserts an element into the collection at the specified index.
		/// </summary>
		/// <param name="index">The index to insert.</param>
		/// <param name="value">The <see cref="Parameter"/> to insert.</param>
		public void Insert(int index, Parameter value) 
		{
			List.Insert(index, value);
		}
		
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		public new ParameterEnumerator GetEnumerator() 
		{
			return new ParameterEnumerator(this);
		}
		
		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="value">The <see cref="Parameter"/> to remove.</param>
		public void Remove(Parameter value) 
		{
			List.Remove(value);
		}
		
		/// <summary>
		/// Sorts the collection.
		/// </summary>
		public void Sort() 
		{
			this.InnerList.Sort();
		}

		/// <summary>
		/// Sorts the collection give the comparer.
		/// </summary>
		/// <param name="comparer"></param>
		public void Sort(IComparer comparer) 
		{
			this.InnerList.Sort(comparer);
		}
		
		/// <summary>
		/// Represents an enumerator for a <see cref="ParameterCollection"/>.
		/// </summary>
		public class ParameterEnumerator : object, IEnumerator 
		{
			private IEnumerator baseEnumerator;
			private IEnumerable temp;
			
			/// <summary>
			/// Constructs a new instance of the class.
			/// </summary>
			/// <param name="mappings">An instance of a <see cref="ParameterCollection"/></param>
			public ParameterEnumerator(ParameterCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
			
			/// <summary>
			/// Returns the current <see cref="Parameter"/>.
			/// </summary>
			public Parameter Current 
			{
				get { return ((Parameter)(baseEnumerator.Current)); }
			}
			
			/// <summary>
			/// Returns the base enumerator's current object. 
			/// </summary>
			object IEnumerator.Current 
			{
				get { return baseEnumerator.Current; }
			}
			
			/// <summary>
			/// Moves the enumerator to the next object.
			/// </summary>
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			/// <summary>
			/// Moves the enumerator to the next object.
			/// </summary>
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			/// <summary>
			/// Resets the base enumerator.
			/// </summary>
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
			
			/// <summary>
			/// Resets the base enumerator.
			/// </summary>
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	}
}
