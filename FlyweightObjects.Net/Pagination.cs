using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents pagination details for a <see cref="IQueryExpression{T}"/>.
    /// </summary>
    [Serializable]
    public class Pagination
    {
        /// <summary>
        /// Gets or sets the starting minimum value for the paging.
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum ending value for the paging.
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="List{PropertyExpression}"/> for the paging.
        /// </summary>
        public List<PropertyExpression> SortingExpressions { get; set; }

        /// <summary>
        /// Gets the alias for the row number field.
        /// </summary>
        protected internal string Alias { get; private set; }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="minValue">The starting minimum value for the paging.</param>
        /// <param name="maxValue">The maximum ending value for the paging.</param>
        public Pagination(int minValue, int maxValue)
        {
            this.SortingExpressions = new List<PropertyExpression>();
            this.Alias = string.Format("R{0}", this.GetHashCode());
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="minValue">The starting minimum value for the paging.</param>
        /// <param name="maxValue">The maximum ending value for the paging.</param>
        /// <param name="sortingExpressions">The sorting expression for the row number field.</param>
        public Pagination(int minValue, int maxValue, params PropertyExpression[] sortingExpressions)
            : this(minValue, maxValue)
        {
            if (sortingExpressions != null)
            {
                this.SortingExpressions.AddRange(sortingExpressions);
            }
        }
    }
}

