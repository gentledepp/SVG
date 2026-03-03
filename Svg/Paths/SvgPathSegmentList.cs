using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Svg.Interfaces;
using ICloneable = Svg.Interfaces.ICloneable;

namespace Svg.Pathing
{
    //[TypeConverter(typeof(SvgPathBuilder))]
    public sealed class SvgPathSegmentList : IList<SvgPathSegment>, ICloneable
    {
        internal SvgPath _owner;
        private List<SvgPathSegment> _segments;

        public SvgPathSegmentList()
        {
            this._segments = new List<SvgPathSegment>();
        }

        public SvgPathSegmentList(IEnumerable<SvgPathSegment> segments)
        {
            this._segments = segments.ToList();
        }

        public SvgPathSegment Last
        {
            get { return this._segments[this._segments.Count-1]; }
        }

        public int IndexOf(SvgPathSegment item)
        {
            return this._segments.IndexOf(item);
        }

        public void Insert(int index, SvgPathSegment item)
        {
            // because cloning is expensive, we only do so if there is an _owner that may be interested
            if (this._owner != null)
            {
                var o = (SvgPathSegmentList)this.Clone();
                this._segments.Insert(index, item);
                this._owner.OnPathUpdated(o);
            }
            else
                this._segments.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            // because cloning is expensive, we only do so if there is an _owner that may be interested
            if (this._owner != null)
            {
                var o = (SvgPathSegmentList)this.Clone();
                this._segments.RemoveAt(index);
                this._owner.OnPathUpdated(o);
            }
            else
                this._segments.RemoveAt(index);
        }

        public SvgPathSegment this[int index]
        {
            get { return this._segments[index]; }
            set
            {
                // because cloning is expensive, we only do so if there is an _owner that may be interested
                if (_owner != null)
                {
                    var o = (SvgPathSegmentList)this.Clone();
                    this._segments[index] = value;
                    this._owner.OnPathUpdated(o);
                }
                else
                    this._segments[index] = value;
            }
        }

        public void Add(SvgPathSegment item)
        {
            // because cloning is expensive, we only do so if there is an _owner that may be interested
            if (this._owner != null)
            {
                var o = (SvgPathSegmentList)this.Clone();
                this._segments.Add(item);
                this._owner.OnPathUpdated(o);
            }
            else
                this._segments.Add(item);
        }

        /// <summary>
        /// Adds a segment without cloning the list or notifying the owner.
        /// Use during high-frequency operations (e.g. free drawing) to avoid O(N²) clone overhead.
        /// Caller is responsible for calling <see cref="SvgPath.MarkPathDirty"/> afterwards.
        /// </summary>
        public void AddWithoutNotify(SvgPathSegment item)
        {
            this._segments.Add(item);
        }

        public void Clear()
        {
            this._segments.Clear();
        }

        public bool Contains(SvgPathSegment item)
        {
            return this._segments.Contains(item);
        }

        public void CopyTo(SvgPathSegment[] array, int arrayIndex)
        {
            this._segments.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this._segments.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(SvgPathSegment item)
        {
            // because cloning is expensive, we only do so if there is an _owner that may be interested
            if (this._owner != null)
            {
                var o = (SvgPathSegmentList)this.Clone();
                bool removed = this._segments.Remove(item);

                if (removed)
                    this._owner.OnPathUpdated(o);

                return removed;
            }
            
            return this._segments.Remove(item);
        }

        public IEnumerator<SvgPathSegment> GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        public object Clone()
        {
            return new SvgPathSegmentList(this._segments.Select(s => s.Clone()));
        }
    }
}