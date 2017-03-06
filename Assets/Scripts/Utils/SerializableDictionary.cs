using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DGP.Util.Collections{
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>{
		#region constants
		private const int DefaultCapacity = 4;
		#endregion

		#region public instance constructors
		public SerializableDictionary() : this(-1, null){}
		public SerializableDictionary(int capacity) : this(capacity, null){}
		public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : this(-1, dictionary){}
		#endregion

		#region protected instance constructors
		protected SerializableDictionary(int capacity, IDictionary<TKey, TValue> dictionary){
			if(!SerializableDictionary<TKey, TValue>.CanBeSerialized(typeof(TKey))){
				throw new InvalidOperationException("TKey can't be serialized.");
			}

			if(!SerializableDictionary<TKey, TValue>.CanBeSerialized(typeof(TValue))){
				throw new InvalidOperationException("TValue can't be serialized.");
			}

			if (dictionary != null){
				capacity = dictionary.Count;
			}else if (capacity < 0){
				capacity = DefaultCapacity;
			}

			this._keys = new List<TKey>(capacity);
			this._values = new List<TValue>(capacity);

			if (dictionary != null){
				foreach (KeyValuePair<TKey, TValue> item in dictionary){
					this.Add(item.Key, item.Value);
				}
			}
		}
		#endregion

		#region protected instance properties
		[SerializeField]
		protected List<TKey> _keys;

		[SerializeField]
		protected List<TValue> _values;
		#endregion

		#region IEnumerable interface implementation: methods
		IEnumerator IEnumerable.GetEnumerator(){
			int count = this.Count;
			
			for (int i = 0; i < count; ++i){
				// Check if the dictionary has been modified since the enumerator was created
				if (count != this.Count){
					throw new InvalidOperationException("Collection was modified.");
				}
				
				yield return new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}
		#endregion

		#region ICollection<KeyValuePair<TKey, TValue>> interface implementation: properties
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly{
			get{return false;}
		}
		#endregion

		#region ICollection<KeyValuePair<TKey, TValue>> interface implementation: methods
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item){
			this.Add(item.Key, item.Value);
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item){
			TValue value;
			
			return	item.Key != null && 
					this.TryGetValue(item.Key, out value) &&
					(value == null && item.Value == null || value != null && value.Equals(item.Value));
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex){
			if (array == null){
				throw new ArgumentNullException("array");
			}else if (arrayIndex < 0){
				throw new ArgumentOutOfRangeException("arrayIndex");
			}else if (this.Count > array.Length - arrayIndex){
				throw new ArgumentException();
			}

			for (int i = 0; i < this.Count; ++i){
				array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item){
			if (item.Key != null){
				int index = this._keys.BinarySearch(item.Key);
				if (index >= 0){
					this._keys.RemoveAt(index);
					this._values.RemoveAt(index);
					return true;
				}
			}
			
			return false;
		}
		#endregion

		#region IDictionary<TKey, TValue> implementation: properties
		public virtual int Count{
			get{return this._keys.Count;}
		}

		public virtual TValue this[TKey key]{
			get{
				if (key == null){
					throw new ArgumentNullException();
				}

				int index = this._keys.BinarySearch(key);
				if (index < 0){
					throw new KeyNotFoundException();
				}

				return this._values[index];
			}
			set{
				if (key == null){
					throw new ArgumentNullException();
				}
				
				int index = this._keys.BinarySearch(key);
				if (index < 0){
					this._keys.Insert(~index, key);
					this._values.Insert(~index, value);
				}else{
					this._values[index] = value;
				}
			}
		}

		public virtual ICollection<TKey> Keys{
			get{
				return new List<TKey>(this._keys);
			}
		}

		public virtual ICollection<TValue> Values{
			get{
				return new List<TValue>(this._values);
			}
		}
		#endregion

		#region IDictionary<TKey, TValue> interfaceimplementation: methods
		public virtual void Add(TKey key, TValue value){
			if (key == null){
				throw new ArgumentNullException();
			}

			int index = this._keys.BinarySearch(key);
			if (index >= 0){
				throw new ArgumentException();
			}

			this._keys.Insert(~index, key);
			this._values.Insert(~index, value);
		}

		public virtual void Clear(){
			this._keys.Clear();
			this._values.Clear();
		}

		public virtual bool ContainsKey(TKey key){
			if (key == null){
				throw new ArgumentNullException();
			}

			return this._keys.BinarySearch(key) >= 0;
		}

		public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator(){
			int count = this.Count;
			
			for (int i = 0; i < count; ++i){
				// Check if the dictionary has been modified since the enumerator was created
				if (count != this.Count){
					throw new InvalidOperationException("Collection was modified.");
				}
				
				yield return new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}

		public virtual bool Remove(TKey key){
			if (key == null){
				throw new ArgumentNullException();
			}

			int index = this._keys.BinarySearch(key);
			if (index < 0){
				return false;
			}

			this._keys.RemoveAt(index);
			this._values.RemoveAt(index);
			return true;
		}

		public virtual bool TryGetValue(TKey key, out TValue value){
			if (key == null){
				throw new ArgumentNullException();
			}
			
			int index = this._keys.BinarySearch(key);
			if (index < 0){
				value = default(TValue);
				return false;
			}

			value = this._values[index];
			return true;
		}
		#endregion

		#region public class methods
		public static bool CanBeSerialized(Type type){
			return 
				type.IsPrimitive ||
				type.IsSerializable || 
				type == typeof(String) ||
				typeof(UnityEngine.Object).IsAssignableFrom(type);
				// TODO: Add Arrays and List of types that can be serialized
		}
		#endregion
	}
}
