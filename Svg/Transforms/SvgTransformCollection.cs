using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Interfaces;
using ICloneable = Svg.Interfaces.ICloneable;

namespace Svg.Transforms
{
    //[TypeConverter(typeof(SvgTransformConverter))]
    public class SvgTransformCollection : List<SvgTransform>, ICloneable
    {
    	private void AddItem(SvgTransform item)
    	{
    		base.Add(item);
    	}
    	
    	public new void Add(SvgTransform item)
	    {
	        var o = this.Clone();
    		AddItem(item);
            OnTransformChanged((SvgTransformCollection)o);
        }
    	
    	public new void AddRange(IEnumerable<SvgTransform> collection)
        {
            var o = this.Clone();
            base.AddRange(collection);
            OnTransformChanged((SvgTransformCollection)o);
        }
    	
    	public new void Remove(SvgTransform item)
        {
            var o = this.Clone();
            base.Remove(item);
            OnTransformChanged((SvgTransformCollection)o);
        }
    	
    	public new void RemoveAt(int index)
        {
            var o = this.Clone();
            base.RemoveAt(index);
            OnTransformChanged((SvgTransformCollection)o);
        }
    	
    	/// <summary>
    	/// Multiplies all matrices
    	/// </summary>
    	/// <returns>The result of all transforms</returns>
    	public Matrix GetMatrix()
        {
            var transformMatrix = SvgEngine.Factory.CreateMatrix();
    		
    		// Return if there are no transforms
            if (this.Count == 0)
            {
            	return transformMatrix;
            }

            foreach (SvgTransform transformation in this)
            {
                transformMatrix.Multiply(transformation.Matrix);
            }

            return transformMatrix;
    	}

		public override bool Equals(object obj)
		{
			if (this.Count == 0 && this.Count == base.Count) //default will be an empty list 
				return true;
			return base.Equals(obj);
		}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public new SvgTransform this[int i]
        {
			get { return base[i]; }
			set
			{
			    var o = this.Clone();
				var oldVal = base[i];
				base[i] = value;
				if(oldVal != value)
                    OnTransformChanged((SvgTransformCollection)o);
			}
		}
		
		/// <summary>
        /// Fired when an SvgTransform has changed
        /// </summary>
        public event EventHandler<AttributeEventArgs> TransformChanged;
        
        protected void OnTransformChanged(SvgTransformCollection oldValue)
        {
            //make a copy of the current value to avoid collection changed exceptions
            TransformChanged?.Invoke(this, new AttributeEventArgs("transform", this.Clone(), oldValue));
        }	
    	
		public object Clone()
		{
			var result = new SvgTransformCollection();
			foreach (var trans in this) 
			{
				result.AddItem(trans.Clone() as SvgTransform);
			}
			return result;
		}

        public override string ToString()
        {
            if (this.Count < 1) return string.Empty;
            return (from t in this select t.ToString()).Aggregate((p,c) => p + " " + c);
        }
    }
}
